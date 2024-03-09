using HealthDiary.Model;
using HealthDiary.Model.Context;
using HealthDiary.Model.Extensions;
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
    public class ProductViewModel : BaseViewModel
    {
        public Product Product { get; private set; }
        public ObservableCollection<ProductViewModel> Products { get; set; }
        public ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();

        public ICommand SaveProduct_cmd { protected set; get; }
        public ICommand DeleteProduct_cmd { protected set; get; }
        public ICommand Back_cmd { protected set; get; }

        public ProductViewModel(INavigation navigation, Product product, ObservableCollection<ProductViewModel> productlist) : base(navigation)
        {
            this.Product = product;
            this.Products = productlist;
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                this.Units = new ObservableCollection<Unit>(db.Units.ToList());
            }
            this.SaveProduct_cmd = new Command(SaveProduct);
            this.DeleteProduct_cmd = new Command(DeleteProduct);
            this.Back_cmd = new Command(Back);
            //small hack to set default value to unit picker
            this.Unit = Units.FirstOrDefault(x => x.Id == this.Product.UnitId);
        }

        public string Name
        {
            get { return Product.Name; }
            set
            {
                Product.Name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public byte[] Image
        {
            get { return Product.Image; }
            set
            {
                Product.Image = value;
                OnPropertyChanged("Image");
            }
        }

        public double Calories
        {
            get { return Product.Calories.RoundToSignDigits(2); }
            set
            {
                Product.Calories = value;
                OnPropertyChanged("Calories");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public double Proteins
        {
            get { return Product.Proteins.RoundToSignDigits(2); }
            set
            {
                Product.Proteins = value;
                OnPropertyChanged("Proteins");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public double Fats
        {
            get { return Product.Fats.RoundToSignDigits(2); }
            set
            {
                Product.Fats = value;
                OnPropertyChanged("Fats");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public double Carbohydrates
        {
            get { return Product.Carbohydrates.RoundToSignDigits(2); }
            set
            {
                Product.Carbohydrates = value;
                OnPropertyChanged("Carbohydrates");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public bool isReadonly
        {
            get { return Product.IsReadonly; }
            set { }
        }

        public Unit Unit
        {
            get { return Product.Unit; }
            set
            {
                Product.Unit = value;
                OnPropertyChanged("Unit");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        private bool isConfirmEnabled;
        public bool IsConfirmEnabled
        {
            get
            {
                return Product.IsValid();
            }
            set { }
        }

        private void SaveProduct()
        {
            if (Product.IsValid())
            {
                if (Product.Id == 0)
                {
                    Product.User = App.CurrentUser;
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        db.Users.Attach(Product.User);
                        db.Units.Attach(Product.Unit);
                        db.Products.Add(Product);
                        db.SaveChanges();
                    }
                    Products.Add(this);
                    //TODO: add sort for products
                }
                else
                {
                    //TODO: add warning about changes dishes and maybe with list of these dishes
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        db.Products.Update(Product);
                        db.SaveChanges();
                    }

                    List<int> dishes_ids = new List<int>();
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        var dishes = db.Dishes.Where(x => x.ProductInDish.Any(y => y.ProductId == Product.Id));
                        foreach (var dish in dishes)
                        {
                            dishes_ids.Add(dish.Id);
                        }
                    }
                    UpdateDishesInDB(dishes_ids);

                    //TODO: how to change vm without remove-insert
                    int index = Products.IndexOf(Products.FirstOrDefault(x => x.Product.Id == this.Product.Id));
                    Products.Remove(Products.FirstOrDefault(x => x.Product.Id == this.Product.Id));
                    Products.Insert(index, this);
                }

                Back();
            }
            else
            {
                //TODO: push error emty Unit or Name
            }
        }

        private void DeleteProduct()
        {
            //TODO: add warning about changes dishes and maybe with list of these dishes
            if (Product.Id != 0)
            {
                List<int> dishes_ids = new List<int>();
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    var dishes = db.Dishes.Where(x => x.ProductInDish.Any(y => y.ProductId == Product.Id));
                    foreach (var dish in dishes)
                    {
                        dishes_ids.Add(dish.Id);
                    }
                }

                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    var product = db.Products.Find(Product.Id);
                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                    }
                }

                UpdateDishesInDB(dishes_ids);

                Products.Remove(Products.FirstOrDefault(x => x.Product.Id == this.Product.Id));
            }

            Back();
        }

        private void Back()
        {
            Navigation.PopAsync();
        }

        private void UpdateDishesInDB(List<int> idlist)
        {
            foreach (int dishid in idlist)
            {
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    var dish = db.Dishes.Find(dishid);
                    var product_in_dish_list = db.ProductInDish.Where(x => x.DishId == dishid).Include(x => x.Product).ThenInclude(y => y.Unit);

                    dish.Calories = product_in_dish_list.Sum(x => x.Product.Calories / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (dish.Portion != null && dish.Portion > 0 ? (double)dish.Portion : 1);
                    dish.Proteins = product_in_dish_list.Sum(x => x.Product.Proteins / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (dish.Portion != null && dish.Portion > 0 ? (double)dish.Portion : 1);
                    dish.Fats = product_in_dish_list.Sum(x => x.Product.Fats / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (dish.Portion != null && dish.Portion > 0 ? (double)dish.Portion : 1);
                    dish.Carbohydrates = product_in_dish_list.Sum(x => x.Product.Carbohydrates / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (dish.Portion != null && dish.Portion > 0 ? (double)dish.Portion : 1);

                    db.SaveChanges();
                }
            }
        }
    }
}
