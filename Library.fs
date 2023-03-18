namespace primordiallycash

open System

type User = Name of string

type Agenda = (DateOnly * Set<User>) seq
module Agenda =

  let capitalize (d: string) =
    let ld = d.Trim().ToLower()
    let c = ld.Substring(0, 1).ToUpper()
    c + ld.Substring(1)

  let dateSpan (from: DateOnly) (until: DateOnly) =
    Seq.unfold (fun d -> if (until >= d)
                            then Some (d, d.AddDays(1))
                            else None)
               from

  [<RequireQualifiedAccess>]
  module Agenda =
    let private fmtUsers (d, users) =
      users |> Seq.map (fun (Name u) -> u)
            |> String.concat ", "
            |> printfn "%s: %s" (d.ToString())

    let show (agenda: Agenda)=
      agenda |> Seq.iter fmtUsers

  type Appointment =
    | Monthly of int
    | ByWeekday of Set<DayOfWeek>
    | Everyday
    | Never

  [<RequireQualifiedAccess>]
  module Appointment =

    let (|ValidMonthly|_|) (s : string) =
      let mutable m = 0
      if Int32.TryParse(s, &m) && m > 0 && m < 29
      then Some (Monthly m)
      else None

    let week = typeof<DayOfWeek>.GetEnumNames() |> set in
    let (|ValidByWeekday|_|) (s: string) =
      let wk = s.Split [|'-'|] |> Array.map capitalize
      if Array.exists (fun d -> not (Set.contains d week)) wk
      then None
      else wk
        |> Array.map DayOfWeek.Parse
        |> Set.ofArray
        |> ByWeekday
        |> Some

    let (|ValidNever|_|) (s: string) =
      if s.ToLower() = "never"
      then Some Never
      else None

    let (|ValidEveryday|_|) (s: string) =
      if s.ToLower() = "everyday"
      then Some Everyday
      else None

    let parse =
      function
      | ValidMonthly day -> Ok day
      | ValidByWeekday wds -> Ok wds
      | ValidEveryday everyday -> Ok everyday
      | ValidNever never -> Ok never
      | err -> err |> sprintf "%s did not conform to any valid format!" |> Error

    let check appointment (d: DateOnly) =
      match appointment with
      | Monthly md when d.Day = md -> true
      | ByWeekday wd when Set.contains d.DayOfWeek wd -> true
      | Everyday -> true
      | Never | _ -> false

  type Schedule = Map<User, Appointment>

  [<RequireQualifiedAccess>]
  module Schedule =

    open System.IO
    open FSharp.Data

    [<Literal>]
    let ResolutionFolder = __SOURCE_DIRECTORY__
    type RawSchedule = CsvProvider<"users.csv", ResolutionFolder=ResolutionFolder>

    let readCsv (filename: string) =
      let path = Path.GetFullPath(filename)
      if not (File.Exists(path))
      then Error [|sprintf "File does not exist: %s" path|]
      else RawSchedule.Load(path).Rows
           |> Seq.mapi (fun i row ->
                          Appointment.parse row.Appointments
                          |> Result.map (fun ap -> Name row.Name, ap)
                          |> Result.mapError (sprintf "%s:%i - %s" filename (i+1)))
           |> Array.ofSeq
           |> Ok

    let create appointments =
      appointments
      |> Result.map (Array.partition Result.isOk)
      |> Result.bind (fun (users, errors) ->
                        if not (Array.isEmpty errors)
                        then errors
                             |> Array.choose (function Ok _ -> None | Error e -> Some e)
                             |> Error
                        else users
                             |> Array.choose Result.toOption
                             |> Map.ofArray
                             |> Ok)

    let selectUsers (sch: Schedule) (d: DateOnly) : DateOnly* Set<User>=
      d, sch
         |> Map.filter (fun _ ap -> Appointment.check ap d)
         |> Map.keys
         |> set

    let toAgenda (sch: Schedule) (dates: DateOnly seq) : Agenda =
      Seq.map (selectUsers sch) dates
