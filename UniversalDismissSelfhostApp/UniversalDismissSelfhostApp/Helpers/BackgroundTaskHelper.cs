using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace UniversalDismissSelfhostApp.Helpers
{
    public static class BackgroundTaskHelper
    {
        public const string TIME_TRIGGER_NAME = "TimeTrigger";

        public static async Task RegisterTimeTriggerBackgroundTaskAsync()
        {
            var accessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (accessStatus.HasFlag(BackgroundAccessStatus.Denied))
            {
                throw new Exception("Background access has been denied. Please allow background access.");
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = TIME_TRIGGER_NAME
            };

            builder.SetTrigger(new TimeTrigger(60, false));

            builder.Register();
        }
    }
}
