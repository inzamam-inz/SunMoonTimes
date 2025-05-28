using SunMoonTimes.Models;
using System;

namespace SunMoonTimes
{
    public class SolarPosition
    {
        public static GeoPosition GetPosition()
        {
            return GetPosition(DateTime.UtcNow);
        }

        // Calculate the sun's position (latitude and longitude) on Earth
        public static GeoPosition GetPosition(DateTime utcTime)
        {
            // Convert DateTime to Julian date
            var julianDate = HelperClass.DateTimeToJulianDate(utcTime);

            // Calculate the number of days since J2000.0
            var n = julianDate - 2451545.0;

            // Calculate mean solar longitude (degrees)
            var L = (280.460 + 0.9856474 * n) % 360;

            // Calculate mean anomaly (degrees)
            var g = (357.528 + 0.9856003 * n) % 360;

            // Convert to radians
            var gRad = g * Math.PI / 180.0;

            // Calculate ecliptic longitude (degrees)
            var eclipticLongitude = L + 1.915 * Math.Sin(gRad) + 0.020 * Math.Sin(2 * gRad);

            // Calculate ecliptic obliquity (degrees)
            var eclipticObliquity = 23.439 - 0.0000004 * n;

            // Convert to radians
            var eclipticLongitudeRad = eclipticLongitude * Math.PI / 180.0;
            var eclipticObliquityRad = eclipticObliquity * Math.PI / 180.0;

            // Calculate right ascension (radians)
            var rightAscension = Math.Atan2(
                Math.Cos(eclipticObliquityRad) * Math.Sin(eclipticLongitudeRad),
                Math.Cos(eclipticLongitudeRad)
            );

            // Calculate declination (radians)
            var declination = Math.Asin(
                Math.Sin(eclipticObliquityRad) * Math.Sin(eclipticLongitudeRad)
            );

            // Calculate Greenwich Mean Sidereal Time (GMST) in degrees
            var gmst = HelperClass.CalculateGMST(utcTime);

            // Convert right ascension to degrees
            var rightAscensionDeg = rightAscension * 180.0 / Math.PI;
            if (rightAscensionDeg < 0)
            {
                rightAscensionDeg += 360.0;
            }

            // Calculate the Sun's longitude (degrees)
            var longitude = (rightAscensionDeg - gmst) % 360;
            if (longitude < -180.0)
            {
                longitude += 360.0;
            }
            else if (longitude > 180.0)
            {
                longitude -= 360.0;
            }

            // Convert declination to degrees for latitude
            var latitude = declination * 180.0 / Math.PI;

            return new GeoPosition(latitude, longitude);
        }
    }
}