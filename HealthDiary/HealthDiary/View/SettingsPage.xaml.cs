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
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage(UserInfoViewModel vm)
        {
            InitializeComponent();
            if (vm != null)
            {
                vm.ShowToast += this.DisplayToastMessage;
                this.BindingContext = vm;
            }
        }

        public void DisplayToastMessage(string message, int durationMsec)
        {
            this.DisplayToastAsync(message, durationMsec);
        }
    }
}