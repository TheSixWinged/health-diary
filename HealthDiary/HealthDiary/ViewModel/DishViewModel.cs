using HealthDiary.Model;
using HealthDiary.Model.Extensions;
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
    public delegate void DishEnergyHandler();

    public class DishViewModel : BaseViewModel
    {
        public event DishEnergyHandler EnergyPriceChanged;

        public Dish Dish { get; private set; }
        public ObservableCollection<ProductInDishViewModel> ProductsInDish { get; set; } = new ObservableCollection<ProductInDishViewModel>();
        public ObservableCollection<DishViewModel> Dishes { get; set; }

        public ICommand AddProduct_cmd { protected set; get; }
        public ICommand SaveDish_cmd { protected set; get; }
        public ICommand DeleteDish_cmd { protected set; get; }
        public ICommand Back_cmd { protected set; get; }

        public DishViewModel(INavigation navigation, Dish dish, ObservableCollection<DishViewModel> dishlist) : base(navigation)
        {
            this.Dish = dish;
            //using (ApplicationContext db = new ApplicationContext(App.dbPath))
            //{
                //var products_in_dish_list = db.ProductInDish.Include(x => x.Dish).Include(x => x.Product).Where(x => x.DishId == this.Dish.Id).ToList();
                var products_in_dish_list = Dish.ProductInDish;
                foreach (var productInDish in products_in_dish_list)
                {
                    var prid = new ProductInDishViewModel(Navigation, productInDish, this);
                    prid.ProductInDishChanged += this.CalcEnergyPrice;
                    this.ProductsInDish.Add(prid);
                }
            //}
            this.Dishes = dishlist;
            this.AddProduct_cmd = new Command(AddProduct);
            this.SaveDish_cmd = new Command(SaveDish);
            this.DeleteDish_cmd = new Command(DeleteDish);
            this.Back_cmd = new Command(Back);

            ProductsInDish.CollectionChanged += ProductInDishListChanges;
            EnergyPriceChanged += CalcEnergyPrice;
        }

        public string Name
        {
            get { return Dish.Name; }
            set
            {
                Dish.Name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public byte[] Image
        {
            get { return Dish.Image; }
            set
            {
                Dish.Image = value;
                OnPropertyChanged("Image");
            }
        }

        public double Calories
        {
            get { return Dish.Calories.RoundToSignDigits(2); }
            set
            {
                Dish.Calories = value;
                OnPropertyChanged("Calories");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public double Proteins
        {
            get { return Dish.Proteins.RoundToSignDigits(2); }
            set
            {
                Dish.Proteins = value;
                OnPropertyChanged("Proteins");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public double Fats
        {
            get { return Dish.Fats.RoundToSignDigits(2); }
            set
            {
                Dish.Fats = value;
                OnPropertyChanged("Fats");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public double Carbohydrates
        {
            get { return Dish.Carbohydrates.RoundToSignDigits(2); }
            set
            {
                Dish.Carbohydrates = value;
                OnPropertyChanged("Carbohydrates");
                OnPropertyChanged("IsConfirmEnabled");
            }
        }

        public string Comment
        {
            get { return Dish.Comment; }
            set
            {
                Dish.Comment = value;
                OnPropertyChanged("Comment");
            }
        }

        public double? Portion
        {
            get { return Dish.Portion; }
            set
            {
                Dish.Portion = value;
                OnPropertyChanged("Portion");
                OnPropertyChanged("IsConfirmEnabled");
                EnergyPriceChanged?.Invoke();
            }
        }

        private bool isConfirmEnabled;
        public bool IsConfirmEnabled
        {
            get
            {
                return Dish.IsValid();
            }
            set { }
        }

        private void AddProduct()
        {
            ProductInDishViewModel productInDish = new ProductInDishViewModel(Navigation, new ProductInDish() { Dish = this.Dish }, this);
            productInDish.ProductInDishChanged += this.CalcEnergyPrice;
            this.Navigation.PushModalAsync(new ProductChoicePage(new ProductChoiceViewModel(Navigation, productInDish)));
        }

        private void SaveDish()
        {
            Dish.ProductInDish = this.ProductsInDish.Select(x => x.ProductInDish).ToList();
            //Dish.Products = this.ProductsInDish.Select(x => x.Product).ToList();

            if (Dish.IsValid())
            {
                if (Dish.Id == 0)
                {
                    Dish.User = App.CurrentUser;
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        Dish dish = new Dish()
                        {
                            UserId = App.CurrentUser.Id,
                            Calories = Dish.Calories,
                            Proteins = Dish.Proteins,
                            Fats = Dish.Fats,
                            Carbohydrates = Dish.Carbohydrates,
                            Name = Dish.Name,
                            Comment = Dish.Comment,
                            Image = Dish.Image,
                            Portion = Dish.Portion
                        };
                        db.Dishes.Add(dish);
                        db.SaveChanges();

                        Dish.Id = dish.Id;
                    }
                    foreach (var prid in Dish.ProductInDish)
                    {
                        var dish_id = Dish.Id;
                        var product_id = prid.Product.Id;
                        var amount = prid.Amount;
                        using (ApplicationContext db = new ApplicationContext(App.dbPath))
                        {
                            db.ProductInDish.Add(new ProductInDish() { DishId = dish_id, ProductId = product_id, Amount = amount });
                            db.SaveChanges();
                        }
                    }
                    Dishes.Add(this);
                    //TODO: add sort for dishes
                }
                else
                {
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        db.ProductInDish.RemoveRange(db.ProductInDish.Where(x => x.DishId == Dish.Id));
                        db.SaveChanges();
                    }
                    foreach (var prid in Dish.ProductInDish)
                    {
                        var dish_id = Dish.Id;
                        var product_id = prid.Product.Id;
                        var amount = prid.Amount;
                        using (ApplicationContext db = new ApplicationContext(App.dbPath))
                        {
                            db.ProductInDish.Add(new ProductInDish() { DishId = dish_id, ProductId = product_id, Amount = amount });
                            db.SaveChanges();
                        }
                    }
                    var calories = Dish.Calories;
                    var proteins = Dish.Proteins;
                    var fats = Dish.Fats;
                    var carbohydrates = Dish.Carbohydrates;
                    var name = Dish.Name;
                    var comment = Dish.Comment;
                    var image = Dish.Image;
                    var portion = Dish.Portion;
                    using (ApplicationContext db = new ApplicationContext(App.dbPath))
                    {
                        var dish = db.Dishes.Find(Dish.Id);
                        dish.Calories = calories;
                        dish.Proteins = proteins;
                        dish.Fats = fats;
                        dish.Carbohydrates = carbohydrates;
                        dish.Name = name;
                        dish.Comment = comment;
                        dish.Image = image;
                        dish.Portion = portion;
                        db.SaveChanges();
                    }

                    //TODO: how to change vm without remove-insert
                    int index = Dishes.IndexOf(Dishes.FirstOrDefault(x => x.Dish.Id == this.Dish.Id));
                    Dishes.Remove(Dishes.FirstOrDefault(x => x.Dish.Id == this.Dish.Id));
                    Dishes.Insert(index, this);
                }

                Back();
            }
            else
            {
                //TODO: alert error
            }
        }

        private void DeleteDish()
        {
            if (Dish.Id != 0)
            {
                using (ApplicationContext db = new ApplicationContext(App.dbPath))
                {
                    var dish = db.Dishes.Find(Dish.Id);
                    if (dish != null)
                    {
                        db.Dishes.Remove(dish);
                        db.SaveChanges();
                    }
                }
                Dishes.Remove(Dishes.FirstOrDefault(x => x.Dish.Id == this.Dish.Id));
            }

            Back();
        }

        private void Back()
        {
            Navigation.PopAsync();
        }

        private void ProductInDishListChanges(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.EnergyPriceChanged?.Invoke();
        }

        public void CalcEnergyPrice()
        {
            this.Dish.ProductInDish = this.ProductsInDish.Select(x => x.ProductInDish).ToList();
            this.Calories = this.ProductsInDish.Sum(x => x.Product.Calories / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (this.Portion != null && this.Portion > 0 ? (double)this.Portion : 1);
            this.Proteins = this.ProductsInDish.Sum(x => x.Product.Proteins / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (this.Portion != null && this.Portion > 0 ? (double)this.Portion : 1);
            this.Fats = this.ProductsInDish.Sum(x => x.Product.Fats / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (this.Portion != null && this.Portion > 0 ? (double)this.Portion : 1);
            this.Carbohydrates = this.ProductsInDish.Sum(x => x.Product.Carbohydrates / x.Product.Unit.StandardAmount * (x.Amount != null && x.Amount > 0 ? (double)x.Amount : 0)) / (this.Portion != null && this.Portion > 0 ? (double)this.Portion : 1);
        }
    }
}
