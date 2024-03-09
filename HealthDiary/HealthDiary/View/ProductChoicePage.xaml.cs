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
    public partial class ProductChoicePage : ContentPage
    {
        public ProductChoicePage(ProductChoiceViewModel vm)
        {
            InitializeComponent();
            if (vm != null)
                this.BindingContext = vm;
        }
    }
}