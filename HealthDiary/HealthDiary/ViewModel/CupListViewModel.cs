using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.ViewModel.Templates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HealthDiary.ViewModel
{
    public class CupListViewModel : BaseViewModel
    {
        public ObservableCollection<Cup> Cups { get; set; } = new ObservableCollection<Cup>();

        public ICommand Back_cmd { protected set; get; }

        public CupListViewModel(INavigation navigation, MainViewModel mvm) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                this.Cups = new ObservableCollection<Cup>(db.Cups.Include(x => x.User).Where(x => x.User == null || x.UserId == App.CurrentUser.Id).ToList());
            }
            
            this.MainViewModel = mvm;
            this.Back_cmd = new Command(Back);
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

        private Cup selectedCup;
        public Cup SelectedCup
        {
            get { return selectedCup; }
            set
            {
                //selectedCup = value;
                //OnPropertyChanged("SelectedCup");
                mvm.CurrentCup = value;
                Back();
            }
        }

        private void Back()
        {
            Navigation.PopModalAsync();
        }
    }
}
