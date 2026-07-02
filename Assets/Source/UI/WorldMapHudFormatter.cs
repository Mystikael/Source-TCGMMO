using System.Collections.Generic;
using System.Text;
using SourceTCG.Core;
using SourceTCG.Data;

namespace SourceTCG.UI
{
    public struct MapPin
    {
        public string SpawnType;
        public string SubtypeId;
        public double Lat;
        public double Lng;
        public float DistanceM;
    }

    public static class WorldMapHudFormatter
    {
        public static string ResolveHex(double lat, double lng) => HexResolver.ResolveHex(lat, lng);

        public static string FormatNearbyPins(IReadOnlyList<MapPin> pins, double playerLat, double playerLng)
        {
            if (pins == null || pins.Count == 0) return "Nearby Ki/ReSource pins: (none)";

            var sb = new StringBuilder();
            sb.AppendLine("Nearby Ki/ReSource pins:");
            foreach (var p in pins)
            {
                var inRange = Haversine.IsWithinRadiusMeters(
                    playerLat, playerLng, p.Lat, p.Lng, EconomyConfig.CollectionRadiusM);
                var label = p.SpawnType == "ki" ? "Ki" : "Re";
                var range = inRange ? "PIN IN RANGE" : $"{p.DistanceM:F0}m";
                sb.AppendLine($"  [{label}] {p.SubtypeId} {range}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string FormatKiDwellProgress(
            int elapsedSeconds, int requiredSeconds, string state, string affinityId)
        {
            var name = GameCatalog.GetAffinityName(affinityId);
            var pct = requiredSeconds > 0
                ? (int)(100.0 * elapsedSeconds / requiredSeconds)
                : 0;
            return $"Ki dwell: {name} {elapsedSeconds}/{requiredSeconds}s ({pct}%) — {state}";
        }
    }
}