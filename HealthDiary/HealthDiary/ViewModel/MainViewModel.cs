using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.Model.Extensions;
using HealthDiary.Model.Services;
using HealthDiary.View;
using HealthDiary.ViewModel.Templates;
using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HealthDiary.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public event ToastMessageHandler ShowToast;

        public Plan Plan { get; private set; }
        public PlanCompletion PlanCompletion { get; set; }
        public ICommand OpenProductList_cmd { protected set; get; }
        public ICommand OpenCupList_cmd { protected set; get; }
        public ICommand EatProduct_cmd { protected set; get; }
        public ICommand EatDish_cmd { protected set; get; }
        public ICommand DrinkWater_cmd { protected set; get; }
        public ICommand OpenProgress_cmd { protected set; get; }
        public ICommand OpenSettings_cmd { protected set; get; }

        public MainViewModel(INavigation navigation) : base(navigation)
        {
            this.Plan = App.CurrentUser.Plan ?? Plan.StandartPlan;

            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                DateTime today = DateTime.Today.Date;
                var plan_completion = db.PlanCompletions.Include(x => x.User).Where(x => x.UserId == App.CurrentUser.Id && x.Date.Date == today).FirstOrDefault();
                if (plan_completion != null)
                    this.PlanCompletion = plan_completion;
                else
                {
                    this.PlanCompletion = new PlanCompletion()
                    {
                        Date = today,
                        User = App.CurrentUser,
                        CaloriesPlan = Plan.Calories,
                        ProteinsPlan = Plan.Proteins,
                        FatsPlan = Plan.Fats,
                        CarbohydratesPlan = Plan.Carbohydrates,
                        WaterAmountPlan = Plan.WaterAmount
                    };
                    db.Users.Attach(App.CurrentUser);
                    db.PlanCompletions.Add(PlanCompletion);
                    db.SaveChanges();
                }
            }

            //this uses for shedule plan after login/registration
            if (!App.Current.Properties.TryGetValue("is_noty_scheduled", out object first_start) || (App.Current.Properties.TryGetValue("is_noty_scheduled", out object is_noty_scheduled) && is_noty_scheduled is bool && !(bool)is_noty_scheduled))
            {
                App.ScheduleNotifications(this.PlanCompletion);
                App.Current.Properties["is_noty_scheduled"] = true;
            }

            this.OpenProductList_cmd = new Command(OpenProductList);
            this.OpenCupList_cmd = new Command(OpenCupList);
            this.EatProduct_cmd = new Command(EatProduct);
            this.EatDish_cmd = new Command(EatDish);
            this.DrinkWater_cmd = new Command(DrinkWater);
            this.OpenProgress_cmd = new Command(OpenProgress);
            this.OpenSettings_cmd = new Command(OpenSettings);

            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                //now this reset current cup to default where we change users
                //TODO: add name constant
                if (App.Current.Properties.TryGetValue("current_cup", out object cup) && cup is double)
                    this.CurrentCup = db.Cups.Where(x => (x.User == null || x.UserId == App.CurrentUser.Id) && x.WaterAmount == (double)cup).FirstOrDefault() ?? db.Cups.Where(x => x.User == null).FirstOrDefault();
                else
                    this.CurrentCup = db.Cups.Where(x => x.User == null).FirstOrDefault();
            }
            IsDrinkWaterEnabled = true;

            new Thread(() => SyncPlanCompletions()).Start();
            //SyncPlanCompletions();
        }

        public double Calories
        {
            get { return PlanCompletion.Calories; }
            set
            {
                PlanCompletion.Calories = value;
                OnPropertyChanged("Calories");
                OnPropertyChanged("CaloriesPresentation");
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    db.PlanCompletions.Update(PlanCompletion);
                    db.SaveChanges();
                }
            }
        }

        public double CaloriesPlan
        {
            get { return PlanCompletion.CaloriesPlan; }
            set
            {
                PlanCompletion.CaloriesPlan = value;
                OnPropertyChanged("CaloriesPlan");
                OnPropertyChanged("CaloriesPresentation");
            }
        }

        public string CaloriesPresentation
        {
            get { return $"{PlanCompletion.Calories.RoundToSignDigits(2)} / {PlanCompletion.CaloriesPlan.RoundToSignDigits(2)}"
                    + "   "
                    + $"{(PlanCompletion.Calories / PlanCompletion.CaloriesPlan * 100).RoundToSignDigits(2)}%"; }
            set { }
        }

        public double Proteins
        {
            get { return PlanCompletion.Proteins; }
            set
            {
                PlanCompletion.Proteins = value;
                OnPropertyChanged("Proteins");
                OnPropertyChanged("ProteinsPresentation");
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    db.PlanCompletions.Update(PlanCompletion);
                    db.SaveChanges();
                }
            }
        }

        public double ProteinsPlan
        {
            get { return PlanCompletion.ProteinsPlan; }
            set
            {
                PlanCompletion.ProteinsPlan = value;
                OnPropertyChanged("ProteinsPlan");
                OnPropertyChanged("ProteinsPresentation");
            }
        }

        public string ProteinsPresentation
        {
            get
            {
                return $"{PlanCompletion.Proteins.RoundToSignDigits(2)} / {PlanCompletion.ProteinsPlan.RoundToSignDigits(2)}"
                  + "   "
                  + $"{(PlanCompletion.Proteins / PlanCompletion.ProteinsPlan * 100).RoundToSignDigits(2)}%";
            }
            set { }
        }

        public double Fats
        {
            get { return PlanCompletion.Fats; }
            set
            {
                PlanCompletion.Fats = value;
                OnPropertyChanged("Fats");
                OnPropertyChanged("FatsPresentation");
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    db.PlanCompletions.Update(PlanCompletion);
                    db.SaveChanges();
                }
            }
        }

        public double FatsPlan
        {
            get { return PlanCompletion.FatsPlan; }
            set
            {
                PlanCompletion.FatsPlan = value;
                OnPropertyChanged("FatsPlan");
                OnPropertyChanged("FatsPresentation");
            }
        }

        public string FatsPresentation
        {
            get
            {
                return $"{PlanCompletion.Fats.RoundToSignDigits(2)} / {PlanCompletion.FatsPlan.RoundToSignDigits(2)}"
                  + "   "
                  + $"{(PlanCompletion.Fats / PlanCompletion.FatsPlan * 100).RoundToSignDigits(2)}%";
            }
            set { }
        }

        public double Carbohydrates
        {
            get { return PlanCompletion.Carbohydrates; }
            set
            {
                PlanCompletion.Carbohydrates = value;
                OnPropertyChanged("Carbohydrates");
                OnPropertyChanged("CarbohydratesPresentation");
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    db.PlanCompletions.Update(PlanCompletion);
                    db.SaveChanges();
                }
            }
        }

        public double CarbohydratesPlan
        {
            get { return PlanCompletion.CarbohydratesPlan; }
            set
            {
                PlanCompletion.CarbohydratesPlan = value;
                OnPropertyChanged("CarbohydratesPlan");
                OnPropertyChanged("CarbohydratesPresentation");
            }
        }

        public string CarbohydratesPresentation
        {
            get
            {
                return $"{PlanCompletion.Carbohydrates.RoundToSignDigits(2)} / {PlanCompletion.CarbohydratesPlan.RoundToSignDigits(2)}"
                  + "   "
                  + $"{(PlanCompletion.Carbohydrates / PlanCompletion.CarbohydratesPlan * 100).RoundToSignDigits(2)}%";
            }
            set { }
        }

        public double WaterAmount
        {
            get { return PlanCompletion.WaterAmount; }
            set
            {
                PlanCompletion.WaterAmount = value;
                OnPropertyChanged("WaterAmount");
                OnPropertyChanged("WaterAmountPresentation");
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    db.PlanCompletions.Update(PlanCompletion);
                    db.SaveChanges();
                }
            }
        }

        public double WaterAmountPlan
        {
            get { return PlanCompletion.WaterAmountPlan; }
            set
            {
                PlanCompletion.WaterAmountPlan = value;
                OnPropertyChanged("WaterAmountPlan");
                OnPropertyChanged("WaterAmountPresentation");
            }
        }

        public string WaterAmountPresentation
        {
            get
            {
                return $"{PlanCompletion.WaterAmount.RoundToSignDigits(2)} / {PlanCompletion.WaterAmountPlan.RoundToSignDigits(2)}"
                  + "   "
                  + $"{(PlanCompletion.WaterAmount / PlanCompletion.WaterAmountPlan * 100).RoundToSignDigits(2)}%";
            }
            set { }
        }

        private Cup currentCup { get; set; } //TODO: del this?
        public Cup CurrentCup
        {
            get { return currentCup; }
            set
            {
                currentCup = value;
                //TODO: add name constant
                App.Current.Properties["current_cup"] = value.WaterAmount;
                OnPropertyChanged("CurrentCup");
            }
        }

        private bool isDrinkWaterEnabled;
        public bool IsDrinkWaterEnabled
        {
            get { return isDrinkWaterEnabled; }
            set
            {
                isDrinkWaterEnabled = value;
                OnPropertyChanged("IsDrinkWaterEnabled");
            }
        }

        private void OpenProductList()
        {
            //Navigation.PushAsync(new ProductListPage(/*new ProductListViewModel(Navigation)*/));

            //Navigation.PushAsync(new ProductAndDishListPage());

            ProductAndDishListPage page = new ProductAndDishListPage();
            ProductListViewModel plist_vm = new ProductListViewModel(Navigation);
            DishListViewModel dlist_vm = new DishListViewModel(Navigation);
            plist_vm.ProductListChanged += dlist_vm.RefreshList;
            ProductListPage plist_page = new ProductListPage(plist_vm);
            DishListPage dlist_page = new DishListPage(dlist_vm);
            page.Children.Add(plist_page);
            page.Children.Add(dlist_page);
            Navigation.PushAsync(page);
        }

        private void OpenCupList()
        {
            Navigation.PushModalAsync(new CupListPage(new CupListViewModel(Navigation, this)));
        }

        private void EatProduct()
        {
            Navigation.PushModalAsync(new ProductChoicePage(new ProductChoiceViewModel(Navigation, this)));
        }

        private void EatDish()
        {
            Navigation.PushModalAsync(new DishChoicePage(new DishChoiceViewModel(Navigation, this)));
        }

        private async void DrinkWater()
        {
            if (IsDrinkWaterEnabled)
            {
                this.WaterAmount += this.CurrentCup.WaterAmount;
                //TODO: add name constant
                App.Current.Properties[App.CurrentUser.Id + "_last_drink"] = DateTime.Now.ToString();
                App.ScheduleNotifications(this.PlanCompletion);
            }
            //small hack to disable button
            IsDrinkWaterEnabled = false;
            await Task.Delay(2000);
            IsDrinkWaterEnabled = true;
        }

        private void OpenProgress()
        {
            Navigation.PushAsync(new ProgressPage(new ProgressViewModel(Navigation)));
        }

        private void OpenSettings()
        {
            Navigation.PushAsync(new SettingsPage(new UserInfoViewModel(Navigation, this)));
        }

        private void SyncPlanCompletions()
        {
            Thread.Sleep(3000);
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                DateTime today = DateTime.Today.Date;
                PlanCompletion plan_completion = null;
                var plan_completions = db.PlanCompletions.Include(x => x.User).Where(x => x.UserId == App.CurrentUser.Id && x.Date != today).ToList();
                var time = long.MaxValue;
                foreach (var pc in plan_completions)
                {
                    var temp_time = Math.Abs((today - pc.Date).Ticks);
                    if (temp_time < time)
                    {
                        time = temp_time;
                        plan_completion = pc;
                    }

                }
                if (plan_completion != null)
                    AddPlanCompletionToSync(plan_completion);
            }
            SavePlanCompletions();
        }

        private async void SavePlanCompletions()
        {
            //TODO: add name constant
            if (App.Current.Properties.TryGetValue("pc_to_sync", out object pc_to_sync))
            {
                List<int> synced = new List<int>();
                //TODO: add name constant
                if (App.Current.Properties.TryGetValue("pc_already_sync", out object pc_already_sync))
                {
                    try
                    {
                        synced = JsonSerializer.Deserialize<List<int>>(pc_already_sync.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        this.ShowToast?.Invoke("Failed to get synced plancompletion list from properties", 3000);
                        return;
                    }
                }

                try
                {
                    var planCompletions = JsonSerializer.Deserialize<List<PlanCompletion>>(pc_to_sync.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    if (planCompletions.Count > 0)
                    {
                        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                        {
                            List<PlanCompletion> pc_not_sync = new List<PlanCompletion>();
                            foreach (var pc in planCompletions)
                            {
                                SynchronizerResponse response = await HttpSynchronizer.SavePlanCompletions(App.connection, App.dbPath, pc.UserId, pc);
                                switch (response)
                                {
                                    case SynchronizerResponse.Success:
                                    case SynchronizerResponse.ServerError:
                                        synced.Add(pc.Id);
                                        break;
                                    case SynchronizerResponse.NetworkError:
                                        this.ShowToast?.Invoke("Failed to sync plancompletion - network error", 3000);
                                        pc_not_sync.Add(pc);
                                        break;
                                    //TODO: think about handle server error already exist plancompletion in db
                                    /*case SynchronizerResponse.ServerError:
                                        this.ShowToast?.Invoke("Failed to sync plancompletion - internal server error", 3000);
                                        pc_not_sync.Add(pc);
                                        break;*/
                                    default:
                                        this.ShowToast?.Invoke("Failed to sync plancompletion", 3000);
                                        pc_not_sync.Add(pc);
                                        break;
                                }
                            }
                            //TODO: add name constant
                            App.Current.Properties["pc_to_sync"] = JsonSerializer.Serialize(pc_not_sync, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            App.Current.Properties["pc_already_sync"] = JsonSerializer.Serialize(synced, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        }
                        else
                            this.ShowToast?.Invoke("Failed to sync plancompletion - network connection failed", 3000);
                    }
                }
                catch
                {
                    this.ShowToast?.Invoke("Failed to sync plan completion list from properties", 3000);
                }
            }
        }

        private void AddPlanCompletionToSync(PlanCompletion pc)
        {
            List<int> synced = new List<int>();
            //TODO: add name constant
            if (App.Current.Properties.TryGetValue("pc_already_sync", out object pc_already_sync))
            {
                try
                {
                    synced = JsonSerializer.Deserialize<List<int>>(pc_already_sync.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    this.ShowToast?.Invoke("Failed to get synced plancompletion list from properties", 3000);
                    return;
                }
            }
            if (synced.All(x => x != pc.Id))
            {
                //TODO: add name constant
                if (App.Current.Properties.TryGetValue("pc_to_sync", out object pc_to_sync))
                {
                    try
                    {
                        var planCompletions = JsonSerializer.Deserialize<List<PlanCompletion>>(pc_to_sync.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        if (planCompletions.Where(x => x.Id == pc.Id).FirstOrDefault() == null)
                        {
                            planCompletions.Add(pc);
                            //TODO: add name constant
                            App.Current.Properties["pc_to_sync"] = JsonSerializer.Serialize(planCompletions, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        }
                    }
                    catch
                    {
                        this.ShowToast?.Invoke("Failed to add plan completion list from properties", 3000);
                    }
                }
                else
                {
                    //TODO: add name constant
                    App.Current.Properties["pc_to_sync"] = JsonSerializer.Serialize(new List<PlanCompletion>() { pc }, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                }
            }
        }
    }
}
