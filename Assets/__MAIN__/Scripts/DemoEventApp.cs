using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Unity;
using System;
using System.Collections;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    [DisallowMultipleComponent]
    public class DemoEventApp : MonoBehaviourEx
    {
        private static DemoEventApp s_Instance = null;

        [SerializeField]
        private EventManager m_EventManager = null;

        [SerializeField]
        private RefPoolManager m_RefPoolManager = null;

        public static bool IsAvailable
        {
            get { return s_Instance != null; }
        }

        public static IEventManager Event
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_EventManager == null)
                {
                    throw new NullReferenceException("Event manager is invalid.");
                }

                return s_Instance.m_EventManager;
            }
        }

        public static IRefPoolManager RefPool
        {
            get
            {
                CheckInstanceOrThrow();
                if (s_Instance.m_RefPoolManager == null)
                {
                    throw new NullReferenceException("Object pool manager is invalid.");
                }

                return s_Instance.m_RefPoolManager;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            s_Instance = this;
        }

        private IEnumerator Start()
        {
            Event.EventArgsReleaser = new SimpleEventArgsReleaser(RefPool);

            RefPool.Init();
            Event.Init();

            Event.AddEventListener(TestEventArgs1.TheEventId, (sender, e) => { });
            Event.AddEventListener(TestEventArgs1.TheEventId, OnHearEvent1);
            Event.AddEventListener(TestEventArgs1.TheEventId, OnHearEvent1);
            yield return new WaitForSeconds(1f);
            Event.AddEventListener(TestEventArgs2.TheEventId, OnHearEvent2);
            Event.SendEvent(this, RefPool.GetOrAdd<TestEventArgs1>().Acquire());
            Event.SendEvent(this, RefPool.GetOrAdd<TestEventArgs2>().Acquire());
            yield return new WaitForSeconds(1f);
            Event.SendEventNow(this, RefPool.GetOrAdd<TestEventArgs1>().Acquire());
            yield return new WaitForSeconds(1f);
            Event.RemoveEventListener(TestEventArgs1.TheEventId, OnHearEvent1);
            Event.RemoveEventListener(TestEventArgs1.TheEventId, OnHearEvent1);
            Event.RemoveEventListener(TestEventArgs2.TheEventId, OnHearEvent2);
        }

        private void OnHearEvent1(object sender, BaseEventArgs eventArgs)
        {
            Debug.Log(eventArgs.GetType().Name);
        }

        private void OnHearEvent2(object sender, BaseEventArgs eventArgs)
        {
            Debug.Log(eventArgs.GetType().Name);
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

        private class SimpleEventArgsReleaser : IEventArgsReleaser
        {
            private readonly IRefPoolManager m_RefPoolManager;

            public SimpleEventArgsReleaser(IRefPoolManager refPoolManager)
            {
                m_RefPoolManager = refPoolManager;
            }

            public void Release(BaseEventArgs eventArgs)
            {
                // Clean up fields in event args instance, if needed.
                m_RefPoolManager.GetOrAdd(eventArgs.GetType()).ReleaseObject(eventArgs);
            }
        }
    }
}