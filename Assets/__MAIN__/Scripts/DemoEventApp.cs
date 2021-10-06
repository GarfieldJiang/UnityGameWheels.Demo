using COL.UnityGameWheels.Core;
using COL.UnityGameWheels.Unity;
using System.Collections;
using COL.UnityGameWheels.Core.Ioc;
using COL.UnityGameWheels.Unity.Ioc;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    [DisallowMultipleComponent]
    public class DemoEventApp : UnityApp
    {
        [SerializeField]
        private RefPoolServiceConfig m_RefPoolServiceConfig = null;

        protected override void Awake()
        {
            base.Awake();
            Log.SetLogger(new LoggerImpl());
            Container.BindInstance<IRefPoolServiceConfigReader>(m_RefPoolServiceConfig);
            Container.BindSingleton<IEventArgsReleaser, SimpleEventArgsReleaser>();
            Container.BindSingleton<IRefPoolService, RefPoolService>();
            Container.BindSingleton<IEventService, EventService>().OnInstanceCreated(serviceInstance =>
            {
                var eventService = (EventService)serviceInstance;
                eventService.MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                eventService.StartTicking();
            });
        }

        private IEnumerator Start()
        {
            var Event = Container.Make<IEventService>();
            var RefPool = Container.Make<IRefPoolService>();

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
            Log.Info(eventArgs.GetType().Name);
        }

        private void OnHearEvent2(object sender, BaseEventArgs eventArgs)
        {
            Log.Info(eventArgs.GetType().Name);
        }


        private class SimpleEventArgsReleaser : IEventArgsReleaser
        {
            [Inject]
            public IRefPoolService RefPoolService { get; set; }

            public void Release(BaseEventArgs eventArgs)
            {
                // Clean up fields in event args instance, if needed.
                RefPoolService.GetOrAdd(eventArgs.GetType()).ReleaseObject(eventArgs);
            }
        }
    }
}