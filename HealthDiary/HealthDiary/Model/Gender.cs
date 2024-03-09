using System;
using System.Collections.Generic;
using System.Text;

namespace HealthDiary.Model
{
    public class Gender
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Rate { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}
