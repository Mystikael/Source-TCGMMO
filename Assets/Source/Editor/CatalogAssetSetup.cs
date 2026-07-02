#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using SourceTCG.Data;
using UnityEditor;
using UnityEngine;

namespace SourceTCG.Editor
{
    public static class CatalogAssetSetup
    {
        const string AffinityDir = "Assets/Source/Data/Catalog/Affinities";
        const string ResourceDir = "Assets/Source/Data/Catalog/Resources";

        [MenuItem("Source TCG/Bake Catalog Assets")]
        public static void BakeMenu() => BakeCatalogAssets();

        /// <summary>Callable from Unity -batchmode -executeMethod SourceTCG.Editor.CatalogAssetSetup.BakeCatalogAssets</summary>
        public static void BakeCatalogAssets()
        {
            Directory.CreateDirectory(AffinityDir);
            Directory.CreateDirectory(ResourceDir);

            foreach (var def in GameCatalog.Affinities)
                BakeAffinity(def);

            foreach (var def in GameCatalog.Resources)
                BakeResource(def);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Catalog assets baked: {GameCatalog.AffinityCount} affinities, {GameCatalog.ResourceCount} resources.");
        }

        [MenuItem("Source TCG/Verify Catalog Assets")]
        public static void VerifyMenu() => VerifyCatalogAssets();

        /// <summary>Callable from Unity -batchmode -executeMethod SourceTCG.Editor.CatalogAssetSetup.VerifyCatalogAssets</summary>
        public static void VerifyCatalogAssets()
        {
            try
            {
                AssertCatalogCounts();
                foreach (var def in GameCatalog.Affinities)
                    AssertAffinityAsset(def);
                foreach (var def in GameCatalog.Resources)
                    AssertResourceAsset(def);
                Debug.Log("VerifyCatalogAssets PASS");
            }
            catch (Exception ex)
            {
                Debug.LogError($"VerifyCatalogAssets FAIL: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }

        static void BakeAffinity(KiAffinityDef def)
        {
            var path = $"{AffinityDir}/{def.Name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<KiAffinityAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<KiAffinityAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var iconPath = $"Assets/ki_affinities/{def.IconFile}";
            EnsureSpriteImport(iconPath);
            asset.affinityId = def.Id;
            asset.displayName = def.Name;
            asset.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            EditorUtility.SetDirty(asset);
        }

        static void BakeResource(ResourceDef def)
        {
            var path = $"{ResourceDir}/{def.Id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ResourceAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ResourceAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.resourceId = def.Id;
            asset.tier = def.Tier;
            asset.category = def.Category;
            asset.displayName = def.Name;
            EditorUtility.SetDirty(asset);
        }

        static void EnsureSpriteImport(string iconPath)
        {
            var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Missing ki icon texture: {iconPath}");
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        static void AssertCatalogCounts()
        {
            var affGuids = AssetDatabase.FindAssets("t:KiAffinityAsset", new[] { AffinityDir });
            var resGuids = AssetDatabase.FindAssets("t:ResourceAsset", new[] { ResourceDir });
            if (affGuids.Length != GameCatalog.AffinityCount)
                throw new InvalidOperationException($"Expected {GameCatalog.AffinityCount} KiAffinityAsset files, found {affGuids.Length}");
            if (resGuids.Length != GameCatalog.ResourceCount)
                throw new InvalidOperationException($"Expected {GameCatalog.ResourceCount} ResourceAsset files, found {resGuids.Length}");
        }

        static void AssertAffinityAsset(KiAffinityDef def)
        {
            var path = $"{AffinityDir}/{def.Name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<KiAffinityAsset>(path);
            if (asset == null)
                throw new InvalidOperationException($"Missing affinity asset: {path}");
            if (asset.affinityId != def.Id)
                throw new InvalidOperationException($"{path} affinityId mismatch");
            if (asset.displayName != def.Name)
                throw new InvalidOperationException($"{path} displayName mismatch");
            if (asset.icon == null)
                throw new InvalidOperationException($"{path} icon sprite not assigned");

            var iconPath = $"Assets/ki_affinities/{def.IconFile}";
            var expectedGuid = AssetDatabase.AssetPathToGUID(iconPath);
            var iconGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset.icon));
            if (string.IsNullOrEmpty(expectedGuid) || iconGuid != expectedGuid)
                throw new InvalidOperationException($"{path} icon does not reference {iconPath}");
        }

        static void AssertResourceAsset(ResourceDef def)
        {
            var path = $"{ResourceDir}/{def.Id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ResourceAsset>(path);
            if (asset == null)
                throw new InvalidOperationException($"Missing resource asset: {path}");
            if (asset.resourceId != def.Id || asset.tier != def.Tier || asset.category != def.Category || asset.displayName != def.Name)
                throw new InvalidOperationException($"{path} resource fields mismatch");
        }
    }
}
#endif