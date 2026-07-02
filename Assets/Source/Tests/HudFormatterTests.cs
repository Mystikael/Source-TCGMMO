#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using SourceTCG.UI;

namespace SourceTCG.Tests
{
    public class HudFormatterTests
    {
        [Test]
        public void FormatNearbyPins_ShowsInRangePin()
        {
            var pins = new List<MapPin>
            {
                new MapPin
                {
                    SpawnType = "ki",
                    SubtypeId = "watyr",
                    Lat = 37.7749,
                    Lng = -122.4194,
                    DistanceM = 5f,
                },
            };
            var text = WorldMapHudFormatter.FormatNearbyPins(pins, 37.7749, -122.4194);
            Assert.That(text, Does.Contain("PIN IN RANGE"));
            Assert.That(text, Does.Contain("[Ki] watyr"));
        }

        [Test]
        public void FormatNearbyPins_ShowsDistanceWhenOutOfRange()
        {
            var pins = new List<MapPin>
            {
                new MapPin
                {
                    SpawnType = "resource",
                    SubtypeId = "terra_t0",
                    Lat = 37.78,
                    Lng = -122.42,
                    DistanceM = 120f,
                },
            };
            var text = WorldMapHudFormatter.FormatNearbyPins(pins, 37.7749, -122.4194);
            Assert.That(text, Does.Contain("[Re] terra_t0"));
            Assert.That(text, Does.Contain("120m"));
        }

        [Test]
        public void FormatKiDwellProgress_Active()
        {
            var line = WorldMapHudFormatter.FormatKiDwellProgress(30, 120, "active", "fyr");
            Assert.That(line, Does.Contain("Fyr"));
            Assert.That(line, Does.Contain("30/120s"));
            Assert.That(line, Does.Contain("active"));
        }

        [Test]
        public void FormatKiDwellProgress_Paused()
        {
            var line = WorldMapHudFormatter.FormatKiDwellProgress(10, 100, "paused", "watyr");
            Assert.That(line, Does.Contain("Watyr"));
            Assert.That(line, Does.Contain("10/100s"));
            Assert.That(line, Does.Contain("paused"));
        }

        [Test]
        public void FormatKiDwellProgress_Completed()
        {
            var line = WorldMapHudFormatter.FormatKiDwellProgress(60, 60, "completed", "aether");
            Assert.That(line, Does.Contain("100%"));
            Assert.That(line, Does.Contain("completed"));
        }
    }
}
#endif