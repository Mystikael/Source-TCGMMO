using System.Linq;
using NUnit.Framework;
using SourceTCG.Data;
using UnityEditor;

namespace SourceTCG.Tests.EditMode
{
    public class CatalogAssetEditModeTests
    {
        const string AffinityDir = "Assets/Source/Data/Catalog/Affinities";
        const string ResourceDir = "Assets/Source/Data/Catalog/Resources";

        [Test]
        public void CatalogAssets_Has12AffinitiesWithKiPngIcons()
        {
            var guids = AssetDatabase.FindAssets("t:KiAffinityAsset", new[] { AffinityDir });
            Assert.AreEqual(GameCatalog.AffinityCount, guids.Length);

            foreach (var def in GameCatalog.Affinities)
            {
                var path = $"{AffinityDir}/{def.Name}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<KiAffinityAsset>(path);
                Assert.IsNotNull(asset, $"Missing {path}");
                Assert.AreEqual(def.Id, asset.affinityId);
                Assert.IsNotNull(asset.icon, $"{path} missing icon sprite");

                var iconPath = $"Assets/ki_affinities/{def.IconFile}";
                var expectedGuid = AssetDatabase.AssetPathToGUID(iconPath);
                var actualGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset.icon));
                Assert.AreEqual(expectedGuid, actualGuid, $"{path} icon must reference {iconPath}");
            }
        }

        [Test]
        public void CatalogAssets_Has27ResourcesCoveringTiersAndCategories()
        {
            var guids = AssetDatabase.FindAssets("t:ResourceAsset", new[] { ResourceDir });
            Assert.AreEqual(GameCatalog.ResourceCount, guids.Length);

            for (var tier = 0; tier <= 8; tier++)
            {
                foreach (var category in new[] { "terra", "fauna", "flora" })
                {
                    var match = guids
                        .Select(g => AssetDatabase.LoadAssetAtPath<ResourceAsset>(AssetDatabase.GUIDToAssetPath(g)))
                        .FirstOrDefault(a => a != null && a.tier == tier && a.category == category);
                    Assert.IsNotNull(match, $"Missing resource asset tier={tier} category={category}");
                }
            }
        }
    }
}