using System;
using System.Collections.Generic;
using System.Text;

namespace HealthDiary.Model
{
    public class Cup
    {
        public int Id { get; set; }
        public double WaterAmount { get; set; }
        public bool IsReadonly { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }
    }
}
