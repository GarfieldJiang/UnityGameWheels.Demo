namespace COL.UnityGameWheels.Demo
{
    using Core.Net;
    using System;
    using System.IO;
    using System.Text;

    public class Packet1 : PacketBase
    {
        public int MyInt;

        public string MyString;

        public override int PacketId
        {
            get
            {
                return 1;
            }
        }

        public override int CalcSerializedLength()
        {
            return sizeof(int) + Encoding.ASCII.GetByteCount(MyString);
        }

        public override void Deserialize(IPacketHeader packetHeader, MemoryStream sourceStream)
        {
            sourceStream.Read(m_Buffer, 0, 4);
            MyInt = BitConverter.ToInt32(m_Buffer, 0);
            sourceStream.Read(m_Buffer, 0, packetHeader.PacketLength - 4);
            MyString = Encoding.ASCII.GetString(m_Buffer, 0, packetHeader.PacketLength - 4);
        }

        public override void Serialize(MemoryStream targetStream)
        {
            var intBuffer = BitConverter.GetBytes(MyInt);
            targetStream.Write(intBuffer, 0, intBuffer.Length);
            var stringBuffer = Encoding.ASCII.GetBytes(MyString);
            targetStream.Write(stringBuffer, 0, stringBuffer.Length);
        }

        public override string ToString()
        {
            return Core.Utility.Text.Format("PacketId={0}, MyInt={1}, MyString={2}", PacketId, MyInt, MyString);
        }
    }
}
