using COL.UnityGameWheels.Unity.Asset;
using COL.UnityGameWheels.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace COL.UnityGameWheels.Demo.Editor
{
    public class AssetBundleBuilderHandler : IAssetBundleBuilderHandler
    {
        public void OnPreBeforeBuild()
        {
            Debug.Log($"[{nameof(AssetBundleBuilderHandler)} {nameof(OnPreBeforeBuild)}]");
        }

        public void OnPostBeforeBuild(AssetBundleBuild[] assetBundleBuilds)
        {
            Debug.Log($"[{nameof(AssetBundleBuilderHandler)} {nameof(OnPostBeforeBuild)}] assetBundleBuilds.Length={assetBundleBuilds.Length}");
        }

        public void OnPreBuildPlatform(ResourcePlatform targetPlatform, int internalResourceVersion)
        {
            Debug.Log($"[{nameof(AssetBundleBuilderHandler)} {nameof(OnPreBuildPlatform)}] targetPlatform={targetPlatform}, " +
                      $"internalResourceVersion={internalResourceVersion}");
        }

        public void OnPostBuildPlatform(ResourcePlatform targetPlatform, int internalResourceVersion, string outputDirectory)
        {
            Debug.Log($"[{nameof(AssetBundleBuilderHandler)} {nameof(OnPostBuildPlatform)}] targetPlatform={targetPlatform}, " +
                      $"internalResourceVersion={internalResourceVersion}, outputDirectory={outputDirectory}");
        }

        public void OnBuildSuccess()
        {
            Debug.Log($"[{nameof(AssetBundleBuilderHandler)} {nameof(OnBuildSuccess)}]");
        }

        public void OnBuildFailure()
        {
            Debug.Log($"[{nameof(AssetBundleBuilderHandler)} {nameof(OnBuildFailure)}]");
        }
    }
}