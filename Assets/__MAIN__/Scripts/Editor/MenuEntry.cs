namespace COL.UnityGameWheels.Demo.Editor
{
    using UnityEditor;
    using Unity.Editor;

    internal static class MenuEntry
    {
        private const string MenuPrefix = "Unity Game Wheels Demo/";

        [MenuItem(MenuPrefix + "Asset Bundle/Organizer")]
        private static void OpenAssetBundleOrganizerEditorWindow()
        {
            AssetBundleOrganizerEditorWindow.Open();
        }

        [MenuItem(MenuPrefix + "Asset Bundle/Builder")]
        private static void OpenAssetBundleBuilderEditorWindow()
        {
            AssetBundleBuilderEditorWindow.Open();
        }

        [MenuItem(MenuPrefix + "Scriptable Object Creator")]
        private static void OpenScriptableObjectCreatorEditorWindow()
        {
            ScriptableObjectCreatorEditorWindow.Open();
        }

        [MenuItem(MenuPrefix + "Project/Save &s")]
        private static void SaveProject()
        {
            Utility.Project.SaveProject();
        }

        [MenuItem(MenuPrefix + "Project/Open Data Path")]
        private static void OpenDataPath()
        {
            Utility.Project.OpenDataPath();
        }

        [MenuItem(MenuPrefix + "Project/Open Persistent Data Path")]
        private static void OpenPersistentDataPath()
        {
            Utility.Project.OpenPersistentDataPath();
        }


        [MenuItem(MenuPrefix + "Project/Open Streaming Assets Path")]
        private static void OpenStreamingAssetsPath()
        {
            Utility.Project.OpenStreamingAssetsPath();
        }

        [MenuItem(MenuPrefix + "Project/Open Temporary Cache Path")]
        private static void OpenTemporaryCachePath()
        {
            Utility.Project.OpenTemporaryCachePath();
        }
    }
}
