using System.Collections.Generic;
using System.Linq;
using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Core.Ioc;
using COL.UnityGameWheels.Core.Asset;
using System;
using System.Collections;
using COL.UnityGameWheels.Unity;
using COL.UnityGameWheels.Unity.Asset;
using COL.UnityGameWheels.Unity.Ioc;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COL.UnityGameWheels.Demo
{
    [DisallowMultipleComponent]
    public class DemoAssetApp : UnityApp
    {
        [SerializeField]
        private AssetServiceConfig m_AssetServiceConfig = null;

        [SerializeField]
        private DownloadServiceConfig m_DownloadServiceConfig = null;

        [SerializeField]
        private RefPoolServiceConfig m_RefPoolServiceConfig = null;

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

        [SerializeField]
        private string m_AssetPathWithLotsOfDependency = null;

        private int[] m_AvailableGroupIds = null;
        private readonly HashSet<int> m_GroupIdsToUpdate = new HashSet<int>();

        private IAssetService m_AssetService = null;

        private bool m_TriedToRestart = false;

        private IAssetService Asset
        {
            get
            {
                if (m_AssetService == null)
                {
                    m_AssetService = Container.Make<IAssetService>();
                }

                return m_AssetService;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Log.SetLogger(new LoggerImpl());
            Container.BindInstance<IRefPoolServiceConfigReader>(m_RefPoolServiceConfig);
            Container.BindSingleton<IRefPoolService, RefPoolService>();
            Container.BindInstance<IDownloadServiceConfigReader>(m_DownloadServiceConfig);
            Container.BindSingleton<IDownloadService, DownloadService>().OnInstanceCreated(StartTickingTickable);
            Container.BindSingleton<ISimpleFactory<IDownloadTaskImpl>, DownloadTaskImplFactory>();
            AssetServiceBinder.Bind(Container, m_AssetServiceConfig, this).OnInstanceCreated(StartTickingTickable);
        }

        private void Start()
        {
            Asset.Prepare(new AssetServicePrepareCallbackSet
            {
                OnSuccess = OnAssetManagerPrepareSuccess,
                OnFailure = OnAssetManagerPrepareFailure,
            }, "Fake context for preparation");
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
            if (Asset.ResourceUpdater.GetResourceGroupStatus(Core.Asset.Constant.CommonResourceGroupId) == ResourceGroupStatus.UpToDate)
            {
                ContinueUpdateResourceGroupsOrUseAssets();
                return;
            }

            Asset.ResourceUpdater.StartUpdatingResourceGroup(Core.Asset.Constant.CommonResourceGroupId, new ResourceGroupUpdateCallbackSet
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
                OnSuccess = (_assetAccessor, _context) =>
                {
                    Asset.UnloadAsset(_assetAccessor);
                    LoadAnotherScene();
                },
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
            LoadAssetWithLotsOfDependency();
        }

        private void LoadAssetWithLotsOfDependency()
        {
            if (Asset.GetAssetResourceGroupId(m_AssetPathWithLotsOfDependency) == Core.Asset.Constant.InvalidResourceGroupId)
            {
                Debug.LogWarning($"Asset '{m_AssetPathWithLotsOfDependency}' cannot be found.");
                return;
            }

            Debug.Log($"Loading asset (with lots of dependency) starts at {Time.time}.");
            Asset.LoadAsset(m_AssetPathWithLotsOfDependency, new LoadAssetCallbackSet
            {
                OnSuccess = OnLoadAssetWithLotsOfDependencySuccess,
            }, null);
        }

        private void OnLoadAssetWithLotsOfDependencySuccess(IAssetAccessor assetAccessor, object context)
        {
            Debug.Log($"Loading asset (with lots of dependency) ends at {Time.time}.");
            StartCoroutine(UnloadAssetWithLotsOfDependencyCo(assetAccessor));
        }

        private IEnumerator UnloadAssetWithLotsOfDependencyCo(IAssetAccessor assetAccessor)
        {
            yield return new WaitForSeconds(3);
            Debug.Log($"Unload asset (with lots of dependency).");
            Asset.UnloadAsset(assetAccessor);
        }

        private void OnLoadAssetProgress(IAssetAccessor assetAccessor, float progress, object context)
        {
            Debug.Log($"[DemoAssetApp OnLoadAssetProgress] assetPath: {assetAccessor.AssetPath}, progress: {progress}");
        }

        void OnGUI()
        {
            if (!m_TriedToRestart && GUILayout.Button("Restart"))
            {
                new GameObject("Restarter").AddComponent<Restarter>();
            }
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