using System;
using System.Collections.Generic;
using System.Text;

namespace HealthDiary.Model.Extensions
{
    public static class DoubleExtensions
    {
        public static double RoundToSignDigits(this double value, int digits)
        {
            return Math.Round(value, digits);
        }
    }
}
