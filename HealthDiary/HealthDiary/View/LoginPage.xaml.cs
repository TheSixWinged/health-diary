using HealthDiary.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthDiary.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage(bool loginVisible)
        {
            InitializeComponent();
            LoginViewModel vm = new LoginViewModel(this.Navigation, loginVisible);
            vm.ShowToast += this.DisplayToastMessage;
            this.BindingContext = vm;
        }

        public void DisplayToastMessage(string message, int durationMsec)
        {
            this.DisplayToastAsync(message, durationMsec);
        }
    }
}