using System.Threading;

namespace COL.UnityGameWheels.Demo
{
    using Core.Net;
    using System.Collections;
    using System.Net;
    using Unity;
    using Unity.Net;
    using UnityEngine;

    public class DemoNetApp : MonoBehaviourEx
    {
        [SerializeField] private NetManager m_Net = null;

        public NetManager Net
        {
            get { return m_Net; }
        }

        override protected void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            Log.SetLogger(new LoggerImpl());
        }

        override protected void OnDestroy()
        {
            Log.Info("[DemoNetApp OnDestroy]");
            Net.ShutDown();
            base.OnDestroy();
        }

        private IEnumerator Start()
        {
            Net.ChannelFactory = new DefaultNetChannelFactory();
            Net.Init();
            var channel = Net.AddChannel("Simple channel", null, new SimpleNetChannelHandler(), 4);
            channel.Connect(IPAddress.Parse("127.0.0.1"), 5555);
            yield return new WaitForSeconds(3);

            for (int i = 0; i < 100; i++)
            {
                channel.Send(new Packet1 {MyInt = i, MyString = new string((i % 10).ToString()[0], 8192)});
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