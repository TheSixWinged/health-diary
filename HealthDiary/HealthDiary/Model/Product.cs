using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HealthDiary.Model
{
    public class Product //: ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }

        [JsonPropertyName("Readonly")]
        public bool IsReadonly { get; set; }

        [JsonPropertyName("Unit_Id")]
        public int UnitId { get; set; }
        public Unit Unit { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }

        //public List<Dish> Dishes { get; set; } = new List<Dish>();
        public List<ProductInDish> ProductInDish { get; set; } = new List<ProductInDish>();

        /*public object Clone()
        {
            return new Product { Id = this.Id, Name = this.Name, Image = this.Image, Calories = this.Calories,
                Proteins = this.Proteins, Fats = this.Fats, Carbohydrates = this.Carbohydrates,
                IsReadonly = this.IsReadonly, UnitId = this.UnitId, Unit = this.Unit, UserId = this.UserId, User = this.User };
        }*/

        public bool IsValid()
        {
            return this.Unit != null && !String.IsNullOrEmpty(Name) && this.Calories >= 0 && this.Proteins >= 0 && this.Fats >= 0 && this.Carbohydrates >= 0;
        }
    }
}
