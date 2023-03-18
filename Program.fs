namespace primordiallycash

module Cli =

  open System
  open Agenda

  let help () =
    printfn "Please provide a csv file as argument. Usage:"
    printfn " | primordiallycash schedule.csv -> print out the agenda for the current month"
    printfn " | primordiallycash schedule.csv until -> print out the agenda from today until given date"
    printfn " | primordiallycash schedule.csv from until -> print out the agenda for the give timespan"
    1

  let main filename from until =
    match filename |> Schedule.readCsv |> Schedule.create with
    | Error errs -> Array.iter (printfn "%s") errs
                    1
    | Ok sch -> dateSpan from until
                |> Schedule.toAgenda sch
                |> Agenda.show
                0

  [<EntryPoint>]
  let cli args =
    let today = DateTime.Today |> DateOnly.FromDateTime
    let firstDayOfMonth = today.AddDays(1 - today.Day)
    let lastDayOfTheMonth = firstDayOfMonth.AddMonths(1).AddDays(-1)
    let mutable from = today
    let mutable until = from.AddMonths(1)
    match args with
    | [||] | [| "help" |] -> help ()
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
    | _ -> help ()