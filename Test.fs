namespace primordiallycash

module Test =

  open System
  open Scheduler
  open NUnit.Framework

  let today = DateTime.Today |> DateOnly.FromDateTime
  let firstDayOfMonth = today.AddDays(1 - today.Day)
  let month = Seq.initInfinite (fun i -> firstDayOfMonth.AddDays(i))
              |> Seq.takeWhile (fun (d: DateOnly) -> d.Month = firstDayOfMonth.Month)

  [<Test>]
  let ``Everyday and Never work the same regardles of date`` () =
    let testAppointments d =
      (seq {"never"; "Never"; "NEVER"; "nEvEr"},
       seq {"everyday"; "Everyday"; "EVERYDAY"; "eVeRyDaY"})
      ||> Seq.iter2 (fun n e ->
                      match (parse n, parse e) with
                      | (Ok n, Ok e) -> (Assert.False(check1 n d); Assert.True(check1 e d))
                      | _ -> Assert.Fail())
    Seq.iter testAppointments month

  [<Test>]
  let ``Montly will only pick a single value per month`` () =
      match parse (today.Day.ToString()) with
      | Ok ns -> (let selected = month |> Seq.filter (check1 ns)
                  Assert.AreEqual(1, Seq.length selected)
                  Assert.AreEqual(today.Day, (Seq.exactlyOne selected).Day))
      | _ -> Assert.Fail()

  [<Test>]
  let ``A single day in a week will only pick a single value per week`` () =
      match parse (today.DayOfWeek.ToString()) with
      | Ok ns -> let selected = seq { for i in 0 .. 6 -> today.AddDays(i) }
                                |> Seq.filter (check1 ns)
                 Assert.AreEqual(1, Seq.length selected)
                 Assert.AreEqual(today.DayOfWeek, (Seq.exactlyOne selected).DayOfWeek)
      | _ -> Assert.Fail()