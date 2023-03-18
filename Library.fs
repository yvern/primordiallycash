namespace primordiallycash

module Scheduler =

  open System

  type Appointment =
    | Monthly of int
    | ByWeekday of Set<DayOfWeek>
    | Everyday
    | Never

  type User = Name of string
  type Schedule = Map<User, Appointment>

  let generateDateSeq (from: DateOnly) (until: DateOnly) =
    Seq.unfold (fun d -> if (until >= d)
                            then Some (d, d.AddDays(1))
                            else None)
               from

  let (|ValidMonthly|_|) (s : string) =
    let mutable m = 0
    if Int32.TryParse(s, &m) && m > 0 && m < 29
    then Some (Monthly m)
    else None

  let capitalize (d: string) =
    let ld = d.Trim().ToLower()
    let c = ld.Substring(0, 1).ToUpper()
    c + ld.Substring(1)

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

  let createSchedule appointments =
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

  let check1 appointment (d: DateOnly) =
    match appointment with
    | Monthly md when d.Day = md -> true
    | ByWeekday wd when Set.contains d.DayOfWeek wd -> true
    | Everyday -> true
    | Never | _ -> false

  let selectUsers (sch: Schedule) (d: DateOnly) : Set<User>=
    sch
    |> Map.filter (fun _ ap -> check1 ap d)
    |> Map.keys
    |> set

  module IO =

    open System.IO
    open FSharp.Data

    [<Literal>]
    let ResolutionFolder = __SOURCE_DIRECTORY__
    type UsersSchedule = CsvProvider<"users.csv", ResolutionFolder=ResolutionFolder>

    let readCsv (filename: string) =
      let path = Path.GetFullPath(filename)
      if not (File.Exists(path))
      then Error [|sprintf "File does not exist: %s" path|]
      else UsersSchedule.Load(path).Rows
           |> Seq.mapi (fun i row ->
                          parse row.Appointments
                          |> Result.map (fun ap -> Name row.Name, ap)
                          |> Result.mapError (sprintf "%s:%i - %s" filename (i+1)))
           |> Array.ofSeq
           |> Ok

    let fmtScheduleForDate d =
      Seq.map (fun (Name u) -> u)
      >> String.concat ", "
      >> printfn "%s: %s" (d.ToString())

