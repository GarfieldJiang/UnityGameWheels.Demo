namespace COL.UnityGameWheels.Demo
{
    using Core.Net;
    using System.IO;

    public abstract class PacketBase : Packet
    {
        protected byte[] m_Buffer = new byte[16384];

        public abstract void Serialize(MemoryStream targetStream);

        public abstract void Deserialize(IPacketHeader packetHeader, MemoryStream sourceStream);

        public abstract int CalcSerializedLength();
    }
}
