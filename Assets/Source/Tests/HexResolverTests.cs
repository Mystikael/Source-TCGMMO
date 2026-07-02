#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using SourceTCG.Core;

namespace SourceTCG.Tests
{
    public class HexResolverTests
    {
        [Test]
        public void ResolveHex_SF_ReturnsStableH3Res9()
        {
            var h3 = HexResolver.ResolveHex(37.7749, -122.4194);
            Assert.AreEqual("89283082803ffff", h3);
            Assert.AreEqual(h3, HexResolver.ResolveHex(37.7749, -122.4194));
        }

        [Test]
        public void ResolveHex_DistantCoords_Differ()
        {
            var sf = HexResolver.ResolveHex(37.7749, -122.4194);
            var nyc = HexResolver.ResolveHex(40.7128, -74.006);
            Assert.AreNotEqual(sf, nyc);
        }
    }
}
#endif