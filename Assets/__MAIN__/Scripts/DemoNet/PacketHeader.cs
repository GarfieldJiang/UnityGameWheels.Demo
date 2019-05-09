namespace COL.UnityGameWheels.Demo
{
    using Core.Net;
    using System;
    using System.IO;

    public class PacketHeader : IPacketHeader
    {
        private ushort m_PacketLength;
        private ushort m_PacketId;

        public int PacketLength
        {
            get
            {
                return m_PacketLength;
            }

            set
            {
                m_PacketLength = (ushort)value;
            }
        }

        public int PacketId
        {
            get
            {
                return m_PacketId;
            }

            set
            {
                m_PacketId = (ushort)value;
            }
        }

        public void Serialize(MemoryStream targetStream)
        {
            targetStream.Write(BitConverter.GetBytes(m_PacketId), 0, 2);
            targetStream.Write(BitConverter.GetBytes(m_PacketLength), 0, 2);
        }

        public void Deserialize(MemoryStream sourceStream)
        {
            var buffer = new byte[2];
            sourceStream.Read(buffer, 0, 2);
            m_PacketId = BitConverter.ToUInt16(buffer, 0);
            sourceStream.Read(buffer, 0, 2);
            m_PacketLength = BitConverter.ToUInt16(buffer, 0);
        }
    }
}
