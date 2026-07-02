using NUnit.Framework;
using SourceTCG.Core;

namespace SourceTCG.Tests.EditMode
{
    public class HexResolverEditModeTests
    {
        [Test]
        public void ResolveHex_LoadsH3Plugin_AndReturnsStableSfIndex()
        {
            var h3 = HexResolver.ResolveHex(37.7749, -122.4194);
            Assert.AreEqual("89283082803ffff", h3);
        }
    }
}