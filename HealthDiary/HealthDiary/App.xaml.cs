using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.Model.Services;
using HealthDiary.Services;
using HealthDiary.View;
using HealthDiary.ViewModel;
using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthDiary
{
    public delegate void ToastMessageHandler(string message, int durationMsec);

    public partial class App : Application
    {
        private event ToastMessageHandler ShowToast;

        public static string host = "http://192.168.1.5";
        public static string port = "8301";
        public static Uri connection = new Uri(host + ':' + port + "/api");

        public const string DBFILENAME = "healthdiary.db";
        public readonly static string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DBFILENAME);
        
        public static User CurrentUser { get; set; }

        public static NotyScheduleService NotyScheduleService { get; set; }

        private TimeSpan startNotyTime = new TimeSpan(9, 0, 0);
        private TimeSpan endNotyTime = new TimeSpan(21, 0, 0);
        private TimeSpan eatNotyInterval = new TimeSpan(3, 0, 0);
        private TimeSpan waterNotyInterval = new TimeSpan(1, 0, 0);
        private TimeSpan delayEatBeforeNight = new TimeSpan(2, 0, 0);
        private bool isNotyAfterCompletion = false;

        public App()
        {
            NotyScheduleService = new NotyScheduleService(startNotyTime, endNotyTime, eatNotyInterval, waterNotyInterval, delayEatBeforeNight, isNotyAfterCompletion);

            InitializeComponent();

            //TODO: add name constant
            bool is_sync = Preferences.Get("synchronized_perm_data", false);
            if (is_sync)
                Login();
            else
            {
                ShowLoading();
                SyncPermanentData();
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void Login()
        {
            //TODO: add name constant
            if (App.Current.Properties.TryGetValue("current_user_id", out object userid) && userid is int)
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    CurrentUser = db.Users.Include(x => x.Gender).Include(x => x.PhysicalActivityType).Include(x => x.Plan).
                    Include(x => x.Products).Include(x => x.Dishes).Include(x => x.Cups).Include(x => x.PlanCompletions).
                    Where(x => x.Id == (int)userid).FirstOrDefault();
                }
                if (CurrentUser != null)
                {
                    if (CurrentUser.Plan != null)
                        MainPage = new NavigationPage(new MainPage());
                    else
                        MainPage = new NavigationPage(new UserInfoPage());
                }
                else
                    MainPage = new NavigationPage(new LoginPage(true));
            }
            else
            {
                bool islogin = false;
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    if (db.Users.Count() > 0)
                        islogin = true;
                }
                MainPage = new NavigationPage(new LoginPage(islogin));
            }
        }

        public static void ScheduleNotifications(PlanCompletion planCompletion)
        {
            bool is_cancel_closer_eat = false;
            bool is_cancel_closer_water = false;
            if (App.Current.Properties.TryGetValue(App.CurrentUser.Id + "_last_eat", out object last_eat) && DateTime.TryParse(last_eat.ToString(), out DateTime eat_date))
            {
                TimeSpan duration = (DateTime.Now - eat_date).Duration();
                if (duration.TotalHours < 1)
                    is_cancel_closer_eat = true;
            }
            if (App.Current.Properties.TryGetValue(App.CurrentUser.Id + "_last_drink", out object last_drink) && DateTime.TryParse(last_drink.ToString(), out DateTime drink_date))
            {
                TimeSpan duration = (DateTime.Now - drink_date).Duration();
                if (duration.TotalMinutes < 20)
                    is_cancel_closer_water = true;
            }
            App.NotyScheduleService.ScheduleNotifications(planCompletion, is_cancel_closer_eat, is_cancel_closer_water);
        }

        private void ShowLoading()
        {
            var page = new LoadingPage();
            this.ShowToast += page.DisplayToastMessage;
            MainPage = page;
        }

        private async void SyncPermanentData()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                if (await SyncProducts() && SyncCups() && await SyncGenders() && await SyncPhysicalActivityTypes())
                {
                    //TODO: add name constant
                    Preferences.Set("synchronized_perm_data", true);
                    Login();
                }
            }
            else
                this.ShowToast?.Invoke("Network connection failed, for first start app needs to sync data with server, please connect your device to network", 5000);
        }

        private bool SyncCups()
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.Cups.RemoveRange(db.Cups);
                    db.SaveChanges();
                }

                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.Cups.Add(new Model.Cup() { WaterAmount = 100, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 125, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 150, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 175, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 200, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 250, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 300, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 350, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 400, IsReadonly = true });
                    db.Cups.Add(new Model.Cup() { WaterAmount = 500, IsReadonly = true });

                    db.SaveChanges();
                }
                return true;
            }
            catch
            {
                this.ShowToast?.Invoke("Failed to save cups", 3000);
                return false;
            }
        }

        private async Task<bool> SyncProducts()
        {
            if (await SyncUnits())
            {
                SynchronizerResponse response = await HttpSynchronizer.InitProducts(connection, dbPath);
                switch (response)
                {
                    case SynchronizerResponse.Success:
                        return true;
                    case SynchronizerResponse.NetworkError:
                        this.ShowToast?.Invoke("Failed to sync products - network error", 3000);
                        return false;
                    case SynchronizerResponse.ServerError:
                        this.ShowToast?.Invoke("Failed to sync products - internal server error", 3000);
                        return false;
                    default:
                        this.ShowToast?.Invoke("Failed to sync products", 3000);
                        return false;
                }
            }
            else
                return false;
        }

        private async Task<bool> SyncUnits()
        {
            SynchronizerResponse response = await HttpSynchronizer.InitUnits(connection, dbPath);
            switch (response)
            {
                case SynchronizerResponse.Success:
                    return true;
                case SynchronizerResponse.NetworkError:
                    this.ShowToast?.Invoke("Failed to sync units - network error", 3000);
                    return false;
                case SynchronizerResponse.ServerError:
                    this.ShowToast?.Invoke("Failed to sync units - internal server error", 3000);
                    return false;
                default:
                    this.ShowToast?.Invoke("Failed to sync units", 3000);
                    return false;
            }
        }

        private async Task<bool> SyncGenders()
        {
            SynchronizerResponse response = await HttpSynchronizer.InitGenders(connection, dbPath);
            switch (response)
            {
                case SynchronizerResponse.Success:
                    return true;
                case SynchronizerResponse.NetworkError:
                    this.ShowToast?.Invoke("Failed to sync genders - network error", 3000);
                    return false;
                case SynchronizerResponse.ServerError:
                    this.ShowToast?.Invoke("Failed to sync genders - internal server error", 3000);
                    return false;
                default:
                    this.ShowToast?.Invoke("Failed to sync genders", 3000);
                    return false;
            }
        }

        private async Task<bool> SyncPhysicalActivityTypes()
        {
            SynchronizerResponse response = await HttpSynchronizer.InitPhysicalActivityTypes(connection, dbPath);
            switch (response)
            {
                case SynchronizerResponse.Success:
                    return true;
                case SynchronizerResponse.NetworkError:
                    this.ShowToast?.Invoke("Failed to sync physical activity types - network error", 3000);
                    return false;
                case SynchronizerResponse.ServerError:
                    this.ShowToast?.Invoke("Failed to sync physical activity types - internal server error", 3000);
                    return false;
                default:
                    this.ShowToast?.Invoke("Failed to sync physical activity types", 3000);
                    return false;
            }
        }
    }
}
