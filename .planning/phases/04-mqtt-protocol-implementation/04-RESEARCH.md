# Phase 4: MQTT Protocol Implementation - Research

## Standard Stack

**Core Technologies:**
- .NET 10 Networking APIs: `System.Net.Sockets.TcpClient`, `System.Net.Security.NetworkStream`
- MQTT 3.1.1 Protocol Specification (OASIS standard)
- Binary packet encoding/decoding with `System.BinaryPrimitives`
- Async/await patterns for network operations

**No External Dependencies:** All MQTT functionality must be implemented from scratch due to zero NuGet constraint.

## Architecture Patterns

### MQTT Packet Structure Pattern
```
[Fixed Header: 2 bytes] + [Variable Header: 0+ bytes] + [Payload: 0+ bytes]

Fixed Header:
- Byte 1: Message Type (4 bits) + Flags (4 bits)
- Byte 2-N: Remaining Length (variable byte encoding)
```

### Connection Management Pattern
```
1. TcpClient.ConnectAsync() - establish TCP connection
2. NetworkStream.GetStream() - get stream for read/write
3. Send CONNECT packet - handshake with broker
4. Event-driven message handling - background read loop
5. Keep-alive timer - PINGREQ/PINGRESP exchange
6. Graceful disconnect - DISCONNECT packet + close
```

### Packet Encoding Pattern
```
- Use MemoryStream + BinaryWriter for packet construction
- Use BinaryPrimitives for cross-platform integer encoding
- Variable byte encoding for remaining length field
- UTF-8 encoding for string fields (client ID, topics)
```

## Don't Hand-Roll

**Avoid implementing:**
- Custom TCP protocol wrappers (use TcpClient directly)
- Manual SSL/TLS implementation (use SslStream if needed)
- Custom thread pools (use Task.Run/async-await)
- Custom buffer management (use ArrayPool<byte>.Shared)
- Custom serialization protocols (use MQTT specification exactly)

## Common Pitfalls

### MQTT Protocol Pitfalls
1. **Incorrect Remaining Length Encoding** [VERIFIED: MQTT 3.1.1 Spec]
   - Must use variable byte encoding: `continue bit + 7-bit data`
   - Maximum 4 bytes for remaining length (max 268,435,455 bytes)

2. **Missing Quality of Service (QoS) Handling** [ASSUMED]
   - QoS 0: At most once delivery (no acknowledgment)
   - QoS 1: At least once delivery (PUBACK)
   - QoS 2: Exactly once delivery (PUBREC/PUBREL/PUBCOMP)

3. **Incorrect Packet Identifier Management** [ASSUMED]
   - Must be unique per message flow (0-65535)
   - Increment after use, wrap around at 65535
   - 0 is reserved (must not be used)

4. **Keep-Alive Violations** [VERIFIED: MQTT 3.1.1 Spec]
   - Client must send PINGREQ if no other packets within keep-alive period
   - Server disconnects if 1.5x keep-alive period elapses without activity

### .NET Networking Pitfalls
1. **Blocking Network Operations** [VERIFIED: .NET Docs]
   - Use async methods (ReadAsync, WriteAsync) never blocking calls
   - Configure TcpClient.ReceiveTimeout/SendTimeout appropriately
   - Handle OperationCanceledException for graceful shutdown

2. **Memory Leaks in Stream Handling** [ASSUMED]
   - Always dispose NetworkStream and TcpClient (using statements)
   - Avoid large buffer allocations (use ArrayPool<byte>.Shared)
   - Clear buffers containing sensitive data

3. **Thread Safety Issues** [ASSUMED]
   - NetworkStream is not thread-safe (single reader/writer)
   - Use ConcurrentQueue for message passing between threads
   - Lock state transitions (connecting → connected → disconnecting)

4. **Exception Handling Gaps** [VERIFIED: .NET Docs]
   - SocketException: network-level errors (connection reset, timeout)
   - IOException: stream-level errors (invalid data, premature close)
   - ObjectDisposedException: cleanup after disposal

## Code Examples

### MQTT Fixed Header Construction
```csharp
public byte[] BuildFixedHeader(byte messageType, byte flags, int remainingLength)
{
    var header = new MemoryStream();
    
    // Byte 1: Message Type (4 bits) + Flags (4 bits)
    byte firstByte = (byte)((messageType << 4) | (flags & 0x0F));
    header.WriteByte(firstByte);
    
    // Byte 2-N: Variable byte encoding for remaining length
    EncodeRemainingLength(header, remainingLength);
    
    return header.ToArray();
}

private void EncodeRemainingLength(MemoryStream stream, int length)
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
```

### CONNECT Packet Construction
```csharp
public byte[] BuildConnectPacket(string clientId, string username = null, string password = null)
{
    var packet = new MemoryStream();
    
    // Fixed Header: CONNECT = 0x10, Flags = 0x00
    packet.Write(BuildFixedHeader(0x01, 0x00, 0), 0, 2); // Placeholder length
    
    // Variable Header: Protocol Name + Version + Flags + Keep-Alive
    WriteString(packet, "MQTT"); // Protocol name
    packet.WriteByte(0x04);      // Protocol version (3.1.1)
    
    byte connectFlags = 0x00;
    if (!string.IsNullOrEmpty(username)) connectFlags |= 0x80;
    if (!string.IsNullOrEmpty(password)) connectFlags |= 0x40;
    connectFlags |= 0x02; // Clean session
    packet.WriteByte(connectFlags);
    
    WriteUInt16(packet, 60); // Keep-alive: 60 seconds
    
    // Payload: Client ID + optional username/password
    WriteString(packet, clientId);
    if (!string.IsNullOrEmpty(username)) WriteString(packet, username);
    if (!string.IsNullOrEmpty(password)) WriteString(packet, password);
    
    // Update remaining length in fixed header
    var packetBytes = packet.ToArray();
    var remainingLength = packetBytes.Length - 2; // Exclude fixed header
    var fixedHeader = BuildFixedHeader(0x01, 0x00, remainingLength);
    Array.Copy(fixedHeader, 0, packetBytes, 0, 2);
    
    return packetBytes;
}
```

### NetworkStream Reading Pattern
```csharp
public async Task<byte[]> ReadPacketAsync(NetworkStream stream, CancellationToken cancellationToken)
{
    // Read fixed header (2 bytes minimum)
    var fixedHeader = await ReadExactBytesAsync(stream, 2, cancellationToken);
    
    // Parse remaining length (variable byte encoding)
    int remainingLength = 0;
    int multiplier = 1;
    byte encodedByte;
    int offset = 1;
    
    do
    {
        encodedByte = await ReadExactByteAsync(stream, cancellationToken);
        remainingLength += (encodedByte & 0x7F) * multiplier;
        multiplier *= 128;
        offset++;
        
        if (multiplier > 128 * 128 * 128)
            throw new InvalidOperationException("Invalid remaining length encoding");
            
    } while ((encodedByte & 0x80) != 0);
    
    // Read variable header + payload
    var packetData = await ReadExactBytesAsync(stream, remainingLength, cancellationToken);
    
    // Combine fixed header + packet data
    var fullPacket = new byte[2 + offset - 1 + remainingLength];
    Array.Copy(fixedHeader, 0, fullPacket, 0, 2);
    fixedHeader[1] = (byte)remainingLength; // Simplified for single-byte remaining length
    Array.Copy(packetData, 0, fullPacket, 2 + offset - 1, packetData.Length);
    
    return fullPacket;
}
```

## Verification Criteria

### Protocol Compliance
- [ ] CONNECT packet follows MQTT 3.1.1 specification exactly
- [ ] Remaining length uses correct variable byte encoding
- [ ] Packet identifiers are unique and properly managed
- [ ] QoS levels implemented correctly (0, 1, 2)
- [ ] Keep-alive mechanism works (PINGREQ/PINGRESP)

### Network Robustness
- [ ] Handles connection failures gracefully
- [ ] Implements proper timeout handling
- [ ] Manages reconnection logic
- [ ] Prevents memory leaks (proper disposal)
- [ ] Thread-safe state management

### .NET Best Practices
- [ ] Uses async/await throughout (no blocking calls)
- [ ] Proper exception handling (SocketException, IOException)
- [ ] Resource disposal (using statements)
- [ ] Configurable timeouts and buffers
- [ ] Cancellation token support

## Threat Model Considerations

### Security Requirements [ASSUMED]
- Validate all incoming packet lengths (prevent buffer overflow)
- Sanitize topic strings (prevent MQTT injection attacks)
- Implement connection rate limiting
- Handle malformed packets without crashing
- Log security events (authentication failures, protocol violations)

### Input Validation
- Maximum packet size limits (prevent DoS)
- Topic string validation (MQTT specification compliance)
- Client ID length restrictions (1-23 characters)
- Username/password validation (if authentication used)

## Performance Targets

- Connection establishment: < 5 seconds [ASSUMED]
- Packet encoding: < 10ms per typical PUBLISH packet [ASSUMED]
- Network throughput: Support at least 100 messages/second [ASSUMED]
- Memory footprint: < 10MB for connection + buffers [ASSUMED]

## Dependencies and Integration

### Upstream Dependencies
- Phase 3 Settings: broker host, port, client credentials [VERIFIED: SettingsModel.cs]
- Phase 2 Design System: error messaging, status display [VERIFIED: DesignSystem.cs]

### Downstream Consumers
- Phase 5: PG-Number Automation will use MQTT client for publishing
- Phase 6: XML Publishing System will integrate with MQTT client

## Confidence Levels

- MQTT 3.1.1 packet structures: HIGH [VERIFIED: MQTT 3.1.1 Spec]
- .NET TcpClient/NetworkStream patterns: HIGH [VERIFIED: .NET Docs]
- QoS implementation details: MEDIUM [ASSUMED]
- Keep-alive timing specifics: HIGH [VERIFIED: MQTT 3.1.1 Spec]
- Security threat model: MEDIUM [ASSUMED]