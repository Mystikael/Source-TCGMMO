#if UNITY_INCLUDE_TESTS
using System.Linq;
using NUnit.Framework;
using SourceTCG.Core;
using SourceTCG.Data;

namespace SourceTCG.Tests
{
    public class EconomyTests
    {
        [Test]
        public void EconomyConfig_MatchesAlphaSpec()
        {
            Assert.AreEqual(3, EconomyConfig.DailyExtractLimit);
            Assert.AreEqual(100, EconomyConfig.KiStartCost);
            Assert.AreEqual(250, EconomyConfig.ResourceGatherCost);
            Assert.AreEqual(40f, EconomyConfig.CollectionRadiusM);
        }

        [Test]
        public void Haversine_SamePoint_IsNearZero()
        {
            var d = Haversine.DistanceMeters(37.77, -122.42, 37.77, -122.42);
            Assert.Less(d, 1.0);
        }

        [Test]
        public void Haversine_Within40Meters()
        {
            var lat = 37.7749;
            var lng = -122.4194;
            var nearLat = lat + 0.00027;
            Assert.IsTrue(Haversine.IsWithinRadiusMeters(lat, lng, nearLat, lng, 40f));
            Assert.IsFalse(Haversine.IsWithinRadiusMeters(lat, lng, lat + 0.01, lng, 40f));
        }

        [Test]
        public void GameCatalog_Has12Affinities()
        {
            Assert.AreEqual(GameCatalog.AffinityCount, GameCatalog.Affinities.Count);
        }

        [Test]
        public void GameCatalog_Has27Resources()
        {
            Assert.AreEqual(GameCatalog.ResourceCount, GameCatalog.Resources.Count);
        }

        [Test]
        public void GetAffinityName_ReturnsCorrectName()
        {
            Assert.AreEqual("Watyr", GameCatalog.GetAffinityName("watyr"));
            Assert.AreEqual("watyr", GameCatalog.GetAffinityName("watyr"));
        }

        [Test]
        public void GameCatalog_CoversAllTiersAndCategories()
        {
            for (var tier = 0; tier <= 8; tier++)
            {
                Assert.IsTrue(GameCatalog.Resources.Any(r => r.Tier == tier && r.Category == "terra"));
                Assert.IsTrue(GameCatalog.Resources.Any(r => r.Tier == tier && r.Category == "fauna"));
                Assert.IsTrue(GameCatalog.Resources.Any(r => r.Tier == tier && r.Category == "flora"));
            }
        }
    }
}
#endif