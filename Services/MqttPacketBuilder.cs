using System;
using System.IO;
using System.Text;

namespace BetriebsmittelPublisher.Services
{
    public static class MqttPacketBuilder
    {
        public static byte[] BuildFixedHeader(byte packetType, byte flags, int remainingLength)
        {
            var header = new MemoryStream();
            
            // Byte 1: Message Type (4 bits) + Flags (4 bits)
            byte firstByte = (byte)((packetType << 4) | (flags & 0x0F));
            header.WriteByte(firstByte);
            
            // Byte 2-N: Variable byte encoding for remaining length
            EncodeRemainingLength(header, remainingLength);
            
            return header.ToArray();
        }

        private static void EncodeRemainingLength(MemoryStream stream, int length)
        {
            do
            {
                byte encodedByte = (byte)(length % 128);
                length /= 128;
                
                if (length > 0)
                    encodedByte |= 0x80; // Set continue bit
                    
                stream.WriteByte(encodedByte);
            } while (length > 0);
        }

        public static byte[] BuildConnectPacket(Models.MqttConnectMessage connectMessage)
        {
            var packet = new MemoryStream();
            
            // Fixed Header: CONNECT = 0x10, Flags = 0x00
            var fixedHeader = BuildFixedHeader(1, 0x00, 0); // Placeholder length
            
            // Variable Header: Protocol Name + Version + Flags + Keep-Alive
            WriteString(packet, "MQTT"); // Protocol name
            packet.WriteByte(0x04);      // Protocol version (3.1.1)
            
            byte connectFlags = 0x00;
            if (!string.IsNullOrEmpty(connectMessage.Username)) connectFlags |= 0x80;
            if (!string.IsNullOrEmpty(connectMessage.Password)) connectFlags |= 0x40;
            if (connectMessage.CleanSession) connectFlags |= 0x02;
            packet.WriteByte(connectFlags);
            
            WriteUInt16(packet, connectMessage.KeepAlive); // Keep-alive in seconds
            
            // Payload: Client ID + optional username/password
            WriteString(packet, connectMessage.ClientId);
            if (!string.IsNullOrEmpty(connectMessage.Username)) WriteString(packet, connectMessage.Username);
            if (!string.IsNullOrEmpty(connectMessage.Password)) WriteString(packet, connectMessage.Password);
            
            // Calculate remaining length and rebuild fixed header
            var packetBytes = packet.ToArray();
            var remainingLength = packetBytes.Length;
            fixedHeader = BuildFixedHeader(1, 0x00, remainingLength);
            
            // Combine fixed header + variable header + payload
            var fullPacket = new byte[fixedHeader.Length + remainingLength];
            Array.Copy(fixedHeader, 0, fullPacket, 0, fixedHeader.Length);
            Array.Copy(packetBytes, 0, fullPacket, fixedHeader.Length, remainingLength);
            
            return fullPacket;
        }

        public static byte[] BuildPublishPacket(Models.MqttPublishMessage publishMessage)
        {
            var packet = new MemoryStream();
            
            // Variable Header: Topic Name + Packet ID (for QoS 1/2)
            WriteString(packet, publishMessage.Topic);
            
            if (publishMessage.QoS != Models.MqttQoS.AtMostOnce)
            {
                WriteUInt16(packet, publishMessage.PacketId);
            }
            
            // Payload
            if (publishMessage.Payload != null)
            {
                packet.Write(publishMessage.Payload, 0, publishMessage.Payload.Length);
            }
            
            var variableHeaderAndPayload = packet.ToArray();
            var remainingLength = variableHeaderAndPayload.Length;
            
            // Fixed Header: PUBLISH = 0x30
            byte flags = 0x00;
            if (publishMessage.Dup) flags |= 0x08;
            flags |= (byte)((int)publishMessage.QoS << 1);
            if (publishMessage.Retain) flags |= 0x01;
            
            var fixedHeader = BuildFixedHeader(3, flags, remainingLength);
            
            // Combine fixed header + variable header + payload
            var fullPacket = new byte[fixedHeader.Length + remainingLength];
            Array.Copy(fixedHeader, 0, fullPacket, 0, fixedHeader.Length);
            Array.Copy(variableHeaderAndPayload, 0, fullPacket, fixedHeader.Length, remainingLength);
            
            return fullPacket;
        }

        public static byte[] BuildPingReqPacket()
        {
            // PINGREQ has no variable header or payload
            return BuildFixedHeader(12, 0x00, 0);
        }

        public static byte[] BuildDisconnectPacket()
        {
            // DISCONNECT has no variable header or payload
            return BuildFixedHeader(14, 0x00, 0);
        }

        private static void WriteString(MemoryStream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            WriteUInt16(stream, (ushort)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteUInt16(MemoryStream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));   // High byte
            stream.WriteByte((byte)(value & 0xFF)); // Low byte
        }
    }
}