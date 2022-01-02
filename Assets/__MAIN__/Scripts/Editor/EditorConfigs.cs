using System;
using COL.UnityGameWheels.Editor;
using COL.UnityGameWheels.Unity.Editor;

namespace COL.UnityGameWheels.Demo.Editor
{
    public static class EditorConfigs
    {
        [AssetBundleOrganizerConfigPath]
        public const string AssetBundleOrganizerConfigPath = "Assets/__MAIN__/Editor/AssetBundleOrganizerConfig.xml";

        [AssetBundleBuilderConfigPath]
        public const string AssetBundleBuilderConfigPathAttribute = "Assets/__MAIN__/Editor/AssetBundleBuilderConfig.xml";

        [AssetBundleBuilderHandlerConfig]
        public static readonly Type AssetBundleBuilderHandlerType = typeof(AssetBundleBuilderHandler);

        [AssetBundleOrganizerIgnoreAssetLabel]
        public const string AssetBundleOrganizerIgnoreAssetLabel = "AssetBundleIgnored";
    }
}