using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HealthDiary.Model
{
    public class PlanCompletion
    {
        public int Id { get; set; }
        //set unique date for user
        public DateTime Date { get; set; }
        public double Calories { get; set; }
        public double CaloriesPlan { get; set; }
        public double Proteins { get; set; }
        public double ProteinsPlan { get; set; }
        public double Fats { get; set; }
        public double FatsPlan { get; set; }
        public double Carbohydrates { get; set; }
        public double CarbohydratesPlan { get; set; }
        public double WaterAmount { get; set; }
        public double WaterAmountPlan { get; set; }

        [JsonPropertyName("User_Id")]
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
    }
}
