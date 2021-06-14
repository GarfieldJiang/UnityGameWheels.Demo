using System.Threading;
using System.Collections;
using System.Net;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    using Core.Net;
    using Core.Ioc;
    using Unity;
    using Unity.Ioc;

    [DisallowMultipleComponent]
    public class DemoNetApp : UnityApp
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            Log.SetLogger(new LoggerImpl());
            Container.BindSingleton<INetService, NetService>().OnInstanceCreated(StartTickingTickable);
            Container.BindSingleton<INetChannelFactory, DefaultNetChannelFactory>();
        }

        private IEnumerator Start()
        {
            var netService = Container.Make<INetService>();
            var channel = netService.AddChannel("Simple channel", null, new SimpleNetChannelHandler(), 4);
            channel.Connect(IPAddress.Parse("127.0.0.1"), 5555);
            yield return new WaitForSeconds(3);

            for (int i = 0; i < 100; i++)
            {
                channel.Send(new Packet1 { MyInt = i, MyString = new string((i % 10).ToString()[0], 8192) });
            }

            yield return new WaitForSeconds(3);

            for (int i = 0; i < 100; i++)
            {
                var j = i;
                new Thread(() =>
                {
                    channel.Send(new Packet2
                    {
                        MyBoolean = j % 2 == 0,
                        MyString = new string((j % 10).ToString()[0], 4096)
                    });
                }).Start();
            }
        }
    }
}