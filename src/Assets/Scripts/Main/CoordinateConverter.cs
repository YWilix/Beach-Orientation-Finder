using System;
using System.Globalization;

public static class CoordinateConverter
{
    private const double EarthRadiusMeters = 6378137; // Average Earth radius

    /// <summary>
    /// Converts a WGS84 coordinates into a web mercator projection coordinates
    /// </summary>
    /// <param name="latitude">the latitude WGS84 coordinate</param>
    /// <param name="longitude">the longitude WGS84 coordinate</param>
    /// <returns>a tuple containing the web mercator x and y position</returns>
    public static (double x , double y) Wgs84ToWebMercator(double latitude, double longitude)
    {
        // Convert latitude and longitude from degrees to radians
        double latRad = latitude * Math.PI / 180.0;
        double lonRad = longitude * Math.PI / 180.0;

        // Calculate Web Mercator x coordinate
        double x = EarthRadiusMeters * lonRad;

        // Calculate Web Mercator y coordinate

        double y = EarthRadiusMeters * Math.Log(Math.Tan(Math.PI / 4.0 + latRad / 2.0));

        return (x, y);
    }


    /// <summary>
    /// Tries to parse a coordinates string that should be in the form of "lat,long" 
    /// </summary>
    /// <param name="Coord"></param>
    /// <returns>a tuple containing the coordinates if the string is in the correct format else it returns null</returns>
    public static (float Lat, float Long)? TryParseCoordinates(string Coord)
    {
        if (!Coord.Contains(',')) return null;

        string[] strs = Coord.Split(',');

        if (strs.Length != 2) return null;

        string LatStr = strs[0];
        string LongStr = strs[1];

        if (float.TryParse(LatStr, out float Lat) && float.TryParse(LongStr, out float Long))
            if (-180 <= Long && Long <= 180 && -85.05112878 <= Lat && Lat <= 85.05112878)
                return (Lat, Long);

        return null;
    }
}
