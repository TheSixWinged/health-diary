using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthDiary.Droid
{
    [Service]
    public class NotyService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var noty = new NotificationRequest
            {
                BadgeNumber = 1,
                Description = "Test noty",
                Title = "TEST NOTY",
                ReturningData = "test data from noty",
                NotificationId = 100,
                NotifyTime = DateTime.Now.AddSeconds(10)
            };

            var noty2 = new NotificationRequest
            {
                BadgeNumber = 2,
                Description = "Test noty 2",
                Title = "TEST NOTY 2",
                ReturningData = "test data from noty 2",
                NotificationId = 101,
                NotifyTime = DateTime.Now.AddSeconds(30)
            };

            NotificationCenter.Current.Show(noty);
            NotificationCenter.Current.Show(noty2);
            //NotificationCenter.Current.CancelAll();

            return StartCommandResult.NotSticky;
        }
    }
}