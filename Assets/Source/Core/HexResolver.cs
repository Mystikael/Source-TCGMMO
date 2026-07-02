using H3.Extensions;
using NetTopologySuite.Geometries;

namespace SourceTCG.Core
{
    /// <summary>Client-side H3 res-9 hex resolution (matches server latLngToH3).</summary>
    public static class HexResolver
    {
        public const int Resolution = 9;

        public static string ResolveHex(double lat, double lng)
        {
            var coordinate = new Coordinate(lng, lat);
            return coordinate.ToH3Index(Resolution).ToString();
        }
    }
}