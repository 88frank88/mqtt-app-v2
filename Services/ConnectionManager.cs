using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BetriebsmittelPublisher.Models;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher.Services
{
    public class ConnectionManager : IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private readonly object _lock = new object();

        public bool IsConnected { get; private set; }

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (IsConnected)
                    throw new InvalidOperationException("Already connected");
            }

            try
            {
                Logger.Info($"Verbinde zu MQTT Broker: {host}:{port}...");
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
                _networkStream = _tcpClient.GetStream();
                IsConnected = true;
                Logger.Info($"Verbunden mit {host}:{port}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Verbindung zu {host}:{port} fehlgeschlagen", ex);
                Cleanup();
                throw new InvalidOperationException($"Failed to connect to {host}:{port}", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            lock (_lock)
            {
                if (!IsConnected)
                    return;
            }

            try
            {
                if (_networkStream != null)
                {
                    await _networkStream.WriteAsync(MqttPacketBuilder.BuildDisconnectPacket(), 0, MqttPacketBuilder.BuildDisconnectPacket().Length, default).ConfigureAwait(false);
                }
            }
            catch
            {
                // Ignore errors during disconnect
            }
            finally
            {
                Cleanup();
            }
        }

        public async Task SendPacketAsync(byte[] packet, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (!IsConnected)
                    throw new InvalidOperationException("Not connected");
            }

            try
            {
                if (_networkStream == null)
                    throw new InvalidOperationException("Network stream is null");

                await _networkStream.WriteAsync(packet, 0, packet.Length, cancellationToken).ConfigureAwait(false);
                await _networkStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Cleanup();
                throw new InvalidOperationException("Failed to send packet", ex);
            }
        }

        public async Task<byte[]> ReceivePacketAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (!IsConnected)
                    throw new InvalidOperationException("Not connected");
            }

            try
            {
                if (_networkStream == null)
                    throw new InvalidOperationException("Network stream is null");

                // Read fixed header (2 bytes minimum)
                var fixedHeader = await MqttPacketParser.ReadExactBytesAsync(_networkStream, 2, cancellationToken);
                
                // Parse remaining length (variable byte encoding)
                int remainingLength = 0;
                int multiplier = 1;
                byte encodedByte;
                
                do
                {
                    encodedByte = await ReadByteAsync(_networkStream, cancellationToken);
                    remainingLength += (encodedByte & 0x7F) * multiplier;
                    multiplier *= 128;
                    
                    if (multiplier > 128 * 128 * 128)
                        throw new InvalidOperationException("Invalid remaining length encoding");
                        
                } while ((encodedByte & 0x80) != 0);
                
                // Read variable header + payload
                var packetData = await MqttPacketParser.ReadExactBytesAsync(_networkStream, remainingLength, cancellationToken);
                
                // Combine fixed header + packet data
                var fullPacket = new byte[2 + remainingLength];
                Array.Copy(fixedHeader, 0, fullPacket, 0, 2);
                Array.Copy(packetData, 0, fullPacket, 2, remainingLength);
                
                return fullPacket;
            }
            catch (Exception ex)
            {
                Cleanup();
                throw new InvalidOperationException("Failed to receive packet", ex);
            }
        }

        private async Task<byte> ReadByteAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[1];
            await stream.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false);
            return buffer[0];
        }

        public async Task PublishAsync(MqttMessage message, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            Logger.Info($"Publish zu Topic: {message.Topic} ({message.Payload.Length} bytes)");
            var packet = MqttPacketBuilder.BuildPublishPacket(message.Topic, message.Payload, message.QoS);
            await SendPacketAsync(packet, cancellationToken).ConfigureAwait(false);
            Logger.Info($"Publish erfolgreich: {message.Topic}");
        }

        public void Publish(MqttMessage message)
        {
            try
            {
                PublishAsync(message, default).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error("Publish fehlgeschlagen", ex);
                throw;
            }
        }

        private void Cleanup()
        {
            lock (_lock)
            {
                try
                {
                    _networkStream?.Close();
                    _networkStream?.Dispose();
                }
                catch { }
                finally
                {
                    _networkStream = null;
                }

                try
                {
                    _tcpClient?.Close();
                    _tcpClient?.Dispose();
                }
                catch { }
                finally
                {
                    _tcpClient = null;
                }

                IsConnected = false;
            }
        }

        public void Dispose()
        {
            try
            {
                DisconnectAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore errors during dispose
            }
        }
    }
}