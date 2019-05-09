namespace COL.UnityGameWheels.Demo
{
    using Core;
    using Unity;
    using System;
    using System.IO;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class DemoDownloadApp : MonoBehaviourEx
    {
        private static DemoDownloadApp s_Instance = null;

        [SerializeField]
        private RefPoolManager m_RefPoolManager = null;

        [SerializeField]
        private DownloadManager m_DownloadManager = null;

        [SerializeField]
        private DownloadInfo[] m_DownloadInfos = null;

        public static bool IsAvailable
        {
            get { return s_Instance != null; }
        }

        public static IRefPoolManager RefPool
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_RefPoolManager == null)
                {
                    throw new NullReferenceException("Reference pool manager is invalid.");
                }

                return s_Instance.m_RefPoolManager;
            }
        }

        public static IDownloadManager Download
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_DownloadManager == null)
                {
                    throw new NullReferenceException("Download manager is invalid.");
                }

                return s_Instance.m_DownloadManager;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            s_Instance = this;
        }

        private void Start()
        {
            Download.RefPoolModule = RefPool.Module;

            RefPool.Init();
            Download.Init();

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
                    crc32: m_DownloadInfos[i].CheckCrc32 ? m_DownloadInfos[i].Crc32 : (uint?) null,
                    callbackSet: new DownloadCallbackSet
                    {
                        OnSuccess = OnDownloadSuccess,
                        OnFailure = OnDownloadFailure,
                        OnProgress = OnDownloadProgress,
                    },
                    context: null);

                Download.StartDownloading(downloadTaskInfo);
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

        private static void CheckInstanceOrThrow()
        {
            if (s_Instance == null)
            {
                throw new NullReferenceException("App instance is invalid.");
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