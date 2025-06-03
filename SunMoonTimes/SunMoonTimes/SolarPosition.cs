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
        /// <param name="time">The UTC time for which to calculate the Sun's position.</param>
        /// <returns>The geographical position where the Sun is directly overhead.</returns>
        /// <remarks>
        /// Uses a simplified solar position algorithm suitable for most applications.
        /// For high-precision applications, consider using more sophisticated algorithms like VSOP87.
        /// </remarks>
        public static GeoPosition GetPosition(DateTime time)
        {
            var utcTime = Helper.ToUTCIfNeeded(time);

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

        /// <summary>
        /// Calculates the Sun's azimuth and elevation from an observer's position.
        /// </summary>
        /// <param name="observerPosition">The observer's geographical position.</param>
        /// <param name="time">The UTC time for the calculation.</param>
        /// <returns>A tuple containing azimuth (degrees from North) and elevation (degrees above horizon).</returns>
        public static (double azimuth, double elevation) GetAzimuthElevation(GeoPosition observerPosition, DateTime time)
        {
            var utcTime = Helper.ToUTCIfNeeded(time);

            // Get solar coordinates
            var (rightAscension, declination) = GetSolarCoordinates(utcTime);

            // Calculate local sidereal time
            double gmst = Helper.CalculateGMST(utcTime);
            double localSiderealTime = gmst + observerPosition.Longitude;
            localSiderealTime = Helper.NormalizeAngle(localSiderealTime);

            // Calculate hour angle
            double hourAngle = localSiderealTime - rightAscension;
            hourAngle = Helper.NormalizeAngle(hourAngle);
            if (hourAngle > 180.0) hourAngle -= 360.0;

            double hourAngleRad = hourAngle * DegreesToRadians;
            double declinationRad = declination * DegreesToRadians;
            double latitudeRad = observerPosition.Latitude * DegreesToRadians;

            // Calculate elevation (altitude)
            double elevationRad = Math.Asin(
                Math.Sin(latitudeRad) * Math.Sin(declinationRad) +
                Math.Cos(latitudeRad) * Math.Cos(declinationRad) * Math.Cos(hourAngleRad)
            );

            double elevation = elevationRad * RadiansToDegrees;

            // Calculate azimuth
            double azimuthRad = Math.Atan2(
                -Math.Sin(hourAngleRad),
                Math.Tan(declinationRad) * Math.Cos(latitudeRad) - Math.Sin(latitudeRad) * Math.Cos(hourAngleRad)
            );

            double azimuth = azimuthRad * RadiansToDegrees;
            azimuth = Helper.NormalizeAngle(azimuth);

            return (azimuth, elevation);
        }

        /// <summary>
        /// Calculates the Sun's azimuth and elevation from an observer's current position and time.
        /// </summary>
        /// <param name="observerPosition">The observer's geographical position.</param>
        /// <returns>A tuple containing azimuth (degrees from North) and elevation (degrees above horizon).</returns>
        public static (double azimuth, double elevation) GetAzimuthElevation(GeoPosition observerPosition)
        {
            return GetAzimuthElevation(observerPosition, DateTime.UtcNow);
        }

        /// <summary>
        /// Calculates sunrise and sunset times for a given date and observer position.
        /// </summary>
        /// <param name="observerPosition">The observer's geographical position.</param>
        /// <param name="date">The date for which to calculate sunrise/sunset.</param>
        /// <param name="sunriseElevation">The elevation angle for sunrise/sunset (typically -0.833 degrees for geometric horizon).</param>
        /// <returns>A tuple containing sunrise and sunset times in UTC, or null if sun doesn't rise/set.</returns>
        public static (DateTime? sunrise, DateTime? sunset) GetRiseSet(GeoPosition observerPosition, DateTime date, double sunriseElevation = -0.833)
        {
            // Use noon as starting point for the date
            var noonUtc = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc);

            // Get solar declination for this date
            var (_, declination) = GetSolarCoordinates(noonUtc);

            double latitudeRad = observerPosition.Latitude * DegreesToRadians;
            double declinationRad = declination * DegreesToRadians;
            double sunriseElevationRad = sunriseElevation * DegreesToRadians;

            // Calculate hour angle for sunrise/sunset
            double cosHourAngle = (Math.Sin(sunriseElevationRad) - Math.Sin(latitudeRad) * Math.Sin(declinationRad)) /
                                 (Math.Cos(latitudeRad) * Math.Cos(declinationRad));

            // Check for polar day/night
            if (cosHourAngle > 1.0)
            {
                return (null, null); // Sun never rises (polar night)
            }
            else if (cosHourAngle < -1.0)
            {
                return (null, null); // Sun never sets (polar day)
            }

            double hourAngle = Math.Acos(cosHourAngle) * RadiansToDegrees;

            // Calculate solar noon time
            double solarNoon = 12.0 - (observerPosition.Longitude / 15.0);

            // Calculate sunrise and sunset times
            double sunriseHour = solarNoon - (hourAngle / 15.0);
            double sunsetHour = solarNoon + (hourAngle / 15.0);

            // Normalize hours to [0, 24)
            sunriseHour = (sunriseHour + 24) % 24;
            sunsetHour = (sunsetHour + 24) % 24;

            DateTime? sunrise = null;
            DateTime? sunset = null;

            if (sunriseHour >= 0 && sunriseHour < 24)
            {
                int hours = (int)sunriseHour;
                int minutes = (int)((sunriseHour - hours) * 60);
                int seconds = (int)(((sunriseHour - hours) * 60 - minutes) * 60);
                sunrise = new DateTime(date.Year, date.Month, date.Day, hours, minutes, seconds, DateTimeKind.Utc);
            }

            if (sunsetHour >= 0 && sunsetHour < 24)
            {
                int hours = (int)sunsetHour;
                int minutes = (int)((sunsetHour - hours) * 60);
                int seconds = (int)(((sunsetHour - hours) * 60 - minutes) * 60);
                sunset = new DateTime(date.Year, date.Month, date.Day, hours, minutes, seconds, DateTimeKind.Utc);
            }

            return (sunrise, sunset);
        }

        /// <summary>
        /// Calculates sunrise and sunset times for today at the observer's position.
        /// </summary>
        /// <param name="observerPosition">The observer's geographical position.</param>
        /// <param name="sunriseElevation">The elevation angle for sunrise/sunset (typically -0.833 degrees for geometric horizon).</param>
        /// <returns>A tuple containing sunrise and sunset times in UTC, or null if sun doesn't rise/set.</returns>
        public static (DateTime? sunrise, DateTime? sunset) GetRiseSet(GeoPosition observerPosition, double sunriseElevation = -0.833)
        {
            return GetRiseSet(observerPosition, DateTime.UtcNow.Date, sunriseElevation);
        }

        /// <summary>
        /// Gets the solar coordinates (right ascension and declination) for a given time.
        /// </summary>
        /// <param name="utcTime">The UTC time.</param>
        /// <returns>A tuple containing right ascension and declination in degrees.</returns>
        public static (double rightAscension, double declination) GetSolarCoordinates(DateTime utcTime)
        {
            double julianDate = Helper.DateTimeToJulianDate(utcTime);
            double n = julianDate - JulianEpochJ2000;

            // Calculate mean solar longitude and mean anomaly
            double meanSolarLongitude = Helper.NormalizeAngle(280.460 + 0.9856474 * n);
            double meanAnomaly = Helper.NormalizeAngle(357.528 + 0.9856003 * n);
            double meanAnomalyRad = meanAnomaly * DegreesToRadians;

            // Calculate true ecliptic longitude
            double eclipticLongitude = meanSolarLongitude +
                                     1.915 * Math.Sin(meanAnomalyRad) +
                                     0.020 * Math.Sin(2 * meanAnomalyRad);

            // Earth's axial tilt
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

            double rightAscensionDeg = Helper.NormalizeAngle(rightAscension * RadiansToDegrees);
            double declinationDeg = declination * RadiansToDegrees;

            return (rightAscensionDeg, declinationDeg);
        }

        /// <summary>
        /// Calculates the equation of time correction in minutes.
        /// </summary>
        /// <param name="utcTime">The UTC time.</param>
        /// <returns>The equation of time in minutes.</returns>
        private static double GetEquationOfTime(DateTime utcTime)
        {
            double julianDate = Helper.DateTimeToJulianDate(utcTime);
            double n = julianDate - JulianEpochJ2000;

            double meanSolarLongitude = Helper.NormalizeAngle(280.460 + 0.9856474 * n);
            double meanAnomaly = Helper.NormalizeAngle(357.528 + 0.9856003 * n);

            double meanSolarLongitudeRad = meanSolarLongitude * DegreesToRadians;
            double meanAnomalyRad = meanAnomaly * DegreesToRadians;

            double eot = 4.0 * (meanSolarLongitudeRad - 0.0057183 - Math.Atan2(
                Math.Tan(meanSolarLongitudeRad),
                Math.Cos(23.44 * DegreesToRadians))) * RadiansToDegrees;

            eot += 4.0 * 0.0430398 * Math.Sin(2.0 * meanAnomalyRad);

            return eot;
        }
    }
}