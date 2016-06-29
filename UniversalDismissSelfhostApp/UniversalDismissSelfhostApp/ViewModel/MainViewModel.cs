using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalDismissSelfhostApp.Helpers;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace UniversalDismissSelfhostApp.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private bool _isEnabled = false;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        private string _status = "Initializing...";

        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public ObservableCollection<ScheduledToastNotification> ScheduledNotifications { get; private set; } = new ObservableCollection<ScheduledToastNotification>();

        private async void Initialize()
        {
            try
            {
                await BackgroundTaskHelper.RegisterTimeTriggerBackgroundTaskAsync();
            }

            catch (Exception ex)
            {
                Status = "Failed registering background task: " + ex.ToString();
                return;
            }

            try
            {
                ToastHelper.EnsureToastsAreScheduled();
            }

            catch (Exception ex)
            {
                Status = "Failed scheduling toasts: " + ex.ToString();
                return;
            }

            IsEnabled = true;
            Status = "Everything's good! Close the app and go on with life. You'll get reminders every hour from 9-5. Make sure you've installed the app on another device too, so that you can actually test Universal Dismiss.";

            UpdateScheduledNotificationsList();

            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateScheduledNotificationsList();
        }

        public MainViewModel()
        {
            Initialize();
        }

        private void UpdateScheduledNotificationsList()
        {
            try
            {
                ScheduledNotifications.Clear();

                foreach (var n in ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications().OrderBy(i => i.DeliveryTime))
                {
                    ScheduledNotifications.Add(n);
                }
            }

            catch { }
        }

        public void ScheduleToast(DateTime timeToAppearAt)
        {
            try
            {
                timeToAppearAt = new DateTime(timeToAppearAt.Year, timeToAppearAt.Month, timeToAppearAt.Day, timeToAppearAt.Hour, timeToAppearAt.Minute, timeToAppearAt.Second, timeToAppearAt.Kind);

                if (timeToAppearAt.Second % 10 != 0)
                {
                    timeToAppearAt = timeToAppearAt.AddSeconds(10 - (timeToAppearAt.Second % 10));
                }

                ToastHelper.ScheduleToast(timeToAppearAt);
            }

            catch (Exception ex)
            {
                var dontWait = new MessageDialog(ex.ToString(), "Failed to schedule toast").ShowAsync();
            }

            UpdateScheduledNotificationsList();
        }

        private class StopInitializationException : Exception { }
    }
}
