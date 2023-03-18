namespace primordiallycash

module Cli =

  open System
  open Scheduler
  open Scheduler.IO

  let help () = printfn "help" ; 1

  let main filename from until =
    match filename |> readCsv |> createSchedule with
    | Error errs -> Array.iter (printfn "%s") errs
                    1
    | Ok sch -> generateDateSeq from until
                |> Seq.iter (fun d ->
                              (selectUsers sch d)
                              |> fmtScheduleForDate d)
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
      if DateOnly.TryParse(fromArg, &from)
         && DateOnly.TryParse(untilArg, &until)
      then main filename from until
      else printfn "Error parsing dates: %s %s" fromArg untilArg ; 1
    | _ -> help()