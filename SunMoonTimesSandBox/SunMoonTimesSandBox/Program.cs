using SunMoonTimes;

// Sun and Moon position calculation example for current UTC time
var sun = SolarPosition.GetPosition();
var moon = LunarPosition.GetPosition();
Console.WriteLine($"Sun Position: Latitude = {sun.Latitude}, Longitude = {sun.Longitude}");
Console.WriteLine($"Moon Position: Latitude = {moon.Latitude}, Longitude = {moon.Longitude}");


// Sun and Moon position calculation example for a specific date and time
var time = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
var tSun = SolarPosition.GetPosition(time);
var tMoon = LunarPosition.GetPosition(time);
Console.WriteLine($"Sun Position at {time}: Latitude = {tSun.Latitude}, Longitude = {tSun.Longitude}");
Console.WriteLine($"Moon Position at {time}: Latitude = {tMoon.Latitude}, Longitude = {tMoon.Longitude}");