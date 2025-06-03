using SunMoonTimes.Models;
using System;

namespace SunMoonTimes
{
    /// <summary>
    /// Provides methods to calculate the Sun's subsolar point (geographical position) on Earth.
    /// </summary>
    public static class SolarPosition
    {
        private const double JulianEpochJ2000 = 2451545.0;
        private const double DegreesPerCircle = 360.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;
        private const double DegreesToRadians = Math.PI / 180.0;

        /// <summary>
        /// Calculates the Sun's current subsolar point using the current UTC time.
        /// </summary>
        /// <returns>The geographical position where the Sun is directly overhead.</returns>
        public static GeoPosition GetPosition()
        {
            return GetPosition(DateTime.UtcNow);
        }

        /// <summary>
        /// Calculates the Sun's subsolar point for the specified UTC time.
        /// </summary>
        /// <param name="utcTime">The UTC time for which to calculate the Sun's position.</param>
        /// <returns>The geographical position where the Sun is directly overhead.</returns>
        /// <remarks>
        /// Uses a simplified solar position algorithm suitable for most applications.
        /// For high-precision applications, consider using more sophisticated algorithms like VSOP87.
        /// </remarks>
        public static GeoPosition GetPosition(DateTime utcTime)
        {
            if (utcTime.Kind == DateTimeKind.Local)
            {
                utcTime = utcTime.ToUniversalTime();
            }
            else if (utcTime.Kind == DateTimeKind.Unspecified)
            {
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            }

            double julianDate = Helper.DateTimeToJulianDate(utcTime);
            double n = julianDate - JulianEpochJ2000;

            // Calculate mean solar longitude and mean anomaly
            double meanSolarLongitude = Helper.NormalizeAngle(280.460 + 0.9856474 * n);
            double meanAnomaly = Helper.NormalizeAngle(357.528 + 0.9856003 * n);
            double meanAnomalyRad = meanAnomaly * DegreesToRadians;

            // Calculate true ecliptic longitude using equation of center
            double eclipticLongitude = meanSolarLongitude +
                                     1.915 * Math.Sin(meanAnomalyRad) +
                                     0.020 * Math.Sin(2 * meanAnomalyRad);

            // Earth's axial tilt (obliquity of the ecliptic)
            double eclipticObliquity = 23.439 - 0.0000004 * n;

            double eclipticLongitudeRad = eclipticLongitude * DegreesToRadians;
            double eclipticObliquityRad = eclipticObliquity * DegreesToRadians;

            // Convert to equatorial coordinates
            double rightAscension = Math.Atan2(
                Math.Cos(eclipticObliquityRad) * Math.Sin(eclipticLongitudeRad),
                Math.Cos(eclipticLongitudeRad)
            );

            double declination = Math.Asin(
                Math.Sin(eclipticObliquityRad) * Math.Sin(eclipticLongitudeRad)
            );

            double gmst = Helper.CalculateGMST(utcTime);
            double rightAscensionDeg = Helper.NormalizeAngle(rightAscension * RadiansToDegrees);

            // Calculate subsolar longitude
            double longitude = rightAscensionDeg - gmst;
            longitude = Helper.NormalizeLongitude(longitude);

            double latitude = declination * RadiansToDegrees;

            return new GeoPosition(latitude, longitude);
        }
    }
}