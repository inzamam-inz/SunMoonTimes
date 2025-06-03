using System;
using System.Collections.Generic;
using System.Text;

namespace SunMoonTimes.Models
{
    public struct FundamentalArguments
    {
        public double L;      // Mean longitude
        public double D;      // Mean elongation
        public double M;      // Sun's mean anomaly
        public double M_moon; // Moon's mean anomaly
        public double F;      // Argument of latitude
    }
}
