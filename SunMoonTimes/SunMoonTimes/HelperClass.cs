using System;

namespace SunMoonTimes
{
    public class HelperClass
    {
        public static double NormalizeAngle(double angle)
        {
            angle %= 360.0;
            if (angle < 0)
            {
                angle += 360.0;
            }

            return angle;
        }

        // Calculate Greenwich Mean Sidereal Time (GMST) in degrees
        public static double CalculateGMST(DateTime utcTime)
        {
            // Convert DateTime to Julian date
            var julianDate = HelperClass.DateTimeToJulianDate(utcTime);

            // Calculate the number of days since J2000.0
            var T = (julianDate - 2451545.0) / 36525.0;

            // Calculate GMST in hours
            var gmst = 280.46061837 +
                         360.98564736629 * (julianDate - 2451545.0) +
                         0.000387933 * T * T -
                         T * T * T / 38710000.0;

            // Convert to degrees and normalize to range [0, 360)
            return HelperClass.NormalizeAngle(gmst);
        }

        // Convert DateTime to Julian date
        public static double DateTimeToJulianDate(DateTime utcTime)
        {
            // Formula from Jean Meeus' "Astronomical Algorithms"
            var year = utcTime.Year;
            var month = utcTime.Month;
            var day = utcTime.Day +
                        (utcTime.Hour +
                        (utcTime.Minute +
                        utcTime.Second / 60.0) / 60.0) / 24.0;

            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            var A = year / 100;
            var B = 2 - A + (A / 4);

            var JD = Math.Floor(365.25 * (year + 4716)) +
                       Math.Floor(30.6001 * (month + 1)) +
                       day + B - 1524.5;

            return JD;
        }
    }
}
