namespace COL.UnityGameWheels.Demo
{
    using Core;
    using Core.Ioc;
    using Unity;
    using Unity.Ioc;
    using System;
    using System.IO;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class DemoDownloadApp : UnityApp
    {
        private static DemoDownloadApp s_Instance = null;

        [SerializeField]
        private RefPoolServiceConfig m_RefPoolServiceConfig = null;

        [SerializeField]
        private DownloadServiceConfig m_DownloadServiceConfig = null;

        [SerializeField]
        private DownloadInfo[] m_DownloadInfos = null;

        protected override void Awake()
        {
            base.Awake();
            s_Instance = this;

            Log.SetLogger(new LoggerImpl());
            Container.BindSingleton<IRefPoolService, RefPoolService>();
            Container.BindInstance<IRefPoolServiceConfigReader>(m_RefPoolServiceConfig);
            Container.BindSingleton<IDownloadService, DownloadService>().OnInstanceCreated(StartTickingTickable);
            Container.BindInstance<IDownloadServiceConfigReader>(m_DownloadServiceConfig);
            Container.BindSingleton<ISimpleFactory<IDownloadTaskImpl>, DownloadTaskImplFactory>();
        }

        private void Start()
        {
            for (int i = 0; i < m_DownloadInfos.Length; i++)
            {
                if (!m_DownloadInfos[i].IsActive)
                {
                    continue;
                }

                var downloadTaskInfo = new DownloadTaskInfo(
                    urlStr: m_DownloadInfos[i].UrlStr,
                    savePath: Application.persistentDataPath + Path.DirectorySeparatorChar + m_DownloadInfos[i].SavePath,
                    size: m_DownloadInfos[i].Size,
                    crc32: m_DownloadInfos[i].CheckCrc32 ? m_DownloadInfos[i].Crc32 : (uint?)null,
                    callbackSet: new DownloadCallbackSet
                    {
                        OnSuccess = OnDownloadSuccess,
                        OnFailure = OnDownloadFailure,
                        OnProgress = OnDownloadProgress,
                    },
                    context: null);

                Container.Make<IDownloadService>().StartDownloading(downloadTaskInfo);
            }
        }

        private void OnDownloadSuccess(int taskId, DownloadTaskInfo info)
        {
            Debug.LogFormat("Download '{0}' to '{1}' succeeded.", info.UrlStr, info.SavePath);
        }

        private void OnDownloadFailure(int taskId, DownloadTaskInfo info, DownloadErrorCode errorCode, string errorMessage)
        {
            Debug.LogWarningFormat("Download '{0}' to '{1}' failed with error code '{2}' and message '{3}'.", info.UrlStr, info.SavePath,
                errorCode, errorMessage);
        }

        private void OnDownloadProgress(int taskId, DownloadTaskInfo info, long downloadedSize)
        {
            // Debug.LogFormat("Download '{0}' progressed to {1} bytes.", info.UrlStr, downloadedSize);
        }

        protected override void OnDestroy()
        {
            s_Instance = null;
            base.OnDestroy();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("ShutDown"))
            {
                Destroy(gameObject);
            }

            if (GUILayout.Button("Disk Full"))
            {
                DownloadTask.StaticDebugOptions.SetIOExceptionMsg(DownloadTask.StaticDebugOptions.IOExceptionMsg.DiskFull);
                DownloadTask.StaticDebugOptions.SetIOExceptionScenario(DownloadTask.StaticDebugOptions.IOExceptionScenario.OnWriteFile, 100);
            }
        }

        [Serializable]
        private class DownloadInfo
        {
            public string SavePath = string.Empty;
            public bool CheckCrc32 = true;
            public bool IsActive = true;
            public uint Crc32 = 0;
            public string UrlStr = string.Empty;
            public long Size = 0L;
        }
    }
}