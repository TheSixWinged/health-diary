using System;
using System.Collections.Generic;
using System.Text;

namespace HealthDiary.Model
{
    public class PhysicalActivityType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Rate { get; set; }
        public string Comment { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}
