using System;
using SunMoonTimes.Models;

namespace SunMoonTimes
{
    public class LunarPosition
    {
        public static GeoPosition GetPosition()
        {
            return GetPosition(DateTime.UtcNow);
        }

        // Calculate the moon's position (latitude, longitude, distance) on Earth
        public static GeoPosition GetPosition(DateTime utcTime)
        {
            // Convert DateTime to Julian date
            var jd = HelperClass.DateTimeToJulianDate(utcTime);

            // Time in Julian centuries since J2000.0
            var T = (jd - 2451545.0) / 36525.0;

            // Moon's mean longitude (degrees)
            var L = 218.316 + 13.176396 * 36525.0 * T;

            // Moon's mean elongation (degrees)
            var D = 297.8502 + 445267.1115 * T - 0.0016300 * T * T + T * T * T / 545868.0 - T * T * T * T / 113065000.0;

            // Sun's mean anomaly (degrees)
            var M = 357.5291 + 35999.0503 * T - 0.0001559 * T * T - 0.00000048 * T * T * T;

            // Moon's mean anomaly (degrees)
            var M_moon = 134.9634 + 477198.8675 * T + 0.0087414 * T * T + T * T * T / 69699.0 - T * T * T * T / 14712000.0;

            // Moon's argument of latitude (degrees)
            var F = 93.2720 + 483202.0175 * T - 0.0036539 * T * T - T * T * T / 3526000.0 + T * T * T * T / 863310000.0;

            // Normalize to 0-360 degrees
            L = HelperClass.NormalizeAngle(L);
            D = HelperClass.NormalizeAngle(D);
            M = HelperClass.NormalizeAngle(M);
            M_moon = HelperClass.NormalizeAngle(M_moon);
            F = HelperClass.NormalizeAngle(F);

            // Convert to radians
            var D_rad = D * Math.PI / 180.0;
            var M_rad = M * Math.PI / 180.0;
            var M_moon_rad = M_moon * Math.PI / 180.0;
            var F_rad = F * Math.PI / 180.0;

            // Ecliptic longitude calculation with perturbations
            var lambda = L + 6.289 * Math.Sin(M_moon_rad)
                             + 1.274 * Math.Sin(2 * D_rad - M_moon_rad)
                             + 0.658 * Math.Sin(2 * D_rad)
                             + 0.214 * Math.Sin(2 * M_moon_rad)
                             - 0.186 * Math.Sin(M_rad)
                             - 0.114 * Math.Sin(2 * F_rad)
                             + 0.059 * Math.Sin(2 * D_rad - 2 * M_moon_rad)
                             + 0.057 * Math.Sin(2 * D_rad - M_rad - M_moon_rad)
                             + 0.053 * Math.Sin(2 * D_rad + M_moon_rad)
                             + 0.046 * Math.Sin(2 * D_rad - M_rad)
                             + 0.041 * Math.Sin(D_rad)
                             - 0.035 * Math.Sin(D_rad + M_moon_rad)
                             - 0.030 * Math.Sin(D_rad - M_moon_rad);

            // Ecliptic latitude calculation with perturbations
            var beta = 5.128 * Math.Sin(F_rad)
                         + 0.280 * Math.Sin(M_moon_rad + F_rad)
                         + 0.277 * Math.Sin(M_moon_rad - F_rad)
                         + 0.173 * Math.Sin(2 * D_rad - F_rad)
                         + 0.055 * Math.Sin(2 * D_rad - M_moon_rad + F_rad)
                         + 0.046 * Math.Sin(2 * D_rad - M_moon_rad - F_rad)
                         + 0.033 * Math.Sin(2 * D_rad + F_rad)
                         + 0.017 * Math.Sin(2 * M_moon_rad + F_rad);

            // Normalize lambda to 0-360 degrees
            lambda = HelperClass.NormalizeAngle(lambda);

            // Calculate the Moon's distance (not used in this example but useful)
            var distance = 385000.56 - 20905.12 * Math.Cos(M_moon_rad)
                             - 3699.11 * Math.Cos(2 * D_rad - M_moon_rad)
                             - 2956.21 * Math.Cos(2 * D_rad)
                             - 569.92 * Math.Cos(2 * M_moon_rad);

            // Calculate obliquity of the ecliptic (degrees)
            var obliquity = 23.439291 - 0.0130042 * T - 0.00000016 * T * T + 0.000000504 * T * T * T;

            // Convert to radians
            var lambda_rad = lambda * Math.PI / 180.0;
            var beta_rad = beta * Math.PI / 180.0;
            var obliquity_rad = obliquity * Math.PI / 180.0;

            // Calculate equatorial coordinates (right ascension and declination)
            var sin_ra = Math.Sin(lambda_rad) * Math.Cos(obliquity_rad) -
                            Math.Tan(beta_rad) * Math.Sin(obliquity_rad);
            var cos_ra = Math.Cos(lambda_rad);

            // Calculate right ascension (radians)
            var ra = Math.Atan2(sin_ra, cos_ra);

            // Calculate declination (radians)
            var dec = Math.Asin(Math.Sin(beta_rad) * Math.Cos(obliquity_rad) +
                                   Math.Cos(beta_rad) * Math.Sin(obliquity_rad) * Math.Sin(lambda_rad));

            // Convert right ascension to degrees
            var ra_deg = ra * 180.0 / Math.PI;
            if (ra_deg < 0)
            {
                ra_deg += 360.0;
            }

            // Calculate Greenwich Mean Sidereal Time (GMST)
            var gmst = HelperClass.CalculateGMST(utcTime);

            // Calculate the Moon's longitude (degrees)
            var longitude = (ra_deg - gmst) % 360;
            if (longitude < -180.0)
            {
                longitude += 360.0;
            }
            else if (longitude > 180.0)
            {
                longitude -= 360.0;
            }

            // Convert declination to degrees for latitude
            var latitude = dec * 180.0 / Math.PI;

            return new GeoPosition(latitude, longitude);
        }
    }
}