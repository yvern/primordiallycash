# primordiallycash

## A simple agenda builder for a technical challenge homework

### Definitions

* Appointment: A specifier of date/frequency of the event happening. Used to know whether the event will happen given a date. This can be:
  * Monthly: A single day in each month
  * ByWeekDay: A set of days in every week
  * Everyday: Every single day
  * Never: No day matches

* User: solely a name for now.

* Schedule: A mapping from Users to their Appointments.

* Agenda: A sequence of pairings of dates and the set of Users that have an appointment for that day.

### Spec

As input, the core library expects a csv file in the same shape as the `users.csv` one, which it will interpret as a Schedule.
It will then build an Agenda, and display it.

If one runs the application from the commandline, with `dotnet run -- users.csv`, one would expect something similar to:

```plain
3/1/2023: bob, yuri
3/2/2023: bob
3/3/2023: bob, yuri
3/4/2023: bob
3/5/2023: bob
3/6/2023: bob
3/7/2023: bob
3/8/2023: bob, yuri
3/9/2023: bob
3/10/2023: bob, yuri
3/11/2023: bob
3/12/2023: bob
3/13/2023: bob
3/14/2023: bob
3/15/2023: alice, bob, yuri
3/16/2023: bob
3/17/2023: bob, yuri
3/18/2023: bob
3/19/2023: bob
3/20/2023: bob
3/21/2023: bob
3/22/2023: bob, yuri
3/23/2023: bob
3/24/2023: bob, yuri
3/25/2023: bob
3/26/2023: bob
3/27/2023: bob
3/28/2023: bob
3/29/2023: bob, yuri
3/30/2023: bob
3/31/2023: bob, yuri
```

### Packaging

A single file executable artifact can be generated with `make publish`.
This still requires a dotnet sdk to be built and a dotnet runtime installed and available to be executed.

A Dockerfile is present for containerization, and one can create an image with `make build-img`.
This command enables anyone without any dotnet sdk or runtimes to still be able to build and run the image.
It is also possible to pass a desired Schedule to be bundled into the image with `make build-img schedule=your-schedule.csv`, by default the `users.csv` is bundled.
