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
    [BroadcastReceiver]
    public class NotyReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            PowerManager.WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "NotyReceiver");
            wakeLock.Acquire();

            var noty = new NotificationRequest
            {
                BadgeNumber = 1,
                Description = "Test noty",
                Title = "TEST NOTY",
                ReturningData = "test data from noty",
                NotificationId = 100,
                NotifyTime = DateTime.Now.AddSeconds(10)
            };

            NotificationCenter.Current.Show(noty);

            wakeLock.Release();
        }
    }
}