using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HealthDiary.Model
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double StandardAmount { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();

        [NotMapped]
        public string AmountName
        {
            get { return $"{StandardAmount} {Name}"; }
            set { }
        }
    }
}
