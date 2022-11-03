using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Unity.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COL.UnityGameWheels.Demo.Editor
{
    public static class AssetsGenerator
    {
        private const string NumberTexturePath = "Assets/__MAIN__/NumberTextures";
        private const string GeneratedAssetPath = "Assets/__MAIN__/GeneratedAssets";
        private const string AssetBundleRoot = "gen";
        private const int ImageCountToGen = 1024;
        private const int AssetCountPerGroup = 8;
        private const int DependencyLayers = 4;
        private const int DependencyOverlap = 2;
        private const int NumberTextureHeight = 128;
        private const int NumberTextureWidth = 64;

        public static void Run()
        {
            ClearGeneratedAssets();
            GenerateTextures();
            GeneratePrefabs();

            var assetBundleOrganizer = new AssetBundleOrganizer();
            assetBundleOrganizer.RefreshAssetForest();
            assetBundleOrganizer.RefreshAssetBundleTree();
            assetBundleOrganizer.CleanUpInvalidAssets();
            ClearAssetBundleInfos(assetBundleOrganizer);
            GenerateAssetBundleInfos(assetBundleOrganizer);
        }

        private static void GenerateAssetBundleInfos(AssetBundleOrganizer organizer)
        {
            var genAssetRoot = organizer.AssetInfoForestRoots.SingleOrDefault(root => root.Path == GeneratedAssetPath);

            foreach (var kv in genAssetRoot.Children)
            {
                var assetInfo = kv.Value;
                if (!assetInfo.IsFile || !string.IsNullOrEmpty(assetInfo.AssetBundlePath))
                {
                    continue;
                }

                var abPath = Path.Combine(AssetBundleRoot, Path.GetFileNameWithoutExtension(assetInfo.Name) ?? throw new NullReferenceException());
                organizer.CreateNewAssetBundle(abPath, Constant.CommonResourceGroupId, false);
                organizer.AssignAssetsToBundle(new[] {assetInfo}, abPath);
            }

            organizer.SaveConfig();
        }

        private static void ClearAssetBundleInfos(AssetBundleOrganizer organizer)
        {
            var root = organizer.AssetBundleInfoTreeRoot;
            if (!root.Children.ContainsKey(AssetBundleRoot))
            {
                return;
            }

            var genRoot = root.Children[AssetBundleRoot];
            var assetBundlesToDelete = new List<string>();
            foreach (var kv in genRoot.Children)
            {
                var assetBundleInfo = kv.Value;
                if (!assetBundleInfo.IsDirectory)
                {
                    assetBundlesToDelete.Add(assetBundleInfo.Path);
                }
            }

            foreach (var assetBundlePath in assetBundlesToDelete)
            {
                organizer.DeleteAssetBundle(assetBundlePath);
            }

            organizer.SaveConfig();
        }

        private static void GeneratePrefabs()
        {
            var lastLayerAssetCount = ImageCountToGen;
            for (int layer = DependencyLayers - 1; layer >= 0; layer--)
            {
                int assetCount = 0;
                DependencyTest current = new GameObject().AddComponent<DependencyTest>();
                current.InitDependencies(AssetCountPerGroup);
                int currentDependencyCount = 0;
                for (int i = 1; i <= lastLayerAssetCount; i++)
                {
                    string nextAssetName;
                    if (layer == DependencyLayers - 1)
                    {
                        nextAssetName = i + ".png";
                    }
                    else
                    {
                        nextAssetName = (layer + 1) + "_" + i + ".prefab";
                    }

                    current.Dependencies[currentDependencyCount++] = AssetDatabase.LoadAssetAtPath<Object>(Path.Combine(GeneratedAssetPath, nextAssetName));

                    if (currentDependencyCount != AssetCountPerGroup)
                    {
                        continue;
                    }

                    PrefabUtility.SaveAsPrefabAsset(current.gameObject, Path.Combine(GeneratedAssetPath, layer + "_" + ++assetCount + ".prefab"));
                    Object.DestroyImmediate(current.gameObject);
                    i -= DependencyOverlap;

                    if (i < lastLayerAssetCount)
                    {
                        current = new GameObject().AddComponent<DependencyTest>();
                        current.InitDependencies(AssetCountPerGroup);
                        currentDependencyCount = 0;
                    }
                    else
                    {
                        current = null;
                    }
                }

                if (current != null)
                {
                    PrefabUtility.SaveAsPrefabAsset(current.gameObject, Path.Combine(GeneratedAssetPath, layer + "_" + ++assetCount + ".prefab"));
                    Object.DestroyImmediate(current.gameObject);
                }

                lastLayerAssetCount = assetCount;
            }
        }

        private static void GenerateTextures()
        {
            var numberTextures = new Texture2D[10];
            for (int i = 0; i < numberTextures.Length; i++)
            {
                numberTextures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(NumberTexturePath, i + ".png"));
            }

            for (int i = 0; i < ImageCountToGen; i++)
            {
                GenerateTexture(i + 1, numberTextures);
            }

            AssetDatabase.Refresh();
        }

        private static void GenerateTexture(int number, Texture2D[] numberTextures)
        {
            List<int> digits = new List<int>();
            var tmpNumber = number;
            while (tmpNumber > 0)
            {
                digits.Add(tmpNumber % 10);
                tmpNumber = tmpNumber / 10;
            }

            var texture = new Texture2D(digits.Count * NumberTextureWidth, NumberTextureHeight, TextureFormat.RGBA32, false);
            for (int i = digits.Count - 1; i >= 0; i--)
            {
                var numberTexture = numberTextures[digits[i]];
                var colors = numberTexture.GetPixels(0, 0, NumberTextureWidth, NumberTextureHeight);
                texture.SetPixels((digits.Count - 1 - i) * NumberTextureWidth, 0, NumberTextureWidth, NumberTextureHeight,
                    colors);
                texture.Apply();
            }

            Directory.CreateDirectory(GeneratedAssetPath);
            File.WriteAllBytes(Path.Combine(GeneratedAssetPath, number + ".png"), texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        private static void ClearGeneratedAssets()
        {
            var assetGuids = AssetDatabase.FindAssets(string.Empty, new string[] {GeneratedAssetPath});
            foreach (var assetGuid in assetGuids)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assetGuid));
            }
        }
    }
}