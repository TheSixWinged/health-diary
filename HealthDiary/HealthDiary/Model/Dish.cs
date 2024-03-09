using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthDiary.Model
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
        public string Comment { get; set; }
        public double? Portion { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        //public List<Product> Products { get; set; } = new List<Product>();
        public List<ProductInDish> ProductInDish { get; set; } = new List<ProductInDish>();

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Name) && !ProductInDish.Any(x => x.Amount == null || x.Amount <= 0) && Portion != null && Portion > 0;
        }
    }
}
