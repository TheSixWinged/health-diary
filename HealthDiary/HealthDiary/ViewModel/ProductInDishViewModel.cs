using HealthDiary.Model;
using HealthDiary.View;
using HealthDiary.ViewModel.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HealthDiary.ViewModel
{
    public class ProductInDishViewModel : BaseViewModel
    {
        public event DishEnergyHandler ProductInDishChanged;

        public ProductInDish ProductInDish { get; private set; }

        public ICommand ChoiceProduct_cmd { protected set; get; }
        public ICommand DeleteProduct_cmd { protected set; get; }

        public ProductInDishViewModel(INavigation navigation, ProductInDish productInDish, DishViewModel dvm) : base(navigation)
        {
            this.ProductInDish = productInDish;
            this.dvm = dvm;
            this.ChoiceProduct_cmd = new Command(ChoiceProduct);
            this.DeleteProduct_cmd = new Command(DeleteProduct);
        }

        private DishViewModel dvm;
        public DishViewModel DishViewModel
        {
            get { return dvm; }
            set
            {
                dvm = value;
                OnPropertyChanged("DishViewModel");
            }
        }

        public Product Product
        {
            get { return ProductInDish.Product; }
            set
            {
                ProductInDish.Product = value;
                OnPropertyChanged("Product");
                this.ProductInDishChanged?.Invoke();
            }
        }

        public double? Amount
        {
            get { return ProductInDish.Amount; }
            set
            {
                ProductInDish.Amount = value;
                OnPropertyChanged("Amount");
                this.ProductInDishChanged?.Invoke();
            }
        }

        private void ChoiceProduct(object productInDishObject)
        {
            if (productInDishObject is ProductInDishViewModel)
            {
                ProductInDishViewModel productInDish = productInDishObject as ProductInDishViewModel;
                //ecxess eventing for calc dish price
                //var prch_vm = new ProductChoiceViewModel(Navigation, productInDish);
                //prch_vm.ProductChanged += ProductChanged;
                //this.Navigation.PushModalAsync(new ProductChoicePage(prch_vm));
                this.Navigation.PushModalAsync(new ProductChoicePage(new ProductChoiceViewModel(Navigation, productInDish)));
            }
        }

        private void DeleteProduct(object productInDishObject)
        {
            if (productInDishObject is ProductInDishViewModel)
            {
                ProductInDishViewModel productInDish = productInDishObject as ProductInDishViewModel;
                this.DishViewModel.ProductsInDish.Remove(productInDish);
            }
        }

        //ecxess eventing for calc dish price
        /*private void ProductChanged()
        {
            this.ProductInDishChanged?.Invoke();
        }*/
    }
}
