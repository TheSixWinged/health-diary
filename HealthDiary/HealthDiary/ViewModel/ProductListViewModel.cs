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
    public delegate void ProductListHandler();

    public class ProductListViewModel : BaseViewModel
    {
        public event ProductListHandler ProductListChanged;

        public ObservableCollection<ProductViewModel> Products { get; set; } = new ObservableCollection<ProductViewModel>();
        public ObservableCollection<ProductViewModel> FilteredProducts { get; set; } = new ObservableCollection<ProductViewModel>();

        public ICommand CreateProduct_cmd { protected set; get; }
        public ICommand Back_cmd { protected set; get; }

        public ProductListViewModel(INavigation navigation) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var product_list = db.Products.Include(x => x.Unit).Include(x => x.User).Where(x => x.User == null || x.UserId == App.CurrentUser.Id).ToList();
                foreach (var product in product_list)
                    this.Products.Add(new ProductViewModel(Navigation, product, this.Products));
            }
            this.FilteredProducts = this.Products;
            Products.CollectionChanged += RefreshFilteredList;
            Products.CollectionChanged += CallEventProductListChanged;
            this.CreateProduct_cmd = new Command(CreateProduct);
            this.Back_cmd = new Command(Back);
        }

        private ProductViewModel selectedProduct;
        public ProductViewModel SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                ProductViewModel product = value;
                selectedProduct = null;
                OnPropertyChanged("SelectedProduct");
                if (!value.isReadonly)
                    this.Navigation.PushAsync(new ProductPage(new ProductViewModel(Navigation, product.Product, this.Products)));
                else
                {
                    //TODO: push toast cant change standart products
                }
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

        private void RefreshFilteredList(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.FilteredProducts = new ObservableCollection<ProductViewModel>(this.Products.Where(x => x.Product.Name.ToLower().Contains(searchInputName?.ToLower())));
            OnPropertyChanged("FilteredProducts");
        }

        private void CallEventProductListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ProductListChanged?.Invoke();
        }

        private void CreateProduct()
        {
            Navigation.PushAsync(new ProductPage(new ProductViewModel(Navigation, new Model.Product(), this.Products)));
        }

        private void Back()
        {
            Navigation.PopAsync();
        }
    }
}
