using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HealthDiary.Model
{
    public class Plan
    {
        public int Id { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
        public double WaterAmount { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public static Plan StandartPlan = new Plan()
        {
            Calories = 2200,
            Proteins = 170,
            Fats = 75,
            Carbohydrates = 220,
            WaterAmount = 2250
        };

        public bool IsValid()
        {
            return Calories > 0 && Proteins > 0 && Fats > 0 && Carbohydrates > 0 && WaterAmount > 0;
        }
    }
}
