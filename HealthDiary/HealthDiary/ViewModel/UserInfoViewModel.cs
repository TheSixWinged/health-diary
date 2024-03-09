using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.Model.Services;
using HealthDiary.View;
using HealthDiary.ViewModel.Templates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HealthDiary.ViewModel
{
    public class UserInfoViewModel : BaseViewModel
    {
        public event ToastMessageHandler ShowToast;

        public ObservableCollection<Gender> Genders { get; set; } = new ObservableCollection<Gender>();
        public ObservableCollection<PhysicalActivityType> PhysicalActivityTypes { get; set; }  = new ObservableCollection<PhysicalActivityType>();

        public ICommand Confirm_cmd { protected set; get; }
        public ICommand ConfirmUpdate_cmd { protected set; get; }
        public ICommand Logout_cmd { protected set; get; }
        public ICommand Back_cmd { protected set; get; }

        public UserInfoViewModel(INavigation navigation, MainViewModel mvm) : base(navigation)
        {
            this.MainViewModel = mvm;
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                this.Genders = new ObservableCollection<Gender>(db.Genders.ToList());
                this.PhysicalActivityTypes = new ObservableCollection<PhysicalActivityType>(db.PhysicalActivityTypes.ToList());
            }
            this.Confirm_cmd = new Command(Confirm);
            this.ConfirmUpdate_cmd = new Command(ConfirmUpdate);
            this.Logout_cmd = new Command(Logout);
            this.Back_cmd = new Command(Back);
            //small hacks to set default values to pickers and doubles
            this.Gender = Genders.FirstOrDefault(x => x.Id == App.CurrentUser.GenderId);
            this.PhysicalActivityType = PhysicalActivityTypes.FirstOrDefault(x => x.Id == App.CurrentUser.PhysicalActivityTypeId);
            if (App.CurrentUser.Growth > 0)
                this.Growth = App.CurrentUser.Growth;
            if (App.CurrentUser.Weight > 0)
                this.Weight = App.CurrentUser.Weight;
        }

        private MainViewModel mvm;
        public MainViewModel MainViewModel
        {
            get { return mvm; }
            set
            {
                if (mvm != value)
                {
                    mvm = value;
                    OnPropertyChanged("MainViewModel");
                }
            }
        }

        private double? growth;
        public double? Growth
        {
            get { return growth; }
            set
            {
                growth = value;
                OnPropertyChanged("Growth");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        private double? weight;
        public double? Weight
        {
            get { return weight; }
            set
            {
                weight = value;
                OnPropertyChanged("Weight");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public DateTime DateMin
        {
            get { return new DateTime(1950, 1, 1); }
            set { }
        }

        public DateTime DateMax
        {
            get { return DateTime.Now.Date; }
            set { }
        }

        public DateTime DateOfBirth
        {
            get { return App.CurrentUser.DateOfBirth; }
            set
            {
                App.CurrentUser.DateOfBirth = value;
                OnPropertyChanged("DateOfBirth");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public Gender Gender
        {
            get { return App.CurrentUser.Gender; }
            set
            {
                App.CurrentUser.Gender = value;
                OnPropertyChanged("Gender");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public PhysicalActivityType PhysicalActivityType
        {
            get { return App.CurrentUser.PhysicalActivityType; }
            set
            {
                App.CurrentUser.PhysicalActivityType = value;
                OnPropertyChanged("PhysicalActivityType");
                OnPropertyChanged("PhysicalActivityTypeComment");
                OnPropertyChanged("IsPhysActTypeCommentVisible");
                OnPropertyChanged("IsPhysActTypeCommentHide");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public string PhysicalActivityTypeComment
        {
            get { return PhysicalActivityType?.Comment; }
            set { }
        }

        public bool IsPhysActTypeCommentVisible
        {
            get { return !String.IsNullOrEmpty(PhysicalActivityTypeComment); }
            set { }
        }

        public bool IsPhysActTypeCommentHide
        {
            get { return !IsPhysActTypeCommentVisible; }
            set { }
        }

        private bool isConfirmEnabled;
        public bool IsConfirmEnabled
        {
            get
            {
                return Growth != null && Growth > 0 && Weight != null && Weight > 0 && DateOfBirth > DateMin && DateOfBirth < DateMax && Gender != null && PhysicalActivityType != null;
            }
            set { }
        }

        private async void Confirm()
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var user = db.Users/*.Include(x => x.Gender).Include(x => x.PhysicalActivityType)*/.Include(x => x.Plan).
                Include(x => x.Products).Include(x => x.Dishes).Include(x => x.Cups).Include(x => x.PlanCompletions).
                Where(x => x.Id == App.CurrentUser.Id).FirstOrDefault();

                if (user != null)
                {
                    user.Growth = (double)Growth;
                    user.Weight = (double)Weight;
                    user.DateOfBirth = DateOfBirth;
                    user.Gender = Gender;
                    user.PhysicalActivityType = PhysicalActivityType;
                    var plan = user.CalculatePlan();
                    user.Plan = plan;
                    plan.User = user;
                    db.Plans.Add(plan);

                    db.SaveChanges();

                    App.CurrentUser = user;
                }
            }

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                SynchronizerResponse response = await HttpSynchronizer.UpdateUser(App.connection, App.dbPath, App.CurrentUser);
                //TODO: make toast message
                switch (response)
                {
                    case SynchronizerResponse.Success:
                        break;
                    case SynchronizerResponse.NetworkError:
                        this.ShowToast?.Invoke("Failed to sync update user - network error", 3000);
                        break;
                    case SynchronizerResponse.ServerError:
                        this.ShowToast?.Invoke("Failed to sync update user - internal server error", 3000);
                        break;
                    default:
                        this.ShowToast?.Invoke("Failed to sync update user", 3000);
                        break;
                }
            }
            else
                this.ShowToast?.Invoke("Failed to sync update user - network connection failed", 3000);

            App.Current.MainPage = new NavigationPage(new MainPage());
            //Navigation.PushAsync(new MainPage());
            ClearNavigationStack();
        }

        private async void ConfirmUpdate()
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var user = db.Users/*.Include(x => x.Gender).Include(x => x.PhysicalActivityType)*/.Include(x => x.Plan).
                Include(x => x.Products).Include(x => x.Dishes).Include(x => x.Cups).Include(x => x.PlanCompletions).
                Where(x => x.Id == App.CurrentUser.Id).FirstOrDefault();

                if (user != null)
                {
                    user.Growth = (double)Growth;
                    user.Weight = (double)Weight;
                    user.DateOfBirth = DateOfBirth;
                    user.Gender = Gender;
                    user.PhysicalActivityType = PhysicalActivityType;

                    db.SaveChanges();

                    App.CurrentUser = user;
                }
            }

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                SynchronizerResponse response = await HttpSynchronizer.UpdateUser(App.connection, App.dbPath, App.CurrentUser);
                //TODO: make toast message
                switch (response)
                {
                    case SynchronizerResponse.Success:
                        break;
                    case SynchronizerResponse.NetworkError:
                        this.ShowToast?.Invoke("Failed to sync update user - network error", 3000);
                        break;
                    case SynchronizerResponse.ServerError:
                        this.ShowToast?.Invoke("Failed to sync update user - internal server error", 3000);
                        break;
                    default:
                        this.ShowToast?.Invoke("Failed to sync update user", 3000);
                        break;
                }
            }
            else
                this.ShowToast?.Invoke("Failed to sync update user - network connection failed", 3000);

            bool updatePlan = await App.Current.MainPage.DisplayAlert("Внимание!", "Обновить план потребления?", "Да", "Нет");
            if (updatePlan)
            {
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    var user = db.Users.Include(x => x.Gender).Include(x => x.PhysicalActivityType).Include(x => x.Plan).
                    Include(x => x.Products).Include(x => x.Dishes).Include(x => x.Cups).Include(x => x.PlanCompletions).
                    Where(x => x.Id == App.CurrentUser.Id).FirstOrDefault();

                    if (user != null)
                    {
                        user.CalculatePlan();
                        db.Plans.Update(user.Plan);

                        db.SaveChanges();

                        App.CurrentUser = user;
                    }
                }

                DateTime today = DateTime.Now.Date;
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    var planCompletion = db.PlanCompletions.Where(x => x.UserId == App.CurrentUser.Id && x.Date.Date == today).FirstOrDefault();
                    planCompletion.CaloriesPlan = App.CurrentUser.Plan.Calories;
                    planCompletion.ProteinsPlan = App.CurrentUser.Plan.Proteins;
                    planCompletion.FatsPlan = App.CurrentUser.Plan.Fats;
                    planCompletion.CarbohydratesPlan = App.CurrentUser.Plan.Carbohydrates;
                    planCompletion.WaterAmountPlan = App.CurrentUser.Plan.WaterAmount;

                    db.SaveChanges();
                }

                if (this.MainViewModel != null)
                {
                    this.MainViewModel.CaloriesPlan = App.CurrentUser.Plan.Calories;
                    this.MainViewModel.ProteinsPlan = App.CurrentUser.Plan.Proteins;
                    this.MainViewModel.FatsPlan = App.CurrentUser.Plan.Fats;
                    this.MainViewModel.CarbohydratesPlan = App.CurrentUser.Plan.Carbohydrates;
                    this.MainViewModel.WaterAmountPlan = App.CurrentUser.Plan.WaterAmount;

                    App.ScheduleNotifications(this.MainViewModel.PlanCompletion);
                }
            }

            Back();
        }

        private async void Logout()
        {
            bool logout = await App.Current.MainPage.DisplayAlert("Выход из системы", "Выйти из системы?", "Да", "Нет");
            if (logout)
            {
                //TODO: add name constant
                App.Current.Properties["is_noty_scheduled"] = false;
                App.NotyScheduleService.CancelNotifications();

                App.Current.Properties.Remove("current_user_id");
                
                App.Current.MainPage = new NavigationPage(new LoginPage(true));
                ClearNavigationStack();
            }
        }

        private void Back()
        {
            Navigation.PopAsync();
        }
    }
}
