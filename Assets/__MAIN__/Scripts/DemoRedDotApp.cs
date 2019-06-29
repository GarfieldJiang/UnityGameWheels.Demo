using COL.UnityGameWheels.Core.RedDot;
using COL.UnityGameWheels.Unity.RedDot;
using UnityEngine.UI;
using System;
using COL.UnityGameWheels.Unity;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    [DisallowMultipleComponent]
    public class DemoRedDotApp : MonoBehaviourEx
    {
        private static DemoRedDotApp s_Instance = null;

        [SerializeField]
        private RedDotManager m_RedDotManager = null;

        [SerializeField]
        private string[] m_LeafConfigs = null;

        [SerializeField]
        private RedDotNonLeafConfig[] m_NonLeafConfigs = null;

        public static IRedDotManager RedDotManager
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_RedDotManager == null)
                {
                    throw new NullReferenceException("Download manager is invalid.");
                }

                return s_Instance.m_RedDotManager;
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
            foreach (var nonLeaf in m_NonLeafConfigs)
            {
                m_RedDotManager.AddNonLeaf(nonLeaf.Key, nonLeaf.Operation, nonLeaf.DependsOn);
            }

            foreach (var leaf in m_LeafConfigs)
            {
                m_RedDotManager.AddLeaf(leaf);
            }

            m_RedDotManager.SetUp();

            foreach (var leaf in m_LeafConfigs)
            {
                var text = GameObject.Find(leaf).GetComponent<Text>();
                RedDotManager.AddObserver(leaf, new RedDotObserver {TextWidget = text, OriginalText = text.text});
            }

            foreach (var nonLeaf in m_NonLeafConfigs)
            {
                var text = GameObject.Find(nonLeaf.Key).GetComponent<Text>();
                RedDotManager.AddObserver(nonLeaf.Key, new RedDotObserver {TextWidget = text, OriginalText = text.text});
            }
        }

        private static void CheckInstanceOrThrow()
        {
            if (s_Instance == null)
            {
                throw new NullReferenceException("App instance is invalid.");
            }
        }

        [Serializable]
        private class RedDotNonLeafConfig
        {
            public string Key;
            public NonLeafOperation Operation;
            public string[] DependsOn;
        }

        private class RedDotObserver : IRedDotObserver
        {
            public string OriginalText;
            public Text TextWidget;

            public void OnChange(string key, int value)
            {
                TextWidget.text = OriginalText + " " + value;
            }
        }
    }
}