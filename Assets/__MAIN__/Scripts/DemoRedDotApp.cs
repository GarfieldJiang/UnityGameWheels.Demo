using COL.UnityGameWheels.Core.RedDot;
using UnityEngine.UI;
using System;
using COL.UnityGameWheels.Unity;
using COL.UnityGameWheels.Unity.Ioc;
using COL.UnityGameWheels.Core.Ioc;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    [DisallowMultipleComponent]
    public class DemoRedDotApp : UnityApp
    {
        private static DemoRedDotApp s_Instance = null;

        [SerializeField]
        private string[] m_LeafConfigs = null;

        [SerializeField]
        private RedDotNonLeafConfig[] m_NonLeafConfigs = null;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            s_Instance = this;
            Log.SetLogger(new LoggerImpl());
            Container.BindSingleton<IRedDotService, RedDotService>();
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
                Container.Make<IRedDotService>().AddNonLeaf(nonLeaf.Key, nonLeaf.Operation, nonLeaf.DependsOn);
            }

            foreach (var leaf in m_LeafConfigs)
            {
                Container.Make<IRedDotService>().AddLeaf(leaf);
            }

            Container.Make<IRedDotService>().SetUp();

            foreach (var leaf in m_LeafConfigs)
            {
                var text = GameObject.Find(leaf).GetComponent<Text>();
                Container.Make<IRedDotService>().AddObserver(leaf, new RedDotObserver {TextWidget = text, OriginalText = text.text});
            }

            foreach (var nonLeaf in m_NonLeafConfigs)
            {
                var text = GameObject.Find(nonLeaf.Key).GetComponent<Text>();
                Container.Make<IRedDotService>().AddObserver(nonLeaf.Key, new RedDotObserver {TextWidget = text, OriginalText = text.text});
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
            public string Key = null;
            public NonLeafOperation Operation = NonLeafOperation.Sum;
            public string[] DependsOn = null;
        }

        private class RedDotObserver : IRedDotObserver
        {
            public string OriginalText = null;
            public Text TextWidget = null;

            public void OnChange(string key, int value)
            {
                TextWidget.text = OriginalText + " " + value;
            }
        }
    }
}