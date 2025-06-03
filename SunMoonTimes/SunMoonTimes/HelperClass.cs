using System;

namespace SunMoonTimes
{
    /// <summary>
    /// Provides utility methods for astronomical calculations including angle normalization,
    /// Greenwich Mean Sidereal Time calculation, and Julian date conversion.
    /// </summary>
    public static class Helper
    {
        private const double JulianEpochJ2000 = 2451545.0;
        private const double DaysPerCentury = 36525.0;
        private const double SecondsPerMinute = 60.0;
        private const double MinutesPerHour = 60.0;
        private const double HoursPerDay = 24.0;
        private const double DegreesPerCircle = 360.0;

        /// <summary>
        /// Normalizes an angle to the range [0, 360) degrees.
        /// </summary>
        /// <param name="angle">The angle in degrees to normalize.</param>
        /// <returns>The normalized angle in the range [0, 360) degrees.</returns>
        public static double NormalizeAngle(double angle)
        {
            if (double.IsNaN(angle) || double.IsInfinity(angle))
                throw new ArgumentException("Angle must be a finite number.", nameof(angle));

            angle %= DegreesPerCircle;
            if (angle < 0)
            {
                angle += DegreesPerCircle;
            }
            return angle;
        }

        /// <summary>
        /// Calculates Greenwich Mean Sidereal Time (GMST) in degrees for the specified UTC time.
        /// </summary>
        /// <param name="utcTime">The UTC time for which to calculate GMST.</param>
        /// <returns>The GMST in degrees, normalized to the range [0, 360).</returns>
        /// <remarks>
        /// Uses the IAU 1982 formula for GMST calculation.
        /// </remarks>
        public static double CalculateGMST(DateTime utcTime)
        {
            double julianDate = DateTimeToJulianDate(utcTime);
            double T = (julianDate - JulianEpochJ2000) / DaysPerCentury;

            // IAU 1982 GMST formula
            double gmst = 280.46061837 +
                         360.98564736629 * (julianDate - JulianEpochJ2000) +
                         0.000387933 * T * T -
                         T * T * T / 38710000.0;

            return NormalizeAngle(gmst);
        }

        /// <summary>
        /// Converts a DateTime to Julian date.
        /// </summary>
        /// <param name="utcTime">The UTC DateTime to convert.</param>
        /// <returns>The corresponding Julian date.</returns>
        /// <remarks>
        /// Uses the algorithm from Jean Meeus' "Astronomical Algorithms".
        /// </remarks>
        public static double DateTimeToJulianDate(DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc && utcTime.Kind != DateTimeKind.Unspecified)
                throw new ArgumentException("DateTime must be in UTC.", nameof(utcTime));

            int year = utcTime.Year;
            int month = utcTime.Month;

            double day = utcTime.Day +
                        (utcTime.Hour +
                        (utcTime.Minute +
                        utcTime.Second / SecondsPerMinute) / MinutesPerHour) / HoursPerDay;

            // January and February are treated as months 13 and 14 of the previous year
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            // Gregorian calendar correction
            int A = year / 100;
            int B = 2 - A + (A / 4);

            return Math.Floor(365.25 * (year + 4716)) +
                   Math.Floor(30.6001 * (month + 1)) +
                   day + B - 1524.5;
        }

        /// <summary>
        /// Normalizes longitude to the range [-180, 180] degrees.
        /// </summary>
        public static double NormalizeLongitude(double longitude)
        {
            longitude %= DegreesPerCircle;

            if (longitude < -180.0)
                longitude += DegreesPerCircle;
            else if (longitude > 180.0)
                longitude -= DegreesPerCircle;

            return longitude;
        }
    }
}