using NotificationsExtensions;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace UniversalDismissSelfhostApp.Helpers
{
    public class ToastHelper
    {
        // Create the toast notifier and store a reference to it
        // Note that for production-quality code, CreateToastNotifier should be inside a try/catch
        // in case the notification platform is unavailable due to critical system-level crashes.
        private static ToastNotifier _toastNotifier = ToastNotificationManager.CreateToastNotifier();

        /// <summary>
        /// Ensures that toasts are scheduled for at least the next 2 days
        /// </summary>
        public static void EnsureToastsAreScheduled()
        {
            // Start with the next even hour time
            DateTime currTimeToAppear = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).AddHours(1);

            // Schedule no more than 2 days worth
            DateTime stopTime = DateTime.Now.AddDays(2);

            for (; currTimeToAppear < stopTime; currTimeToAppear = currTimeToAppear.AddHours(1))
            {
                // If within reasonable hours
                if (IsWithinReminderHours(currTimeToAppear))
                {
                    // Schedule the toast (won't schedule if already scheduled)
                    ScheduleToast(currTimeToAppear);
                }
            }
        }

        /// <summary>
        /// Returns true if the time is within working hours.
        /// </summary>
        /// <param name="time"></param>
        public static bool IsWithinReminderHours(DateTime time)
        {
            return time.TimeOfDay >= TimeSpan.FromHours(9) && time.TimeOfDay <= TimeSpan.FromHours(17);
        }

        public static void ScheduleToast(DateTime timeToAppearAt)
        {
            // If in the past or within 5 seconds, don't schedule
            if (timeToAppearAt <= DateTime.Now.AddSeconds(5))
            {
                throw new ArgumentException("timeToAppearAt must be at least 5 seconds in the future.");
            }

            // Generate a RemoteId for the specified time
            string remoteId = GetRemoteId(timeToAppearAt);

            // If this notification is already scheduled
            if (IsScheduled(timeToAppearAt))
            {
                // Do nothing
                return;
            }

            // Create the notification's content
            XmlDocument toastContent = CreateToastContent(timeToAppearAt, remoteId);

            ScheduledToastNotification notif = new ScheduledToastNotification(toastContent, timeToAppearAt)
            {
                RemoteId = remoteId
            };

            // And schedule the toast
            _toastNotifier.AddToSchedule(notif);
        }

        private static bool IsScheduled(DateTime timeToAppearAt)
        {
            return _toastNotifier.GetScheduledToastNotifications().Any(i => i.DeliveryTime.Ticks == timeToAppearAt.Ticks);
        }

        private static XmlDocument CreateToastContent(DateTime timeToAppearAt, string remoteId)
        {
            ToastContent content = new ToastContent()
            {
                Scenario = ToastScenario.Reminder,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"This is your {timeToAppearAt.ToString("t")} reminder for {timeToAppearAt.ToString("d")}."
                            },

                            new AdaptiveText()
                            {
                                Text = $"RemoteId: {remoteId}"
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastSelectionBox("snoozePicker")
                        {
                            Items =
                            {
                                new ToastSelectionBoxItem("1", "1 minute"),
                                new ToastSelectionBoxItem("5", "5 minutes"),
                                new ToastSelectionBoxItem("15", "15 minutes"),
                                new ToastSelectionBoxItem("60", "1 hour"),
                                new ToastSelectionBoxItem("1440", "1 day")
                            },
                            DefaultSelectionBoxItemId = "5"
                        }
                    },

                    Buttons =
                    {
                        new ToastButtonSnooze()
                        {
                            SelectionBoxId = "snoozePicker"
                        },

                        new ToastButtonDismiss()
                    }
                }
            };

            return content.GetXml();
        }

        private static string GetRemoteId(DateTime timeToAppearAt)
        {
            return timeToAppearAt.Ticks.ToString();
        }
    }
}
