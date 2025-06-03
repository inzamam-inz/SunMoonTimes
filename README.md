# SunMoonTimes

**SunMoonTimes** is a .NET library for calculating the real-time geographic positions (latitude and longitude) of the Sun and Moon on Earth. It allows you to determine sunrise, sunset, moonrise, and moonset times for any observer location and date—whether past, present, or future. Additionally, it computes the azimuth and elevation angles of the Sun and Moon as seen from the observer’s position.

The library supports both current and user-specified date/time inputs, in either UTC or local time.

## 📦 Installation

Install via NuGet:

```sh
dotnet add package InTaha.SunMoonTimes
```

## 🧭 Usage

### 1. Get Live Sun and Moon position

```csharp
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

```

### 2. Using any specific time (UTC)

```csharp
// You can use any valid date time for all of the method, for example
var time = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);

var tSun = SolarPosition.GetPosition(time);
var tMoon = LunarPosition.GetPosition(time);

Console.WriteLine($"Sun Position at {time}: Latitude = {tSun.Latitude}, Longitude = {tSun.Longitude}");
Console.WriteLine($"Moon Position at {time}: Latitude = {tMoon.Latitude}, Longitude = {tMoon.Longitude}");
```

### 3. Using any specific time (local)

```csharp
// You can use any valid local date time for all of the method, for example
var localTime = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Local);

var localSun = SolarPosition.GetPosition(localTime);
var localMoon = LunarPosition.GetPosition(localTime);

Console.WriteLine($"Local Time: {localTime}");
Console.WriteLine($"Local Sun Position: Latitude = {localSun.Latitude}, Longitude = {localSun.Longitude}");
Console.WriteLine($"Local Moon Position: Latitude = {localMoon.Latitude}, Longitude = {localMoon.Longitude}");
```

## 📘 Notes

- If using local time and don't get accurate output, ensure your system time zone is set correctly.
- Results may vary slightly depending on the internal model used for astronomical calculations.

## 📄 License

MIT License
