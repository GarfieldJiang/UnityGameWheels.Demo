namespace COL.UnityGameWheels.Demo
{
    using Core.Net;
    using System;
    using System.IO;
    using System.Text;

    public class Packet2 : PacketBase
    {
        public bool MyBoolean;

        public string MyString;

        public override int PacketId
        {
            get
            {
                return 2;
            }
        }

        public override int CalcSerializedLength()
        {
            return sizeof(bool) + Encoding.ASCII.GetByteCount(MyString);
        }

        public override void Deserialize(IPacketHeader packetHeader, MemoryStream sourceStream)
        {
            sourceStream.Read(m_Buffer, 0, packetHeader.PacketLength - 1);
            MyString = Encoding.ASCII.GetString(m_Buffer, 0, packetHeader.PacketLength - 1);
            sourceStream.Read(m_Buffer, 0, 1);
            MyBoolean = BitConverter.ToBoolean(m_Buffer, 0);
        }

        public override void Serialize(MemoryStream targetStream)
        {
            var stringBuffer = Encoding.ASCII.GetBytes(MyString);
            targetStream.Write(stringBuffer, 0, stringBuffer.Length);
            var boolBuffer = BitConverter.GetBytes(MyBoolean);
            targetStream.Write(boolBuffer, 0, boolBuffer.Length);
        }
        public override string ToString()
        {
            return Core.Utility.Text.Format("PacketId={0}, MyBoolean={1}, MyString={2}", PacketId, MyBoolean, MyString);
        }
    }
}
