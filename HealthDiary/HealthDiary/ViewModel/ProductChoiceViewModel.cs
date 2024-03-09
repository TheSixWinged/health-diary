using HealthDiary.Model;
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
    public class ProductChoiceViewModel : BaseViewModel
    {
        //ecxess eventing for calc dish price
        //public event DishEnergyHandler ProductChanged;

        public ObservableCollection<ProductViewModel> Products { get; set; } = new ObservableCollection<ProductViewModel>();
        public ObservableCollection<ProductViewModel> FilteredProducts { get; set; } = new ObservableCollection<ProductViewModel>();

        public ICommand Back_cmd { protected set; get; }

        public ProductChoiceViewModel(INavigation navigation, MainViewModel mvm) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var product_list = db.Products.Include(x => x.Unit).Include(x => x.User).Where(x => x.User == null || x.UserId == App.CurrentUser.Id).ToList();
                foreach (var product in product_list)
                    this.Products.Add(new ProductViewModel(Navigation, product, this.Products));
            }
            this.FilteredProducts = this.Products;
            this.MainViewModel = mvm;
            this.Back_cmd = new Command(Back);
        }

        public ProductChoiceViewModel(INavigation navigation, ProductInDishViewModel productInDish) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var product_list = db.Products.Include(x => x.Unit).Include(x => x.User).Where(x => x.User == null || x.UserId == App.CurrentUser.Id).ToList().Where(x => !productInDish.DishViewModel.ProductsInDish.Any(y => y.ProductInDish.Product.Id == x.Id));
                foreach (var product in product_list)
                    this.Products.Add(new ProductViewModel(Navigation, product, this.Products));
            }
            this.FilteredProducts = this.Products;
            this.ProductInDish = productInDish;
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

        private ProductInDishViewModel productInDish;
        public ProductInDishViewModel ProductInDish
        {
            get { return productInDish; }
            set
            {
                if (productInDish != value)
                {
                    productInDish = value;
                    OnPropertyChanged("ProductInDish");
                }
            }
        }

        private ProductViewModel selectedProduct;
        public ProductViewModel SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                OnPropertyChanged("SelectedProduct");
                ConfirmProduct(value);
                selectedProduct = null;
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
                this.FilteredProducts = new ObservableCollection<ProductViewModel>(this.Products.Where(x => x.Product.Name.ToLower().Contains(searchInputName.ToLower())));
                OnPropertyChanged("FilteredProducts");
            }
        }

        private async void ConfirmProduct(ProductViewModel product)
        {
            if (this.mvm != null)
            {
                string product_info = $"{product.Name} ({product.Calories} ккал, {product.Proteins} г бел, {product.Fats} г жир, {product.Carbohydrates} г угл)";
                string placeholder = $"Количество в {product.Unit?.Name}";
                string result = await App.Current.MainPage.DisplayPromptAsync("Сколько съели", product_info, "Подтвердить", "Отмена", placeholder, keyboard: Keyboard.Numeric);

                //TODO: add some confirm push or visible count of energy price before confirm
                if (!String.IsNullOrEmpty(result))
                {
                    if (Double.TryParse(result, out double count) && count > 0)
                    {
                        double factor = count / product.Unit.StandardAmount;

                        this.MainViewModel.Calories += factor * product.Calories;
                        this.MainViewModel.Proteins += factor * product.Proteins;
                        this.MainViewModel.Fats += factor * product.Fats;
                        this.MainViewModel.Carbohydrates += factor * product.Carbohydrates;

                        //await Navigation.PopModalAsync();

                        //TODO: add name constant
                        App.Current.Properties[App.CurrentUser.Id + "_last_eat"] = DateTime.Now.ToString();
                        App.ScheduleNotifications(this.MainViewModel.PlanCompletion);
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Ошибка", "Некорректный ввод", "Ок");
                    }
                }
            }
            else if (ProductInDish != null)
            {
                ProductInDish.Product = product.Product;

                if (!ProductInDish.DishViewModel.ProductsInDish.Contains(ProductInDish))
                    ProductInDish.DishViewModel.ProductsInDish.Add(ProductInDish);
                else
                    ProductInDish.Amount = null;

                //ecxess eventing for calc dish price
                //this.ProductChanged?.Invoke();

                Back();
            }
        }

        private void Back()
        {
            Navigation.PopModalAsync();
        }
    }
}
