using System.Collections.Generic;
using System.Linq;
using COL.UnityGameWheels.Core;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace COL.UnityGameWheels.Demo
{
    using Core.Asset;
    using System;
    using System.Collections;
    using Unity;
    using Unity.Asset;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [DisallowMultipleComponent]
    public class DemoAssetApp : MonoBehaviourEx
    {
        private static DemoAssetApp s_Instance = null;

        [SerializeField]
        private AssetManager m_AssetManager = null;

        [SerializeField]
        private IRefPoolService m_RefPoolService = null;

        [SerializeField]
        private IDownloadService m_DownloadService = null;

        [SerializeField]
        private RemoteIndexFileInfo m_RemoteIndexFileInfo = null;

        [SerializeField]
        private int m_UpdateCheckerRetryTimes = 2;

        [SerializeField]
        private string m_CheckGroupAssetPath = null;

        [SerializeField]
        private string m_DepOnSpritePrefabPath = null;

        [SerializeField]
        private string m_PrefabADependsOnBPath = null;

        [SerializeField]
        private string m_GetPipTextPath = null;

        [SerializeField]
        private string m_SceneAssetPath = null;

        private int[] m_AvailableGroupIds = null;
        private readonly HashSet<int> m_GroupIdsToUpdate = new HashSet<int>();

        public static bool IsAvailable
        {
            get { return s_Instance != null; }
        }

        public static IAssetManager Asset
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_AssetManager == null)
                {
                    throw new NullReferenceException("Asset manager is invalid.");
                }

                return s_Instance.m_AssetManager;
            }
        }

        public static IRefPoolService RefPool
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_RefPoolService == null)
                {
                    throw new NullReferenceException("Reference pool manager is invalid.");
                }

                return s_Instance.m_RefPoolService;
            }
        }

        public static IDownloadService Download
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_DownloadService == null)
                {
                    throw new NullReferenceException("Download manager is invalid.");
                }

                return s_Instance.m_DownloadService;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            s_Instance = this;

            Log.SetLogger(new LoggerImpl());
        }

        protected override void OnDestroy()
        {
            s_Instance = null;
            base.OnDestroy();
        }

        private void Start()
        {
            Download.RefPoolService = RefPool;
            Asset.DownloadModule = Download;
            Asset.RefPoolModule = RefPool;

            RefPool.OnInit();
            Download.OnInit();
            Asset.Init();

            Asset.Prepare(new AssetManagerPrepareCallbackSet
            {
                OnSuccess = OnAssetManagerPrepareSuccess,
                OnFailure = OnAssetManagerPrepareFailure,
            }, "Fake context for preparation");
        }

        private static void CheckInstanceOrThrow()
        {
            if (s_Instance == null)
            {
                throw new NullReferenceException("App instance is invalid.");
            }
        }

        private void OnAssetManagerPrepareFailure(string errorMessage, object context)
        {
            Debug.LogWarningFormat("[DemoAssetApp OnAssetManagerPrepareFailure] errorMessage='{0}', context='{1}'.",
                errorMessage, context);
        }

        private void OnAssetManagerPrepareSuccess(object context)
        {
            Debug.LogFormat("[DemoAssetApp OnAssetManagerPrepareSuccess] context='{0}'.", context);

            Asset.CheckUpdate((AssetIndexRemoteFileInfo)m_RemoteIndexFileInfo, new UpdateCheckCallbackSet
            {
                OnFailure = OnUpdateCheckFailure,
                OnSuccess = OnUpdateCheckSuccess,
            }, "Fake context for update checking");
        }

        private void OnUpdateCheckFailure(string errorMessage, object context)
        {
            if (m_UpdateCheckerRetryTimes-- <= 0)
            {
                Debug.LogErrorFormat("[DemoAssetApp OnUpdateCheckFailure] errorMessage='{0}', context='{1}'.",
                    errorMessage, context);
                return;
            }

            Debug.LogWarningFormat("[DemoAssetApp OnUpdateCheckFailure] errorMessage='{0}', context='{1}'.",
                errorMessage, context);
            Asset.CheckUpdate((AssetIndexRemoteFileInfo)m_RemoteIndexFileInfo, new UpdateCheckCallbackSet
            {
                OnFailure = OnUpdateCheckFailure,
                OnSuccess = OnUpdateCheckSuccess,
            }, "Fake context for update checking");
        }

        private void OnUpdateCheckSuccess(object context)
        {
            Debug.LogFormat("[DemoAssetApp OnUpdateCheckSuccess] context='{0}'.", context);
            Debug.LogFormat("[DemoAssetApp OnUpdateCheckSuccess] Group ID of asset '{0}' is {1}.",
                m_CheckGroupAssetPath,
                Asset.GetAssetResourceGroupId(m_CheckGroupAssetPath));
            UpdateCommonGroup();
        }

        private void UpdateCommonGroup()
        {
            if (Asset.ResourceUpdater.GetResourceGroupStatus(Constant.CommonResourceGroupId) == ResourceGroupStatus.UpToDate)
            {
                ContinueUpdateResourceGroupsOrUseAssets();
                return;
            }

            Asset.ResourceUpdater.StartUpdatingResourceGroup(Constant.CommonResourceGroupId, new ResourceGroupUpdateCallbackSet
            {
                OnAllFailure = OnUpdateAllResourcesFailure,
                OnAllSuccess = OnUpdateAllResourcesSuccess,
                OnSingleFailure = OnUpdateResourceFailure,
                OnSingleSuccess = OnUpdateResourceSuccess,
                OnSingleProgress = OnUpdateResourceProgress,
            }, 0);
        }

        private void ContinueUpdateResourceGroupsOrUseAssets()
        {
            m_AvailableGroupIds = Asset.ResourceUpdater.GetAvailableResourceGroupIds();

            foreach (var groupId in m_AvailableGroupIds.Where(id => id != 0))
            {
                if (Asset.ResourceUpdater.GetResourceGroupStatus(groupId) == ResourceGroupStatus.OutOfDate)
                {
                    m_GroupIdsToUpdate.Add(groupId);
                }
            }

            if (m_GroupIdsToUpdate.Count == 0)
            {
                LoadAtlas();
                return;
            }

            foreach (var groupId in m_GroupIdsToUpdate)
            {
                var sb = Core.StringBuilderCache.Acquire();
                sb.AppendFormat("[DemoAssetApp ContinueUpdateResourceGroupsOrUseAssets] " +
                                "Resources to update for group '{0}':\n", groupId);

                Asset.ResourceUpdater.StartUpdatingResourceGroup(groupId, new ResourceGroupUpdateCallbackSet
                {
                    OnAllFailure = OnUpdateAllResourcesFailure,
                    OnAllSuccess = OnUpdateAllResourcesSuccess,
                    OnSingleFailure = OnUpdateResourceFailure,
                    OnSingleSuccess = OnUpdateResourceSuccess,
                    OnSingleProgress = OnUpdateResourceProgress,
                }, groupId);
            }
        }

        private IEnumerator StopAndResume(int groupId)
        {
            yield return null;
            Debug.LogWarning("Stop group " + groupId);
            Asset.ResourceUpdater.StopUpdatingResourceGroup(groupId);
            yield return null;
            Debug.LogWarning("Resume group " + groupId);
            Asset.ResourceUpdater.StartUpdatingResourceGroup(groupId, new ResourceGroupUpdateCallbackSet
            {
                OnSingleSuccess = OnUpdateResourceSuccess,
                OnSingleFailure = OnUpdateResourceFailure,
                OnSingleProgress = OnUpdateResourceProgress,
                OnAllSuccess = OnUpdateAllResourcesSuccess,
                OnAllFailure = OnUpdateAllResourcesFailure,
            }, groupId);
            yield break;
        }

        private void OnUpdateResourceSuccess(string resourcePath, long totalSize, object context)
        {
            Debug.LogFormat(
                "[DemoAssetApp OnSingleResourceUpdateSuccess] resourcePath='{0}', totalSize='{1}', context='{2}'.",
                resourcePath, totalSize, context);
        }

        private void OnUpdateResourceFailure(string resourcePath, string errorMessage, object context)
        {
            Debug.LogWarningFormat(
                "[DemoAssetApp OnSingleResourceUpdateFailure] resourcePath='{0}', errorMessage='{1}', context='{2}'.",
                resourcePath, errorMessage, context);
        }

        private void OnUpdateResourceProgress(string resourcePath, long updatedSize, long totalSize, object context)
        {
            Debug.LogFormat(
                "[DemoAssetApp OnSingleResourceUpdateProgress] resourcePath='{0}', updatedSize='{1}', totalSize='{2}', context='{3}'.",
                resourcePath, updatedSize, totalSize, context);
        }

        private void OnUpdateAllResourcesSuccess(object context)
        {
            Debug.LogFormat("[DemoAssetApp OnAllResourcesUpdateSuccess] context='{0}'", context);
            var groupId = (int)context;
            if (groupId == Core.Asset.Constant.CommonResourceGroupId)
            {
                ContinueUpdateResourceGroupsOrUseAssets();
            }

            m_GroupIdsToUpdate.Remove(groupId);
            if (m_GroupIdsToUpdate.Count > 0)
            {
                return;
            }

            LoadAtlas();
        }

        private void OnUpdateAllResourcesFailure(string errorMessage, object context)
        {
            Debug.LogWarningFormat("[DemoAssetApp OnAllResourcesUpdateFailure] errorMessage='{0}', context='{1}'",
                errorMessage, context);
        }

        private void OnLoadAssetFailure(IAssetAccessor assetAccessor, string errorMessage, object context)
        {
            Debug.LogWarningFormat(
                "[DemoAssetApp OnLoadAssetFailure] assetPath='{0}', errorMessage='{1}', context='{2}'",
                assetAccessor.AssetPath, errorMessage, context);
            Asset.UnloadAsset(assetAccessor);
        }

        private void OnLoadAssetSuccess(IAssetAccessor assetAccessor, object context)
        {
            Debug.LogFormat("[DemoAssetApp OnLoadAssetSuccess] assetPath='{0}', assetObject='{1}', context='{2}'",
                assetAccessor.AssetPath, assetAccessor.AssetObject, context);
            var go = Instantiate((GameObject)assetAccessor.AssetObject);
            StartCoroutine(UnloadFirstAssetCo(assetAccessor, go));
        }

        private IEnumerator UnloadFirstAssetCo(IAssetAccessor assetAccessor, GameObject go)
        {
            yield return new WaitForSeconds(3);
            Destroy(go);
            Asset.UnloadAsset(assetAccessor);
            yield return new WaitForSeconds(1);
            LoadAtlas();
        }

        private void LoadAtlas()
        {
            Asset.LoadAsset(m_DepOnSpritePrefabPath, new LoadAssetCallbackSet
            {
                OnFailure = null,
                OnSuccess = OnLoadAtlasSuccess,
                OnProgress = OnLoadAssetProgress,
            }, null);
        }

        private void OnLoadAtlasSuccess(IAssetAccessor assetAccessor, object context)
        {
            StartCoroutine(OnLoadAtlasSuccessCo(assetAccessor, context));
        }

        private IEnumerator OnLoadAtlasSuccessCo(IAssetAccessor assetAccessor, object context)
        {
            var prefab = (GameObject)assetAccessor.AssetObject;
            Instantiate(prefab);
            yield return new WaitForSeconds(5);
            Asset.UnloadAsset(assetAccessor);
            StartCoroutine(LoadDepPrefabsCo());
        }

        private IEnumerator LoadDepPrefabsCo()
        {
            yield return new WaitForSeconds(3);
            Asset.LoadAsset(m_PrefabADependsOnBPath, new LoadAssetCallbackSet
            {
                OnSuccess = OnLoadPrefabADependsOnBSuccess,
                OnFailure = OnLoadAssetFailure,
                OnProgress = OnLoadAssetProgress,
            }, null);
        }

        private void OnLoadPrefabADependsOnBSuccess(IAssetAccessor assetAccessor, object context)
        {
            StartCoroutine(OnLoadPrefabADependsOnBSuccessCo(assetAccessor, context));
        }

        private IEnumerator OnLoadPrefabADependsOnBSuccessCo(IAssetAccessor assetAccessor, object context)
        {
            var go = Instantiate((GameObject)assetAccessor.AssetObject);
            yield return new WaitForSeconds(2f);
            Destroy(go);
            Asset.UnloadAsset(assetAccessor);
            yield return new WaitForSeconds(2f);

            Asset.LoadAsset(m_GetPipTextPath, new LoadAssetCallbackSet
            {
                OnSuccess = (_assetAccessor, _context) => { LoadAnotherScene(); },
                OnFailure = OnLoadAssetFailure,
                OnProgress = OnLoadAssetProgress,
            }, null);
        }

        private void LoadAnotherScene()
        {
            Asset.LoadSceneAsset(m_SceneAssetPath,
                new LoadAssetCallbackSet
                {
                    OnFailure = OnLoadAssetFailure,
                    OnSuccess = (_assetAccessor, _context) =>
                    {
                        StartCoroutine(AfterLoadScene(_assetAccessor,
                            SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(m_SceneAssetPath), LoadSceneMode.Single)));
                    },
                    OnProgress = OnLoadAssetProgress,
                }, null);
        }

        private IEnumerator AfterLoadScene(IAssetAccessor sceneAssetAccessor, AsyncOperation loadSceneOp)
        {
            yield return loadSceneOp;
            yield return new WaitForSeconds(3);
            Asset.UnloadAsset(sceneAssetAccessor);
        }

        private void OnLoadAssetProgress(IAssetAccessor assetAccessor, float progress, object context)
        {
            Debug.Log($"[DemoAssetApp OnLoadAssetProgress] assetPath: {assetAccessor.AssetPath}, progress: {progress}");
        }

        [Serializable]
        private class RemoteIndexFileInfo
        {
            public int InternalAssetVersion = 0;
            public uint Crc32 = 0;
            public long FileSize = 0L;

            public static explicit operator AssetIndexRemoteFileInfo(RemoteIndexFileInfo self)
            {
                return new AssetIndexRemoteFileInfo(self.InternalAssetVersion, self.Crc32, self.FileSize);
            }
        }
    }
}