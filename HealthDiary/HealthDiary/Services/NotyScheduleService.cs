using HealthDiary.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Plugin.LocalNotification;
using System.Linq;

namespace HealthDiary.Services
{
    public class NotyScheduleService
    {
        public TimeSpan startTime;
        public TimeSpan endTime;

        public TimeSpan scheduleIntervalEat;
        public TimeSpan scheduleIntervalWater;
        public TimeSpan delayEatBeforeNight;

        public bool isNotyAfterCompletion;

        public List<TimeSpan> scheduleEat = new List<TimeSpan>();
        public List<TimeSpan> scheduleWater = new List<TimeSpan>();

        public NotyScheduleService(TimeSpan startTime, TimeSpan endTime, TimeSpan scheduleIntervalEat, TimeSpan scheduleIntervalWater, TimeSpan delayEatBeforeNight, bool isNotyAfterCompletion)
        {
            this.startTime = startTime;
            this.endTime = endTime;

            this.scheduleIntervalEat = scheduleIntervalEat;
            this.scheduleIntervalWater = scheduleIntervalWater;
            this.delayEatBeforeNight = delayEatBeforeNight;

            this.isNotyAfterCompletion = isNotyAfterCompletion;

            for (TimeSpan i = this.startTime; i <= this.endTime.Subtract(this.delayEatBeforeNight); i = i.Add(this.scheduleIntervalEat))
                this.scheduleEat.Add(i);
            for (TimeSpan i = this.startTime; i <= this.endTime; i = i.Add(this.scheduleIntervalWater))
                this.scheduleWater.Add(i);
        }

        public void ScheduleNotifications(PlanCompletion planCompletion, bool is_cancel_closer_eat, bool is_cancel_closer_water)
        {
            if (planCompletion != null)
            {
                CancelNotifications();
                ScheduleEat(planCompletion, is_cancel_closer_eat);
                ScheduleWeeklyEat(planCompletion);
                ScheduleWater(planCompletion, is_cancel_closer_water);
                ScheduleWeeklyWater(planCompletion);
            }
        }

        public void CancelNotifications()
        {
            NotificationCenter.Current.CancelAll();
        }

        private void ScheduleEat(PlanCompletion planCompletion, bool is_cancel_closer)
        {
            if (planCompletion.Calories >= planCompletion.CaloriesPlan && !isNotyAfterCompletion)
            { }
            else
            {
                TimeSpan delay = new TimeSpan(0, 0, 0);
                if (is_cancel_closer)
                    delay = new TimeSpan(1, 0, 0);
                TimeSpan now = DateTime.Now.TimeOfDay.Add(delay);

                foreach (TimeSpan time in scheduleEat)
                {
                    if (time > now)
                    {
                        double retard = CalcRetardCompletion(planCompletion.Calories, planCompletion.CaloriesPlan, scheduleEat.IndexOf(time) + 1, scheduleEat.Count);
                        string description = SelectEatDescription(retard);

                        var noty = new NotificationRequest
                        {
                            BadgeNumber = 1,
                            Description = description,
                            Title = eatNotyTitle,
                            NotificationId = eatNotyId + scheduleEat.IndexOf(time),
                            NotifyTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, time.Hours, time.Minutes, time.Seconds)
                        };
                        NotificationCenter.Current.Show(noty);
                    }
                }
            }
        }

        private void ScheduleWeeklyEat(PlanCompletion planCompletion)
        {
            foreach (TimeSpan time in scheduleEat)
            {
                double retard = CalcRetardCompletion(0, planCompletion.CaloriesPlan, scheduleEat.IndexOf(time) + 1, scheduleEat.Count);
                string description = SelectEatDescription(retard);

                DateTime now = DateTime.Today.AddDays(1);
                var noty = new NotificationRequest
                {
                    BadgeNumber = 1,
                    Description = description,
                    Title = eatNotyTitle,
                    NotificationId = weeklyEatNotyId + scheduleEat.IndexOf(time),
                    NotifyTime = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, time.Seconds),
                    Repeats = NotificationRepeat.Daily
                };
                NotificationCenter.Current.Show(noty);
            }
        }

        private void ScheduleWater(PlanCompletion planCompletion, bool is_cancel_closer)
        {
            if (planCompletion.WaterAmount >= planCompletion.WaterAmountPlan && !isNotyAfterCompletion)
            { }
            else
            {
                TimeSpan delay = new TimeSpan(0, 0, 0);
                if (is_cancel_closer)
                    delay = new TimeSpan(0, 20, 0);
                TimeSpan now = DateTime.Now.TimeOfDay.Add(delay);

                foreach (TimeSpan time in scheduleWater)
                {
                    if (time > now)
                    {
                        double retard = CalcRetardCompletion(planCompletion.WaterAmount, planCompletion.WaterAmountPlan, scheduleWater.IndexOf(time) + 1, scheduleWater.Count);
                        string description = SelectWaterDescription(retard);

                        var noty = new NotificationRequest
                        {
                            BadgeNumber = 1,
                            Description = description,
                            Title = waterNotyTitle,
                            NotificationId = waterNotyId + scheduleWater.IndexOf(time),
                            NotifyTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, time.Hours, time.Minutes, time.Seconds)
                        };
                        NotificationCenter.Current.Show(noty);
                    }
                }
            }
        }

        private void ScheduleWeeklyWater(PlanCompletion planCompletion)
        {
            foreach (TimeSpan time in scheduleWater)
            {
                double retard = CalcRetardCompletion(0, planCompletion.WaterAmountPlan, scheduleWater.IndexOf(time) + 1, scheduleWater.Count);
                string description = SelectWaterDescription(retard);

                DateTime now = DateTime.Today.AddDays(1);
                var noty = new NotificationRequest
                {
                    BadgeNumber = 1,
                    Description = description,
                    Title = waterNotyTitle,
                    NotificationId = weeklyWaterNotyId + scheduleWater.IndexOf(time),
                    NotifyTime = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, time.Seconds),
                    Repeats = NotificationRepeat.Daily
                };
                NotificationCenter.Current.Show(noty);
            }
        }

        private double CalcRetardCompletion(double current, double plan, int index_noty, int count_noty)
        {
            return ((double)index_noty / (double)count_noty * 100) - (current / plan * 100);
        }

        private string SelectEatDescription(double retard)
        {
            if (retard >= 75)
                return eatNotyDescriptionCritical;
            else if (retard >= 50)
                return eatNotyDescriptionAttention;
            else
                return eatNotyDescriptionNormal;
        }

        private string SelectWaterDescription(double retard)
        {
            if (retard >= 50)
                return waterNotyDescriptionCritical;
            else if (retard >= 25)
                return waterNotyDescriptionAttention;
            else
                return waterNotyDescriptionNormal;
        }

        private int waterNotyId = 100;
        private int weeklyWaterNotyId = 150;
        private string waterNotyTitle = "Время пить воду";
        private string waterNotyDescriptionNormal = "Выпейте воду и подтвердите";
        private string waterNotyDescriptionAttention = "Вы отстаете от плана, необходимо выпить воды!";
        private string waterNotyDescriptionCritical = "Водный баланс под угрозой, срочно выпейте воды!";

        private int eatNotyId = 200;
        private int weeklyEatNotyId = 250;
        private string eatNotyTitle = "Время перекусить";
        private string eatNotyDescriptionNormal = "Съешьте что-нибудь и подтвердите";
        private string eatNotyDescriptionAttention = "Вы отстаете от плана, необходимо перекусить!";
        private string eatNotyDescriptionCritical = "Здоровье под угрозой, срочно съешьте что-нибудь!";
    }
}
