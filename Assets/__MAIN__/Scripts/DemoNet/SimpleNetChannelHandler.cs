namespace COL.UnityGameWheels.Demo
{
    using System.IO;
    using Core.Net;
    using Unity;

    public class SimpleNetChannelHandler : INetChannelHandler
    {
        public Packet Deserialize(IPacketHeader packetHeader, MemoryStream sourceStream)
        {
            var packetId = packetHeader.PacketId;

            PacketBase packet = null;
            switch (packetId)
            {
                case 1:
                    packet = new Packet1();
                    break;
                case 2:
                default:
                    packet = new Packet2();
                    break;
            }

            packet.Deserialize(packetHeader, sourceStream);

            Log.InfoFormat("[SimpleNetChannelHandler Deserialize] packet={0}", packet);
            // Recycle packet header.
            return packet;
        }

        public IPacketHeader DeserializePacketHeader(MemoryStream sourceStream)
        {
            var packetHeader = new PacketHeader();
            packetHeader.Deserialize(sourceStream);

            Log.InfoFormat("[SimpleNetChannelHandler DeserializePacketHeader] packetId={0}, packetLength={1}", packetHeader.PacketId, packetHeader.PacketLength);
            return packetHeader;
        }

        public void OnConnected()
        {
            Log.Info("[SimpleNetChannelHandler OnConnected]");
        }

        public void OnError(string errorMessage, object errorData)
        {
            Log.WarningFormat("[SimpleNetChannelHandler OnError] errorMessage='{0}', errorData='{1}'.", errorMessage, errorData);
        }

        public void OnReceive(Packet packet)
        {
            Log.InfoFormat("[SimpleNetChannelHandler OnReceive] packet='{0}'.", packet);
            // Recycle packet.
        }

        public void OnRecycle(Packet packet)
        {
            Log.InfoFormat("[SimpleNetChannelHandler OnRecycle] packet='{0}'.", packet);
            // Recycle packet.
        }

        public void Serialize(Packet packet, MemoryStream targetStream)
        {
            var packetHeader = new PacketHeader
            {
                PacketId = packet.PacketId,
                PacketLength = ((PacketBase)packet).CalcSerializedLength()
            };
            packetHeader.Serialize(targetStream);
            Log.InfoFormat("[SimpleNetChannelHandler Serialize] packet='{0}'", packet);
            // Recycle packet header.
            ((PacketBase)packet).Serialize(targetStream);
        }
    }
}
