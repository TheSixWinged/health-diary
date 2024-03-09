using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace HealthDiary.ViewModel.Templates
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public INavigation Navigation;
        public BaseViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        protected async void ClearNavigationStack()
        {
            /*var existingPages = Navigation.NavigationStack.ToList();
            foreach (var page in existingPages)
                Navigation.RemovePage(page);*/

            await Navigation.PopToRootAsync();
        }
    }
}
