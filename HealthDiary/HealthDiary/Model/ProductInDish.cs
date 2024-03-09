using System;
using System.Collections.Generic;
using System.Text;

namespace HealthDiary.Model
{
    public class ProductInDish
    {
        public double? Amount { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
    }
}
