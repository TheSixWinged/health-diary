using HealthDiary.Model.Context;
using HealthDiary.View;
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
    public class DishListViewModel : BaseViewModel
    {
        public ObservableCollection<DishViewModel> Dishes { get; set; } = new ObservableCollection<DishViewModel>();
        public ObservableCollection<DishViewModel> FilteredDishes { get; set; } = new ObservableCollection<DishViewModel>();
        
        public ICommand CreateDish_cmd { protected set; get; }
        public ICommand Back_cmd { protected set; get; }

        public DishListViewModel(INavigation navigation) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var dish_list = db.Dishes/*.Include(x => x.Products).ThenInclude(y => y.Unit)*/.Include(x => x.ProductInDish).ThenInclude(y => y.Product).ThenInclude(z => z.Unit).Include(x => x.User).Where(x => x.User.Id == App.CurrentUser.Id).ToList();
                foreach (var dish in dish_list)
                    this.Dishes.Add(new DishViewModel(Navigation, dish, this.Dishes));
            }
            this.FilteredDishes = this.Dishes;
            Dishes.CollectionChanged += RefreshFilteredList;
            this.CreateDish_cmd = new Command(CreateDish);
            this.Back_cmd = new Command(Back);
        }

        private DishViewModel selectedDish;
        public DishViewModel SelectedDish
        {
            get { return selectedDish; }
            set
            {
                DishViewModel dish = value;
                selectedDish = null;
                OnPropertyChanged("SelectedDish");
                this.Navigation.PushAsync(new DishPage(new DishViewModel(Navigation, dish.Dish, this.Dishes)));
            }
        }

        private string searchInputName;
        public string SearchInputName
        {
            get { return searchInputName ?? ""; }
            set
            {
                searchInputName = value;
                OnPropertyChanged("SearchInput");
                RefreshFilteredList(null, null);
            }
        }

        public void RefreshList()
        {
            this.Dishes = new ObservableCollection<DishViewModel>();
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var dish_list = db.Dishes/*.Include(x => x.Products).ThenInclude(y => y.Unit)*/.Include(x => x.ProductInDish).ThenInclude(y => y.Product).ThenInclude(z => z.Unit).Include(x => x.User).Where(x => x.User.Id == App.CurrentUser.Id).ToList();
                foreach (var dish in dish_list)
                    this.Dishes.Add(new DishViewModel(Navigation, dish, this.Dishes));
            }
            this.FilteredDishes = this.Dishes;
            OnPropertyChanged("FilteredDishes");
            Dishes.CollectionChanged += RefreshFilteredList;
        }

        private void RefreshFilteredList(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.FilteredDishes = new ObservableCollection<DishViewModel>(this.Dishes.Where(x => x.Dish.Name.ToLower().Contains(searchInputName?.ToLower())));
            OnPropertyChanged("FilteredDishes");
        }

        private void CreateDish()
        {
            Navigation.PushAsync(new DishPage(new DishViewModel(Navigation, new Model.Dish(), this.Dishes)));
        }

        private void Back()
        {
            Navigation.PopAsync();
        }
    }
}
