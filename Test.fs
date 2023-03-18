namespace primordiallycash

module Test =

  open System
  open Agenda
  open NUnit.Framework

  let today = DateTime.Today |> DateOnly.FromDateTime
  let firstDayOfMonth = today.AddDays(1 - today.Day)
  let lastDayOfTheMonth = firstDayOfMonth.AddMonths(1).AddDays(-1)
  let thisMonth = dateSpan firstDayOfMonth lastDayOfTheMonth

  [<Test>]
  let ``Everyday and Never work the same regardles of date`` () =
    let testAppointments d =
      (seq {"never"; "Never"; "NEVER"; "nEvEr"},
       seq {"everyday"; "Everyday"; "EVERYDAY"; "eVeRyDaY"})
      ||> Seq.iter2 (fun n e ->
                      match (Appointment.parse n, Appointment.parse e) with
                      | (Ok n, Ok e) -> (Assert.False(Appointment.check n d); Assert.True(Appointment.check e d))
                      | _ -> Assert.Fail())
    Seq.iter testAppointments thisMonth

  [<Test>]
  let ``Montly will only pick a single value per month`` () =
      match Appointment.parse (today.Day.ToString()) with
      | Ok ns -> (let selected = thisMonth |> Seq.filter (Appointment.check ns)
                  Assert.AreEqual(1, Seq.length selected)
                  Assert.AreEqual(today.Day, (Seq.exactlyOne selected).Day))
      | _ -> Assert.Fail()

  [<Test>]
  let ``A single day in a week will only pick a single value per week`` () =
      match Appointment.parse (today.DayOfWeek.ToString()) with
      | Ok ns -> let selected = seq { for i in 0 .. 6 -> today.AddDays(i) }
                                |> Seq.filter (Appointment.check ns)
                 Assert.AreEqual(1, Seq.length selected)
                 Assert.AreEqual(today.DayOfWeek, (Seq.exactlyOne selected).DayOfWeek)
      | _ -> Assert.Fail()

  [<Test>]
  let ``Check full month for arbitrary schedule`` () =
      let march2023 = (DateOnly.Parse("03/01/2023"), DateOnly.Parse("03/31/2023"))
                      ||> dateSpan

      let expected = seq { set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "alice"; Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"]
                           set [Name "bob"]
                           set [Name "bob"; Name "yuri"] }


      match __SOURCE_DIRECTORY__ + "/users.csv" |> Schedule.readCsv |> Schedule.create with
      | Ok sch ->
        let marchAgenda = march2023
                          |> Seq.map (Schedule.selectUsers sch)
        (Seq.zip march2023 expected, marchAgenda)
        ||> Seq.iter2 (fun e r -> Assert.AreEqual(e, r))
      | e -> Assert.Fail()
