namespace COL.UnityGameWheels.Demo
{
    using System.Collections;
    using System.IO;
    using Unity;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class DemoLogCollectionApp : MonoBehaviourEx
    {
        [SerializeField]
        private LogCollectionManager m_LogCollection = null;

        [SerializeField]
        private string m_LogFileName = "DemoLog.txt";

        private string LogFilePath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, m_LogFileName);
            }
        }

        private ILogCollectionManager LogCollection
        {
            get
            {
                return m_LogCollection;
            }
        }

        private ILogCollector m_LogCollector = null;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            Log.SetLogger(new LoggerImpl());

            LogCollection.LogCallbackRegistrar = new DefaultLogCallbackRegistrar();

            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }

            var logCollector = new LogCollector();
            logCollector.LogFilePath = LogFilePath;
            m_LogCollector = logCollector;
        }

        private IEnumerator Start()
        {
            LogCollection.Init();
            Log.Info("First log message");
            yield return null;
            LogCollection.AddLogCollector(m_LogCollector);
            Log.Warning("Second log message");
            Log.Info("Third log message");
            yield return null;
            LogCollection.RemoveLogCollector(m_LogCollector);
            Log.Info("Fourth log message");
            yield return null;
            Log.Info(File.ReadAllText(LogFilePath));
        }

        private class LogCollector : ILogCollector
        {
            public string LogFilePath;

            public void OnReceiveLogEntry(LogEntry logEntry)
            {
                File.AppendAllText(LogFilePath, Core.Utility.Text.Format("[{0}]{1}\n", logEntry.LogType, logEntry.LogMessage));
            }
        }
    }
}
