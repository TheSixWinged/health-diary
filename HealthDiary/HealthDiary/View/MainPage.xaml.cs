using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.View;
using HealthDiary.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace HealthDiary
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            MainViewModel vm = new MainViewModel(this.Navigation);
            vm.ShowToast += this.DisplayToastMessage;
            this.BindingContext = vm;
        }

        public void DisplayToastMessage(string message, int durationMsec)
        {
            this.DisplayToastAsync(message, durationMsec);
        }
    }
}
