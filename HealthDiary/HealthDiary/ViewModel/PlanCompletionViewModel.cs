using HealthDiary.Model;
using HealthDiary.Model.Extensions;
using HealthDiary.ViewModel.Templates;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HealthDiary.ViewModel
{
    public class PlanCompletionViewModel : BaseViewModel
    {
        public PlanCompletion PlanCompletion { get; set; }

        public PlanCompletionViewModel(INavigation navigation, PlanCompletion planCompletion) : base(navigation)
        {
            this.PlanCompletion = planCompletion;
        }

        public string Date
        {
            get { return PlanCompletion.Date.ToString("dd-MM-yyyy"); }
            set { }
        }

        public string Calories
        {
            get { return $"{PlanCompletion.Calories.RoundToSignDigits(2)} / {PlanCompletion.CaloriesPlan.RoundToSignDigits(2)}"; }
            set { }
        }

        public string CaloriesPercent
        {
            get { return $"{(PlanCompletion.Calories / PlanCompletion.CaloriesPlan * 100).RoundToSignDigits(2)}%"; }
            set { }
        }

        public string Proteins
        {
            get { return $"{PlanCompletion.Proteins.RoundToSignDigits(2)} / {PlanCompletion.ProteinsPlan.RoundToSignDigits(2)}"; }
            set { }
        }

        public string ProteinsPercent
        {
            get { return $"{(PlanCompletion.Proteins / PlanCompletion.ProteinsPlan * 100).RoundToSignDigits(2)}%"; }
            set { }
        }

        public string Fats
        {
            get { return $"{PlanCompletion.Fats.RoundToSignDigits(2)} / {PlanCompletion.FatsPlan.RoundToSignDigits(2)}"; }
            set { }
        }

        public string FatsPercent
        {
            get { return $"{(PlanCompletion.Fats / PlanCompletion.FatsPlan * 100).RoundToSignDigits(2)}%"; }
            set { }
        }

        public string Carbohydrates
        {
            get { return $"{PlanCompletion.Carbohydrates.RoundToSignDigits(2)} / {PlanCompletion.CarbohydratesPlan.RoundToSignDigits(2)}"; }
            set { }
        }

        public string CarbohydratesPercent
        {
            get { return $"{(PlanCompletion.Carbohydrates / PlanCompletion.CarbohydratesPlan * 100).RoundToSignDigits(2)}%"; }
            set { }
        }

        public string WaterAmount
        {
            get { return $"{PlanCompletion.WaterAmount.RoundToSignDigits(2)} / {PlanCompletion.WaterAmountPlan.RoundToSignDigits(2)}"; }
            set { }
        }

        public string WaterAmountPercent
        {
            get { return $"{(PlanCompletion.WaterAmount / PlanCompletion.WaterAmountPlan * 100).RoundToSignDigits(2)}%"; }
            set { }
        }
    }
}
