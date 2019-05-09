using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Demo
{
    public class TestEventArgs1 : BaseEventArgs
    {
        public static readonly int TheEventId = EventIdToTypeMap.Generate<TestEventArgs1>();

        public override int EventId => TheEventId;
    }
}
