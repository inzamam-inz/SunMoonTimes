namespace SunMoonTimes.Models
{
    public readonly struct GeoPosition
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeoPosition(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
