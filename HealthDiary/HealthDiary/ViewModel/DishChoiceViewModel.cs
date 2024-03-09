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
    public class DishChoiceViewModel : BaseViewModel
    {
        public ObservableCollection<DishViewModel> Dishes { get; set; } = new ObservableCollection<DishViewModel>();
        public ObservableCollection<DishViewModel> FilteredDishes { get; set; } = new ObservableCollection<DishViewModel>();

        public ICommand Back_cmd { protected set; get; }

        public DishChoiceViewModel(INavigation navigation, MainViewModel mvm) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var dish_list = db.Dishes.Include(x => x.User).Where(x => x.User.Id == App.CurrentUser.Id).ToList();
                foreach (var dish in dish_list)
                    this.Dishes.Add(new DishViewModel(Navigation, dish, this.Dishes));
            }
            this.FilteredDishes = this.Dishes;
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

        private DishViewModel selectedDish;
        public DishViewModel SelectedDish
        {
            get { return selectedDish; }
            set
            {
                OnPropertyChanged("SelectedDish");
                ConfirmDish(value);
                selectedDish = null;
            }
        }

        private string searchInputName;
        public string SearchInputName
        {
            get { return searchInputName; }
            set
            {
                searchInputName = value;
                OnPropertyChanged("SearchInput");
                this.FilteredDishes = new ObservableCollection<DishViewModel>(this.Dishes.Where(x => x.Dish.Name.ToLower().Contains(searchInputName.ToLower())));
                OnPropertyChanged("FilteredDishes");
            }
        }

        private async void ConfirmDish(DishViewModel dish)
        {
            string dish_info = $"{dish.Name} ({dish.Calories} ккал, {dish.Proteins} г бел, {dish.Fats} г жир, {dish.Carbohydrates} г угл)";
            bool result = await App.Current.MainPage.DisplayAlert("Подтвердите что съели порцию", dish_info, "Подтвердить", "Отмена");

            //TODO: add some confirm push after confirm
            if (result)
            {
                this.MainViewModel.Calories += dish.Calories;
                this.MainViewModel.Proteins += dish.Proteins;
                this.MainViewModel.Fats += dish.Fats;
                this.MainViewModel.Carbohydrates += dish.Carbohydrates;
                //TODO: add name constant
                App.Current.Properties[App.CurrentUser.Id + "_last_eat"] = DateTime.Now.ToString();
                App.ScheduleNotifications(this.MainViewModel.PlanCompletion);
            }
        }

        private void Back()
        {
            Navigation.PopModalAsync();
        }
    }
}
