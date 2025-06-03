using System;
using SunMoonTimes.Models;

namespace SunMoonTimes
{
    /// <summary>
    /// Provides methods to calculate the Moon's subsolar point (geographical position) on Earth.
    /// </summary>
    public static class LunarPosition
    {
        private const double JulianEpochJ2000 = 2451545.0;
        private const double DaysPerCentury = 36525.0;
        private const double DegreesPerCircle = 360.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;
        private const double DegreesToRadians = Math.PI / 180.0;

        /// <summary>
        /// Calculates the Moon's current subsolar point using the current UTC time.
        /// </summary>
        /// <returns>The geographical position where the Moon is directly overhead.</returns>
        public static GeoPosition GetPosition()
        {
            return GetPosition(DateTime.UtcNow);
        }

        /// <summary>
        /// Calculates the Moon's subsolar point for the specified UTC time.
        /// </summary>
        /// <param name="utcTime">The UTC time for which to calculate the Moon's position.</param>
        /// <returns>The geographical position where the Moon is directly overhead.</returns>
        /// <remarks>
        /// Uses a truncated ELP-2000 lunar theory with major perturbation terms.
        /// Accuracy is typically within a few arcminutes for modern dates.
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
            double T = (julianDate - JulianEpochJ2000) / DaysPerCentury;

            var fundamentalArguments = CalculateFundamentalArguments(T);
            var eclipticCoordinates = CalculateEclipticCoordinates(fundamentalArguments);
            var equatorialCoordinates = ConvertToEquatorialCoordinates(eclipticCoordinates, T);

            return CalculateSubsolarPoint(equatorialCoordinates, utcTime);
        }

        /// <summary>
        /// Calculates the fundamental arguments of lunar motion.
        /// </summary>
        private static FundamentalArguments CalculateFundamentalArguments(double T)
        {
            double meanLongitude = 218.316 + 13.176396 * DaysPerCentury * T;
            double meanElongation = 297.8502 + 445267.1115 * T - 0.0016300 * T * T +
                                  T * T * T / 545868.0 - T * T * T * T / 113065000.0;
            double sunMeanAnomaly = 357.5291 + 35999.0503 * T - 0.0001559 * T * T -
                                  0.00000048 * T * T * T;
            double moonMeanAnomaly = 134.9634 + 477198.8675 * T + 0.0087414 * T * T +
                                   T * T * T / 69699.0 - T * T * T * T / 14712000.0;
            double argumentOfLatitude = 93.2720 + 483202.0175 * T - 0.0036539 * T * T -
                                      T * T * T / 3526000.0 + T * T * T * T / 863310000.0;

            return new FundamentalArguments
            {
                L = Helper.NormalizeAngle(meanLongitude),
                D = Helper.NormalizeAngle(meanElongation),
                M = Helper.NormalizeAngle(sunMeanAnomaly),
                M_moon = Helper.NormalizeAngle(moonMeanAnomaly),
                F = Helper.NormalizeAngle(argumentOfLatitude)
            };
        }

        /// <summary>
        /// Calculates ecliptic coordinates using perturbation theory.
        /// </summary>
        private static EclipticCoordinates CalculateEclipticCoordinates(FundamentalArguments args)
        {
            double D_rad = args.D * DegreesToRadians;
            double M_rad = args.M * DegreesToRadians;
            double M_moon_rad = args.M_moon * DegreesToRadians;
            double F_rad = args.F * DegreesToRadians;

            // Major longitude perturbations
            double lambda = args.L + 6.289 * Math.Sin(M_moon_rad)
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

            // Major latitude perturbations
            double beta = 5.128 * Math.Sin(F_rad)
                        + 0.280 * Math.Sin(M_moon_rad + F_rad)
                        + 0.277 * Math.Sin(M_moon_rad - F_rad)
                        + 0.173 * Math.Sin(2 * D_rad - F_rad)
                        + 0.055 * Math.Sin(2 * D_rad - M_moon_rad + F_rad)
                        + 0.046 * Math.Sin(2 * D_rad - M_moon_rad - F_rad)
                        + 0.033 * Math.Sin(2 * D_rad + F_rad)
                        + 0.017 * Math.Sin(2 * M_moon_rad + F_rad);

            return new EclipticCoordinates
            {
                Lambda = Helper.NormalizeAngle(lambda),
                Beta = beta
            };
        }

        /// <summary>
        /// Converts ecliptic coordinates to equatorial coordinates.
        /// </summary>
        private static EquatorialCoordinates ConvertToEquatorialCoordinates(EclipticCoordinates ecliptic, double T)
        {
            // Obliquity of the ecliptic with nutation
            double obliquity = 23.439291 - 0.0130042 * T - 0.00000016 * T * T + 0.000000504 * T * T * T;

            double lambda_rad = ecliptic.Lambda * DegreesToRadians;
            double beta_rad = ecliptic.Beta * DegreesToRadians;
            double obliquity_rad = obliquity * DegreesToRadians;

            double sin_ra = Math.Sin(lambda_rad) * Math.Cos(obliquity_rad) -
                           Math.Tan(beta_rad) * Math.Sin(obliquity_rad);
            double cos_ra = Math.Cos(lambda_rad);

            double rightAscension = Math.Atan2(sin_ra, cos_ra);
            double declination = Math.Asin(Math.Sin(beta_rad) * Math.Cos(obliquity_rad) +
                                          Math.Cos(beta_rad) * Math.Sin(obliquity_rad) * Math.Sin(lambda_rad));

            return new EquatorialCoordinates
            {
                RightAscension = rightAscension,
                Declination = declination
            };
        }

        /// <summary>
        /// Calculates the subsolar point from equatorial coordinates.
        /// </summary>
        private static GeoPosition CalculateSubsolarPoint(EquatorialCoordinates equatorial, DateTime utcTime)
        {
            double rightAscensionDeg = Helper.NormalizeAngle(equatorial.RightAscension * RadiansToDegrees);
            double gmst = Helper.CalculateGMST(utcTime);

            double longitude = rightAscensionDeg - gmst;
            longitude = Helper.NormalizeLongitude(longitude);
            double latitude = equatorial.Declination * RadiansToDegrees;

            return new GeoPosition(latitude, longitude);
        }
    }
}