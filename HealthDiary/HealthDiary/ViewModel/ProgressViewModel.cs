using HealthDiary.Model;
using HealthDiary.Model.Context;
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
    public class ProgressViewModel : BaseViewModel
    {
        public ObservableCollection<PlanCompletionViewModel> PlanCompletions { get; set; } = new ObservableCollection<PlanCompletionViewModel>();

        public ICommand Back_cmd { protected set; get; }

        public ProgressViewModel(INavigation navigation) : base(navigation)
        {
            using (ApplicationContext db = new ApplicationContext(App.dbPath))
            {
                var plan_completions = db.PlanCompletions.Include(x => x.User).Where(x => x.UserId == App.CurrentUser.Id).ToList();
                foreach (var plan_completion in plan_completions)
                {
                    this.PlanCompletions.Add(new PlanCompletionViewModel(Navigation, plan_completion));
                }
            }
            //TODO: add sort plancompletions by date

            this.Back_cmd = new Command(Back);
        }

        private void Back()
        {
            Navigation.PopAsync();
        }
    }
}
