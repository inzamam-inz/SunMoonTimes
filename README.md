# SunMoonTimes

**SunMoonTimes** is a .NET library for calculating the real-time geographic positions (latitude and longitude) on earth of the Sun and Moon. It supports both current and user-specified date/time inputs (in UTC or local time).

## 🚀 Features

- Calculate the Sun’s and Moon’s position (latitude, longitude) at any `DateTime`
- Supports both UTC and local time
- Lightweight and easy to use

## 📦 Installation

Install via NuGet:

```sh
dotnet add package InTaha.SunMoonTimes
```

## 🧭 Usage

### 1. Get Live Sun and Moon position

```csharp
using SunMoonTimes;

var sun = SolarPosition.GetPosition();
var moon = LunarPosition.GetPosition();

Console.WriteLine($"Sun Position: Latitude = {sun.Latitude}, Longitude = {sun.Longitude}");
Console.WriteLine($"Moon Position: Latitude = {moon.Latitude}, Longitude = {moon.Longitude}");
```


### 2. Get position at a specific time (UTC)

```csharp
var time = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);

var tSun = SolarPosition.GetPosition(time);
var tMoon = LunarPosition.GetPosition(time);

Console.WriteLine($"Sun Position at {time}: Latitude = {tSun.Latitude}, Longitude = {tSun.Longitude}");
Console.WriteLine($"Moon Position at {time}: Latitude = {tMoon.Latitude}, Longitude = {tMoon.Longitude}");
```


### 3. Get position using local time

```csharp
var localTime = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Local);

var localSun = SolarPosition.GetPosition(localTime);
var localMoon = LunarPosition.GetPosition(localTime);

Console.WriteLine($"Local Time: {localTime}");
Console.WriteLine($"Local Sun Position: Latitude = {localSun.Latitude}, Longitude = {localSun.Longitude}");
Console.WriteLine($"Local Moon Position: Latitude = {localMoon.Latitude}, Longitude = {localMoon.Longitude}");
```

## 📘 Notes

* If using local time and don't get accurate output, ensure your system time zone is set correctly.
* Results may vary slightly depending on the internal model used for astronomical calculations.

## 📄 License

MIT License


