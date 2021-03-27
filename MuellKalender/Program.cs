using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace DennisKae.GoogleCalendarScripts.MuellKalender
{
    /// <summary>
    /// Ist: 
    /// Die Termine des Müllkalenders stehen von 01:00 des Abholtages bis 01:00 am Folgetag im Kalender. 
    /// Die Standard-Erinnerungen des Kalenders werden verwendet (3 Tage, 1 Tag, 2 Stunden vor dem Termin).
    /// 
    /// Soll: 
    /// Die Termine sollen von 00:00 bis 00:00 im Kalender stehen. 
    /// Die Erinnerung soll einmalig 12 Stunden vor dem jeweiligen Termin erfolgen.
    /// </summary>
    public static class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        private static readonly string[] _scopes = { CalendarService.Scope.Calendar };
        private static readonly string _applicationName = "Google Calendar API .NET Quickstart";
        private static readonly string _calendarName = "Feuerwehr Dienstplan";

        public static void Main()
        {
            CalendarService service = GetCalendarService();
            CalendarListEntry calendarListEntry = GetCalendarListEntry(service);
            if (calendarListEntry == null)
            {
                Console.WriteLine("Der Kalender konnte nicht gefunden werden.");
                return;
            }

            // Define parameters of request.
            // Alternative KalenderId für den "Haupt"-Kalender: "primary"
            EventsResource.ListRequest request = service.Events.List(calendarListEntry.Id);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 100;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;


            // List events.
            Events events = request.Execute();
            if (events?.Items?.Any() != true)
            {
                Console.WriteLine("No upcoming events found.");
                return;
            }

            int updateCounter = 0;
            int skippedCounter = 0;
            foreach (Event eventItem in events.Items)
            {
                bool eventWasUpdated = false;
                bool isMuellKalenderEvent = EventIsMuellKalenderEvent(eventItem);

                // Korrigiert den Zeitraum des Events
                if (isMuellKalenderEvent && DateFixIsRequired(eventItem))
                {
                    var oldDateTime = eventItem.Start.DateTime.Value;
                    eventItem.Start.DateTime = new DateTime(oldDateTime.Year, oldDateTime.Month, oldDateTime.Day);
                    eventItem.End.DateTime = eventItem.Start.DateTime.Value.AddDays(1);
                    eventWasUpdated = true;
                }

                // Setzt die Erinnerung auf 12 Stunden vor dem Termin
                if (isMuellKalenderEvent && eventItem.Reminders?.UseDefault != false)
                {
                    if (eventItem.Reminders == null)
                    {
                        eventItem.Reminders = new Event.RemindersData();
                    }
                    eventItem.Reminders.UseDefault = false;
                    eventItem.Reminders.Overrides = new List<EventReminder>();
                    // Reminder per popup (nicht per email) 12 Stunden vor dem Event
                    eventItem.Reminders.Overrides.Add(new EventReminder { Method = "popup", Minutes = 12 * 60 });
                    eventWasUpdated = true;
                }

                if (!eventWasUpdated)
                {
                    skippedCounter++;
                    Console.WriteLine($"Skipped #{skippedCounter}: {eventItem.Summary ?? eventItem.Id} ({eventItem.Start.DateTime})");
                    continue;
                }

                EventsResource.UpdateRequest updateRequest = service.Events.Update(eventItem, calendarListEntry.Id, eventItem.Id);
                Event updateResult = updateRequest.Execute();
                updateCounter++;
                Console.WriteLine($"Updated #{updateCounter}: {eventItem.Summary ?? eventItem.Id} ({eventItem.Start.DateTime})");
            }
        }

        /// <summary>Entscheidet, ob der Zeitraum des Events korrigiert werden muss.</summary>
        private static bool DateFixIsRequired(Event eventItem)
        {
            if (eventItem?.Start?.DateTime == null || eventItem?.End?.DateTime == null)
            {
                return false;
            }

            TimeSpan eventDuration = eventItem.End.DateTime.Value - eventItem.Start.DateTime.Value;

            var result = eventDuration.TotalDays == 24 && eventItem.Start.DateTime.Value.Hour != 0;
            return result;
        }

        /// <summary>Entscheidet, ob die Bezeichnung des Events aus dem Müllkalender ist.</summary>
        private static bool EventIsMuellKalenderEvent(Event eventItem)
        {
            var result = eventItem?.Summary?.EndsWith("Tonne") == true || eventItem?.Summary == "Schadstoffannahme";
            return result;
        }

        private static CalendarListEntry GetCalendarListEntry(CalendarService service)
        {
            CalendarListResource.ListRequest calendarListRequest = service.CalendarList.List();
            CalendarList calendarList = calendarListRequest.Execute();

            CalendarListEntry dienstplan = calendarList.Items.FirstOrDefault(x => x.Summary == _calendarName);
            return dienstplan;
        }

        private static CalendarService GetCalendarService()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
            return service;
        }
    }
}
