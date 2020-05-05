using COL.UnityGameWheels.Unity;
using COL.UnityGameWheels.Unity.Ioc;
using COL.UnityGameWheels.Core.Ioc;
using System.Collections;
using System.IO;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    [DisallowMultipleComponent]
    public class DemoLogCollectionApp : UnityApp
    {
        [SerializeField]
        private string m_LogFileName = "DemoLog.txt";

        private string LogFilePath => Path.Combine(Application.persistentDataPath, m_LogFileName);

        private ILogCollector m_LogCollector = null;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            Log.SetLogger(new LoggerImpl());
            Container.BindSingleton<ILogCollectionService, LogCollectionService>();
            Container.BindSingleton<ILogCallbackRegistrar, DefaultLogCallbackRegistrar>();

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
            var LogCollection = Container.Make<ILogCollectionService>();
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