using HealthDiary.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HealthDiary.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CupListPage : ContentPage
    {
        public CupListPage(CupListViewModel vm)
        {
            InitializeComponent();
            if (vm != null)
                this.BindingContext = vm;
        }
    }
}