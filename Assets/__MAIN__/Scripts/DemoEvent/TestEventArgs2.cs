using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Demo
{
    public class TestEventArgs2 : BaseEventArgs
    {
        public static readonly int TheEventId = EventIdToTypeMap.Generate<TestEventArgs2>();

        public override int EventId => TheEventId;
    }
}
