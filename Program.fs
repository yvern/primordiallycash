namespace primordiallycash

module Cli =

  open System
  open System.IO
  open Scheduler
  open FSharp.Data

  let help () = printfn "help" ; 1

  [<Literal>]
  let ResolutionFolder = __SOURCE_DIRECTORY__
  type Users = CsvProvider<"users.csv", ResolutionFolder=ResolutionFolder>

  let readCsv (filename: string) =
    let path = Path.GetFullPath(filename)
    if not (File.Exists(path))
    then Error [|(0, sprintf "File does not exist: %s" path)|]
    else Users.Load(path).Rows
         |> Seq.mapi (fun i row ->
                        parse row.Appointments
                        |> Result.map (fun ap -> Name row.Name, ap)
                        |> Result.mapError (fun e -> i + 1, e))
         |> Array.ofSeq
         |> Ok

  let main filename from until =
    match filename |> readCsv |> createSchedule with
    | Error errs ->
      printfn "Errors Found!"
      Array.iter (fun (l, e) -> printfn "%s:%i - %s" filename l e) errs
      1
    | Ok sch ->
      generateDateSeq from until
      |> Seq.iter (fun d ->
                    (selectUsers sch d)
                    |> Seq.map (fun (Name u) -> u)
                    |> String.concat ", "
                    |> printfn "%s: %s" (d.ToString()))
      0

  [<EntryPoint>]
  let cli args =
    let today = DateTime.Today |> DateOnly.FromDateTime
    let firstDayOfMonth = today.AddDays(1 - today.Day)
    let lastDayOfTheMonth = firstDayOfMonth.AddMonths(1).AddDays(-1)
    let mutable from = today
    let mutable until = from.AddMonths(1)
    match args with
    | [| filename |] -> main filename firstDayOfMonth lastDayOfTheMonth
    | [| filename ; untilArg |] ->
      if DateOnly.TryParse(untilArg, &until)
      then main filename today until
      else printfn "Error parsing date: %s" untilArg ; 1
    | [| filename ; fromArg ; untilArg |] ->
      if DateOnly.TryParse(fromArg, &from) && DateOnly.TryParse(untilArg, &until)
      then main filename from until
      else printfn "Error parsing dates: %s %s" fromArg untilArg ; 1
    | _ -> help()