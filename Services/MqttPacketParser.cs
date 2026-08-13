using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetriebsmittelPublisher.Services
{
    public static class MqttPacketParser
    {
        public static Models.MqttPacketType ParsePacketType(byte fixedHeader)
        {
            return (Models.MqttPacketType)(fixedHeader >> 4);
        }

        public static byte ParseFlags(byte fixedHeader)
        {
            return (byte)(fixedHeader & 0x0F);
        }

        public static int ParseRemainingLength(Stream stream)
        {
            int remainingLength = 0;
            int multiplier = 1;
            byte encodedByte;
            
            do
            {
                encodedByte = (byte)stream.ReadByte();
                remainingLength += (encodedByte & 0x7F) * multiplier;
                multiplier *= 128;
                
                if (multiplier > 128 * 128 * 128)
                    throw new InvalidOperationException("Invalid remaining length encoding");
                    
            } while ((encodedByte & 0x80) != 0);
            
            return remainingLength;
        }

        public static async Task<Models.MqttConnAckMessage> ParseConnectAckAsync(Stream stream, CancellationToken cancellationToken)
        {
            // Fixed header already consumed, read return code
            var returnCode = (byte)stream.ReadByte();
            
            return new Models.MqttConnAckMessage
            {
                SessionPresent = false, // Simplified for now
                ReturnCode = returnCode
            };
        }

        public static async Task ParsePublishAckAsync(Stream stream, CancellationToken cancellationToken)
        {
            // PUBACK has 2-byte packet identifier
            await ReadExactBytesAsync(stream, 2, cancellationToken);
        }

        public static async Task ParsePingResponseAsync(Stream stream, CancellationToken cancellationToken)
        {
            // PINGRESP has no payload, just consume any remaining bytes if present
        }

        public static async Task<byte[]> ReadExactBytesAsync(Stream stream, int count, CancellationToken cancellationToken)
        {
            var buffer = new byte[count];
            int totalRead = 0;
            
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, totalRead, count - totalRead, cancellationToken);
                if (read == 0)
                    throw new IOException("Stream closed unexpectedly");
                totalRead += read;
            }
            
            return buffer;
        }

        public static string ReadString(Stream stream)
        {
            var lengthBytes = new byte[2];
            if (stream.Read(lengthBytes, 0, 2) != 2)
                throw new IOException("Unexpected end of stream");
                
            ushort length = (ushort)((lengthBytes[0] << 8) | lengthBytes[1]);
            var stringBytes = new byte[length];
            
            if (stream.Read(stringBytes, 0, length) != length)
                throw new IOException("Unexpected end of stream");
                
            return Encoding.UTF8.GetString(stringBytes);
        }

        public static ushort ReadUInt16(Stream stream)
        {
            var bytes = new byte[2];
            if (stream.Read(bytes, 0, 2) != 2)
                throw new IOException("Unexpected end of stream");
                
            return (ushort)((bytes[0] << 8) | bytes[1]);
        }
    }
}