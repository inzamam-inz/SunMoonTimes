//using SunMoonTimesSandBox.Models;
//using SunMoonTimesSandBox;
using SunMoonTimes;
using SunMoonTimes.Models;

Console.WriteLine("Solar Position Calculator Demo\n");

var kagoshima = new GeoPosition(31.35, 130.33);
var tokyo = new GeoPosition(35.68, 139.76);
var london = new GeoPosition(51.51, -0.13);
var dhaka = new GeoPosition(23.81, 90.41);

var observer = kagoshima; 
Console.WriteLine($"Observer: {observer.Latitude:F4}°, {observer.Longitude:F4}°");

// 1. Get current sun subsolar point
var sunPosition = SolarPosition.GetPosition();
Console.WriteLine("1. Current Sun Subsolar Point:");
Console.WriteLine($"   Sun is directly overhead at: {sunPosition.Latitude:F2}°, {sunPosition.Longitude:F2}°\n");

// 2. Get sun's azimuth and elevation from observer
var (azimuth, elevation) = SolarPosition.GetAzimuthElevation(observer);
Console.WriteLine("2. Sun's Position from Observer:");
Console.WriteLine($"   Azimuth: {azimuth:F1}° (from North)");
Console.WriteLine($"   Elevation: {elevation:F1}° (above horizon)\n");

// 3. Get today's sunrise and sunset
var (sunrise, sunset) = SolarPosition.GetRiseSet(observer);
Console.WriteLine("3. Today's Sunrise & Sunset:");
Console.WriteLine(sunrise != null
    ? $"   Sunrise: {sunrise.Value:HH:mm:ss}"
    : "   No sunrise today");
Console.WriteLine(sunset != null
    ? $"   Sunset: {sunset.Value:HH:mm:ss}"
    : "   No sunset today");


Console.WriteLine("\n\nLunar Position Calculator Demo\n");

// 1. Get current moon subsolar point
var moonPosition = LunarPosition.GetPosition();

Console.WriteLine("1. Current Moon Subsolar Point:");
Console.WriteLine($"   Moon is directly overhead at: {moonPosition.Latitude:F2}°, {moonPosition.Longitude:F2}°\n");


// 2. Get moon's azimuth and elevation from observer
var (moonAzimuth, moonElevation) = LunarPosition.GetAzimuthElevation(observer);
Console.WriteLine("2. Moon's Position from Observer:");
Console.WriteLine($"   Azimuth: {moonAzimuth:F1}° (from North)");
Console.WriteLine($"   Elevation: {moonElevation:F1}° (above horizon)\n");

// 3. Get the next moonrise and moonset times occurring after now (within the next 24 hours)
var (moonrise, moonset) = LunarPosition.GetNextRiseSet(observer);
Console.WriteLine("3. Today's Moonrise & Moonset:");
Console.WriteLine(moonrise != null
    ? $"   Moonrise: {moonrise.Value:HH:mm:ss}"
    : "   No moonrise today");
Console.WriteLine(moonset != null
    ? $"   Moonset: {moonset.Value:HH:mm:ss}"
    : "   No moonset today");


