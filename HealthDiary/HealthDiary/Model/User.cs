using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace HealthDiary.Model
{
    public class User
    {
        public int Id { get; set; }
        //TODO: set unique login
        public string Login { get; set; }
        public string Password { get; set; }
        public double Growth { get; set; }
        public double Weight { get; set; }

        [JsonPropertyName("Date_Of_Birth")]
        public DateTime DateOfBirth { get; set; }


        [JsonPropertyName("Gender_Id")]
        public int? GenderId { get; set; }
        [JsonIgnore]
        public Gender Gender { get; set; }

        [JsonPropertyName("Physical_Activity_Type_Id")]
        public int? PhysicalActivityTypeId { get; set; }
        [JsonIgnore]
        public PhysicalActivityType PhysicalActivityType { get; set; }

        [JsonIgnore]
        public int? PlanId { get; set; }

        [ForeignKey("PlanId"), JsonIgnore]
        public Plan Plan { get; set; }

        [JsonIgnore]
        public List<Product> Products { get; set; } = new List<Product>();
        [JsonIgnore]
        public List<Dish> Dishes { get; set; } = new List<Dish>();
        [JsonIgnore]
        public List<Cup> Cups { get; set; } = new List<Cup>();
        [JsonIgnore]
        public List<PlanCompletion> PlanCompletions { get; set; } = new List<PlanCompletion>();

        //TODO: del this method for tests
        public double CalculatePlanCalories(DateTime birthdate, Gender gender, PhysicalActivityType type, double weight, double growth)
        {
            double gender_rate = 0;
            double type_rate = 0;
            DateTime now = DateTime.Now.Date; //1

            int age = now.Year - birthdate.Year - 1; //2

            if (age < 0) //3
                age = 0; //4
            else if (now.Month > birthdate.Month || now.Month == birthdate.Month && now.Day >= birthdate.Day) //5
                age++; //6

            if (gender != null) //7
                gender_rate = gender.Rate; //8

            if (type != null) //9
                type_rate = type.Rate; //10

            double cal = (weight * 10 + growth * 6.25 - 5 * age + gender_rate) * (type_rate); //11
            return cal; //12
        }

        public Plan CalculatePlan()
        {
            DateTime now = DateTime.Now.Date;
            double cal = (this.Weight * 10 + this.Growth * 6.25 - 5 * (now.Year - this.DateOfBirth.Year - 1
                + ((now.Month > this.DateOfBirth.Month || now.Month == this.DateOfBirth.Month && now.Day >= this.DateOfBirth.Day) ? 1 : 0))
                + (this.Gender != null ? this.Gender.Rate : 0)) * (this.PhysicalActivityType != null ? this.PhysicalActivityType.Rate : 0);
            if (cal < 0)
                cal = 0;
            double prot = (0.3 * cal) / 4;
            double fat = (0.3 * cal) / 9;
            double carb = (0.4 * cal) / 4;

            double wat = this.Weight * 30;
            if (wat < 0)
                wat = 0;

            Plan plan = new Plan()
            {
                Calories = cal,
                Proteins = prot,
                Fats = fat,
                Carbohydrates = carb,
                WaterAmount = wat
            };
            plan = plan.IsValid() ? plan : Plan.StandartPlan;
            if (this.Plan == null)
                return plan;
            else
            {
                this.Plan.Calories = plan.Calories;
                this.Plan.Proteins = plan.Proteins;
                this.Plan.Fats = plan.Fats;
                this.Plan.Carbohydrates = plan.Carbohydrates;
                this.Plan.WaterAmount = plan.WaterAmount;
                return null;
            }
        }
    }
}
