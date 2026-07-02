using System;

namespace SourceTCG.Core
{
    public static class Haversine
    {
        const double EarthRadiusM = 6371000d;

        public static double DistanceMeters(double lat1, double lng1, double lat2, double lng2)
        {
            double ToRad(double d) => d * Math.PI / 180d;
            var dLat = ToRad(lat2 - lat1);
            var dLng = ToRad(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return 2 * EarthRadiusM * Math.Asin(Math.Sqrt(a));
        }

        public static bool IsWithinRadiusMeters(double lat1, double lng1, double lat2, double lng2, float radiusM)
        {
            return DistanceMeters(lat1, lng1, lat2, lng2) <= radiusM;
        }
    }
}