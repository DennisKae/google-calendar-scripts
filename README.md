# google-calendar-scripts

Sammlung verschiedener C# Scripts für die Google Calendar API v3.

### Voraussetzungen

0. net core 3.1 oder neuer
1. [Google Calendar Api aktivieren](https://developers.google.com/calendar/quickstart/dotnet)
2. API Credentials als `credentials.json` im Projekt (neben der .csproj) ablegen
3. `credentials.json` --> *properties* --> *copy to output directory: **copy always***

### Weitere Infos

- [Google.Apis.Calendar.v3 nuget Paket](https://www.nuget.org/packages/Google.Apis.Calendar.v3/)
  - [GitHub API Clients for .NET Repository](https://github.com/googleapis/google-api-dotnet-client)
- [Google Calendar API - Homepage](https://developers.google.com/calendar)
- [Google Calendar API - .NET Quickstart](https://developers.google.com/calendar/quickstart/dotnet)
- [Google Calendar API Namespace Reference](https://googleapis.dev/dotnet/Google.Apis.Calendar.v3/latest/api/Google.Apis.Calendar.v3.Data.html)
- [Google Calendar API Namespace Reference 2](https://developers.google.com/resources/api-libraries/documentation/calendar/v3/csharp/latest/namespaceGoogle_1_1Apis_1_1Calendar_1_1v3.html)

## Müllkalender

**Problem:**

- Die Termine des Müllkalenders stehen von 01:00 des Abholtages bis 01:00 am Folgetag im Kalender.
- Die Standard-Erinnerungen des Kalenders werden verwendet (3 Tage, 1 Tag, 2 Stunden vor dem Termin).

**Ziel:**

- Die Termine sollen von 00:00 bis 00:00 im Kalender stehen.
- Die Erinnerung soll einmalig 12 Stunden vor dem jeweiligen Termin erfolgen.
