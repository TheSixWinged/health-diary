using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.Model.Services;
using HealthDiary.View;
using HealthDiary.ViewModel.Templates;
using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HealthDiary.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        public event ToastMessageHandler ShowToast;

        public ICommand Login_cmd { protected set; get; }
        public ICommand Registration_cmd { protected set; get; }
        public ICommand SetRegistrationVisible_cmd { protected set; get; }
        public ICommand SetLoginVisible_cmd { protected set; get; }

        public LoginViewModel(INavigation navigation, bool loginVisible) : base(navigation)
        {
            if (loginVisible)
                IsLoginVisible = true;
            else
                IsLoginVisible = false;

            this.Login_cmd = new Command(Loging);
            this.Registration_cmd = new Command(Registration);
            this.SetRegistrationVisible_cmd = new Command(SetRegistrationVisible);
            this.SetLoginVisible_cmd = new Command(SetLoginVisible);
        }

        private string login;
        public string Login
        {
            get { return login; }
            set
            {
                login = value;
                OnPropertyChanged("Login");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        private bool isConfirmEnabled;
        public bool IsConfirmEnabled
        {
            //TODO: password validation
            get { return !String.IsNullOrEmpty(Login) && !String.IsNullOrEmpty(Password); }
            set { }
        }

        private bool isLoginVisible;
        public bool IsLoginVisible
        {
            get { return isLoginVisible; }
            set
            {
                isLoginVisible = value;
                OnPropertyChanged("IsLoginVisible");
                OnPropertyChanged("IsRegistrationVisible");
            }
        }

        private bool isRegistrationVisible;
        public bool IsRegistrationVisible
        {
            get { return !IsLoginVisible; }
            set { }
        }

        private async void Loging()
        {
            //TODO: password validation

            if (!String.IsNullOrEmpty(Login) && !String.IsNullOrEmpty(Password))
            {
                bool user_cached = false;
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    if (db.Users.Where(x => x.Login == Login).Count() != 0)
                        user_cached = true;
                }

                if (user_cached)
                {
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        var user = db.Users.Include(x => x.Gender).Include(x => x.PhysicalActivityType).Include(x => x.Plan).
                        Include(x => x.Products).Include(x => x.Dishes).Include(x => x.Cups).Include(x => x.PlanCompletions).
                        Where(x => x.Login == Login).FirstOrDefault();

                        if (user.Password == Password)
                        {
                            App.CurrentUser = user;
                            //TODO: add name constant
                            App.Current.Properties["current_user_id"] = user.Id;

                            if (user.Plan == null)
                                await Navigation.PushAsync(new UserInfoPage());
                            else
                            {
                                App.Current.MainPage = new NavigationPage(new MainPage());
                                //Navigation.PushAsync(new MainPage());
                                ClearNavigationStack();
                            }
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert("Ошибка", "Неверный пароль", "OK");
                            Password = null;
                        }
                    }
                }
                else
                {
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        SynchronizerResponse response = await HttpSynchronizer.LoginUser(App.connection, App.dbPath, Login, Password);
                        //TODO: make toast message
                        switch (response)
                        {
                            case SynchronizerResponse.Success:
                                Loging();
                                break;
                            case SynchronizerResponse.NetworkError:
                                this.ShowToast?.Invoke("Failed to login user - network error", 3000);
                                break;
                            case SynchronizerResponse.ServerError:
                                this.ShowToast?.Invoke("Failed to login user - internal server error", 3000);
                                break;
                            case SynchronizerResponse.InvalidPasswordError:
                                await App.Current.MainPage.DisplayAlert("Ошибка", "Неверный пароль", "OK");
                                Password = null;
                                break;
                            default:
                                await App.Current.MainPage.DisplayAlert("Ошибка", "Пользователь не найден", "OK");
                                Login = null;
                                Password = null;
                                break;
                        }
                    }
                    else
                        this.ShowToast?.Invoke("Network connection failed, for first login app needs to sync data with server, please connect you device to network", 5000);
                }
            }
            else
            {
                this.ShowToast?.Invoke("Login or password is empty", 3000);
            }
        }

        private async void Registration()
        {
            if (!String.IsNullOrEmpty(Login) && !String.IsNullOrEmpty(Password))
            {
                bool user_cached = false;
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    if (db.Users.Where(x => x.Login == Login).Count() != 0)
                        user_cached = true;
                }

                if (user_cached)
                {
                    await App.Current.MainPage.DisplayAlert("Ошибка", "Пользователь уже зарегистрирован", "OK");
                    Login = null;
                    Password = null;
                }
                else
                {
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        SynchronizerResponse response = await HttpSynchronizer.RegisterUser(App.connection, App.dbPath, Login, Password);
                        //TODO: make toast message
                        switch (response)
                        {
                            case SynchronizerResponse.Success:
                                Loging();
                                break;
                            case SynchronizerResponse.NetworkError:
                                this.ShowToast?.Invoke("Failed to reg user - network error", 3000);
                                break;
                            case SynchronizerResponse.ServerError:
                                this.ShowToast?.Invoke("Failed to reg user - internal server error", 3000);
                                break;
                            case SynchronizerResponse.UserAlreadyExistError:
                                await App.Current.MainPage.DisplayAlert("Ошибка", "Пользователь уже зарегистрирован", "OK");
                                Login = null;
                                Password = null;
                                break;
                            default:
                                this.ShowToast?.Invoke("Failed to reg user", 3000);
                                break;
                        }
                    }
                    else
                        this.ShowToast?.Invoke("Network connection failed, for registration app needs to sync data with server, please connect you device to network", 5000);
                }
            }
            else
            {
                this.ShowToast?.Invoke("Login or password is empty", 3000);
            }
        }

        private void SetRegistrationVisible()
        {
            IsLoginVisible = false;
            Login = null;
            Password = null;
        }

        private void SetLoginVisible()
        {
            IsLoginVisible = true;
            Login = null;
            Password = null;
        }
    }
}
