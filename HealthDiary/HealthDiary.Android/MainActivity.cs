using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Plugin.LocalNotification;
using Android.Content;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android;
using Android.Widget;

namespace HealthDiary.Droid
{
    [Activity(Label = "HealthDiary", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            //for android API level >= 26
            NotificationCenter.CreateNotificationChannel();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            
            //alarm
            /*var notyIntent = new Intent(this, typeof(NotyReceiver));
            var pending = PendingIntent.GetBroadcast(this, 0, notyIntent, PendingIntentFlags.UpdateCurrent);
            var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
            alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 5 * 1000, pending);*/

            //service
            /*var intent = new Intent(this, typeof(NotyService));
            StartService(intent);*/

            //permissions
            /*ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReceiveBootCompleted }, 1);
            var Grant = ContextCompat.CheckSelfPermission(Android.App.Application.Context, Manifest.Permission.ReceiveBootCompleted);*/
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}