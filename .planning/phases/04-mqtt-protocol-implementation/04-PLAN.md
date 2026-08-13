---
wave: 1
depends_on: ["03-settings-window-development"]
files_modified: ["Services/MqttClient.cs", "Services/MqttPacketBuilder.cs", "Services/MqttPacketParser.cs", "Services/ConnectionManager.cs", "Models/MqttMessage.cs"]
autonomous: true
requirements: ["FR-003", "NFR-002"]
---

# Phase 4: MQTT Protocol Implementation - Execution Plan

## 1. Plan Overview

### Summary
Implement a custom MQTT 3.1.1 client using .NET TcpClient and NetworkStream to enable publishing messages to configured topics. This phase builds the networking foundation for the automation system, requiring zero external dependencies while ensuring protocol compliance and robust connection management.

### Key Deliverables
- `Models/MqttMessage.cs`: MQTT message data structures
- `Services/MqttPacketBuilder.cs`: Packet construction utilities
- `Services/MqttPacketParser.cs`: Packet parsing utilities
- `Services/ConnectionManager.cs`: TCP connection handling
- `Services/MqttClient.cs`: Main MQTT client with state management

### Success Criteria
- [ ] MQTT CONNECT packet successfully establishes connection
- [ ] PUBLISH packets send messages to broker correctly
- [ ] Connection state machine manages transitions properly
- [ ] Keep-alive mechanism prevents timeout disconnections
- [ ] Network errors handled gracefully with recovery
- [ ] Protocol compliance verified against MQTT 3.1.1 spec
- [ ] Integration with Phase 3 settings for connection parameters

---

## 2. Task Breakdown

### Task 1: Create MQTT Message Models
**File**: `Models/MqttMessage.cs`
**Dependencies**: None
**Description**: Define data structures for MQTT messages and packet types.
**Implementation**:
- Create enums for `MqttPacketType` (CONNECT=1, PUBLISH=3, SUBSCRIBE=8, PINGREQ=12, DISCONNECT=14)
- Create `MqttQoS` enum (AtMostOnce=0, AtLeastOnce=1, ExactlyOnce=2)
- Create `MqttMessage` class with Topic, Payload, QoS, PacketId properties
- Create `MqttConnectMessage` class with ClientId, Username, Password, KeepAlive
- Create `MqttPublishMessage` class with Topic, Payload, QoS, Retain, Dup
**Verification**: All message types compile, enums match MQTT 3.1.1 specification values

---

### Task 2: Implement MQTT Packet Builder
**File**: `Services/MqttPacketBuilder.cs`
**Dependencies**: Task 1
**Description**: Build MQTT protocol packets according to 3.1.1 specification.
**Implementation**:
- `BuildFixedHeader(packetType, flags, remainingLength)` - constructs 2-byte header
- `EncodeRemainingLength(stream, length)` - variable byte encoding algorithm
- `BuildConnectPacket(connectMessage)` - CONNECT packet with protocol handshake
- `BuildPublishPacket(publishMessage)` - PUBLISH packet with topic and payload
- `BuildPingReqPacket()` - PINGREQ packet for keep-alive
- `BuildDisconnectPacket()` - DISCONNECT packet for graceful shutdown
- `WriteString(stream, text)` - UTF-8 string with 2-byte length prefix
- `WriteUInt16(stream, value)` - big-endian 16-bit integer encoding
**Verification**: Packet bytes match MQTT 3.1.1 specification format, remaining length encoding works for large packets

---

### Task 3: Implement MQTT Packet Parser
**File**: `Services/MqttPacketParser.cs`
**Dependencies**: Task 1
**Description**: Parse incoming MQTT packets from network stream.
**Implementation**:
- `ParsePacketType(fixedHeader)` - extract message type and flags from first byte
- `ParseRemainingLength(stream)` - variable byte decoding for packet length
- `ParseConnectAck(stream)` - parse CONNACK response (session present, return code)
- `ParsePublishAck(stream)` - parse PUBACK for QoS 1 confirmation
- `ParsePingResponse(stream)` - parse PINGRESP from broker
- `ReadExactBytesAsync(stream, count, cancellationToken)` - read exact byte count
- `ReadString(stream)` - read UTF-8 string with 2-byte length prefix
- `ReadUInt16(stream)` - read big-endian 16-bit integer
**Verification**: Parser correctly extracts fields from valid packets, handles malformed packets gracefully

---

### Task 4: Create TCP Connection Manager
**File**: `Services/ConnectionManager.cs`
**Dependencies**: None
**Description**: Manage TCP connections with TcpClient and NetworkStream.
**Implementation**:
- `ConnectAsync(host, port, cancellationToken)` - establish TCP connection
- `DisconnectAsync()` - close connection gracefully
- `SendPacketAsync(packet, cancellationToken)` - write packet to network stream
- `ReceivePacketAsync(cancellationToken)` - read complete packet from stream
- `IsConnected` property - connection state tracking
- `Dispose()` pattern - cleanup network resources
- Error handling for SocketException, IOException, OperationCanceledException
**Verification**: Connection establishment works, data transfer succeeds, disconnection is clean

---

### Task 5: Implement MQTT Client Core
**File**: `Services/MqttClient.cs`
**Dependencies**: Task 1, Task 2, Task 3, Task 4
**Description**: Main MQTT client with connection lifecycle and message publishing.
**Implementation**:
- `ConnectAsync(settings, cancellationToken)` - connect to broker with settings
- `DisconnectAsync()` - disconnect from broker
- `PublishAsync(topic, payload, qos, cancellationToken)` - publish message
- Background message receiver loop (Task.Run)
- Keep-alive timer for PINGREQ/PINGRESP
- Connection state machine (Disconnected → Connecting → Connected → Disconnecting)
- Event system for connection state changes
- Packet ID management (incrementing 16-bit counter)
- Error handling and automatic reconnection logic
**Verification**: Client connects, publishes messages, maintains connection, handles disconnections

---

### Task 6: Add MQTT Settings Integration
**File**: `Services/MqttClient.cs`, `Models/SettingsModel.cs`
**Dependencies**: Task 5
**Description**: Extend SettingsModel with MQTT connection parameters.
**Implementation**:
- Add broker host, port, client ID, username, password to SettingsModel
- Update SettingsPersistence to save/load MQTT settings
- Update SettingsWindow UI to include connection configuration
- Update MqttClient.ConnectAsync to use SettingsModel parameters
**Verification**: MQTT settings persist and load correctly, client uses configured parameters

---

### Task 7: Implement Error Handling and Recovery
**File**: `Services/MqttClient.cs`, `Services/ConnectionManager.cs`
**Dependencies**: Task 5
**Description**: Add robust error handling and recovery mechanisms.
**Implementation**:
- Connection timeout handling
- Network failure detection and reconnection logic
- Malformed packet handling
- Buffer overflow prevention (packet size limits)
- Memory leak prevention (proper disposal, buffer pooling)
- Thread safety for connection state changes
- Event-based error reporting
**Verification**: Client recovers from network failures, no memory leaks, thread-safe operations

---

## 3. Dependency Graph

```
Task 1: MqttMessage Models (no dependencies)
  └─> Task 2: MqttPacketBuilder (depends on Task 1)
  └─> Task 3: MqttPacketParser (depends on Task 1)

Task 4: ConnectionManager (no dependencies)
  └─> Task 5: MqttClient Core (depends on Task 1, Task 2, Task 3, Task 4)
       ├─> Task 6: MQTT Settings Integration (depends on Task 5)
       └─> Task 7: Error Handling and Recovery (depends on Task 5)
```

**Parallel Execution Opportunities:**
- Tasks 1, 4 can run in parallel (both independent)
- Tasks 2, 3 can run in parallel after Task 1 completes
- Task 5 can start after Task 4 completes (needs connection manager)
- Task 6, 7 depend on Task 5 completion

---

## 4. Implementation Approach

### MQTT Protocol Implementation Strategy
**Protocol Version**: MQTT 3.1.1 exactly as specified in OASIS standard
**Packet Structure**: Fixed header (2 bytes min) + Variable header + Payload
**Encoding**: Binary encoding with big-endian integers, UTF-8 strings
**QoS Support**: QoS 0 and 1 initially (QoS 2 can be added later if needed)

### Connection Management Architecture
**Lifecycle**: Disconnected → Connecting → Connected → Disconnecting → Disconnected
**Async Pattern**: Use async/await throughout (ReadAsync, WriteAsync)
**Timeout Configuration**: 30-second connection timeout, 60-second keep-alive
**Reconnection**: Exponential backoff (1s, 2s, 4s, 8s, 16s max)

### Network Stream Handling
**Reading**: Background Task.Run loop reading packets continuously
**Writing**: Synchronized writes (single writer at a time)
**Buffer Management**: Use ArrayPool<byte>.Shared for efficient buffer reuse
**Cancellation**: Support CancellationToken for graceful shutdown

### Integration with Phase 3 Settings
**Settings Location**: Extend SettingsModel with MQTT connection fields
**UI Updates**: Add connection configuration section to SettingsWindow
**Persistence**: Save/load broker settings alongside topic configuration
**Default Values**: localhost:1883, anonymous client for testing

---

## 5. Risk Mitigation

### Risk 1: MQTT Protocol Compliance Issues
**Mitigation**: 
- Follow MQTT 3.1.1 specification exactly for packet formats
- Implement variable byte encoding precisely for remaining length field
- Test against public MQTT brokers (test.mosquitto.org, hivemq.com)
- Include protocol compliance verification in testing

### Risk 2: Network Connection Instability
**Mitigation**:
- Implement robust error handling for SocketException, IOException
- Add connection timeout and retry logic with exponential backoff
- Support manual reconnection triggers via API
- Log all network events for troubleshooting

### Risk 3: Memory Leaks and Resource Management
**Mitigation**:
- Use using statements for TcpClient and NetworkStream
- Implement proper disposal patterns (IDisposable)
- Use ArrayPool<byte>.Shared for buffer allocation
- Monitor memory usage during extended operation

### Risk 4: Thread Safety and Concurrency Issues
**Mitigation**:
- Single reader/writer pattern for NetworkStream
- Lock connection state transitions
- Use ConcurrentQueue for message passing between threads
- Avoid shared mutable state between threads

### Risk 5: Performance Bottlenecks
**Mitigation**:
- Use async/await to avoid blocking threads
- Implement buffer pooling for frequent allocations
- Profile packet encoding/decoding performance
- Set appropriate buffer sizes (4KB default, 64KB max)

### Risk 6: Security Vulnerabilities
**Mitigation**:
- Validate all incoming packet lengths
- Sanitize topic strings to prevent injection attacks
- Implement connection rate limiting
- Handle malformed packets without crashing
- Consider SSL/TLS support for future phases

---

## 6. Testing Strategy

### Unit Testing (Manual, No Framework)
- Test packet encoding/decoding with known good values
- Test remaining length encoding edge cases (0, 127, 128, 16383, 16384)
- Test connection manager with local TCP echo server
- Test packet parser with malformed packets

### Integration Testing
- Connect to public MQTT broker (test.mosquitto.org:1883)
- Publish messages to test topics and verify delivery
- Test connection timeout and reconnection logic
- Test keep-alive mechanism with broker disconnect simulation

### Protocol Compliance Testing
- Verify CONNECT packet format against MQTT 3.1.1 spec
- Verify PUBLISH packet structure matches specification
- Test QoS 0 and QoS 1 message delivery
- Verify keep-alive timing (PINGREQ/PINGRESP exchange)

### Network Resilience Testing
- Simulate network failures during connection
- Test with slow/unreliable network conditions
- Verify client behavior during broker restart
- Test memory usage over extended operation periods

---

## 7. Estimated Timeline
**Task 1**: 1 hour
**Task 2**: 2 hours
**Task 3**: 2 hours
**Task 4**: 1.5 hours
**Task 5**: 3 hours
**Task 6**: 1 hour
**Task 7**: 2 hours
**Total**: ~12.5 hours (2-3 days)

---

## 8. Exit Criteria
Phase 4 is complete when:
- [ ] All MQTT message models and packet utilities compile without errors
- [ ] CONNECT packet successfully establishes connection to MQTT broker
- [ ] PUBLISH packets send messages to configured topics correctly
- [ ] Connection state machine transitions properly through all states
- [ ] Keep-alive mechanism prevents timeout disconnections
- [ ] Network errors are handled gracefully with automatic reconnection
- [ ] No memory leaks during extended operation (verified with monitoring)
- [ ] MQTT client integrates with Phase 3 settings for connection parameters
- [ ] Protocol compliance verified against MQTT 3.1.1 specification
- [ ] All code follows .NET async/await best practices
- [ ] Thread-safe operations verified under concurrent load

---

## 9. Threat Model

### Security Considerations

**Input Validation:**
- Validate all packet lengths (max 268,435,455 bytes per MQTT spec)
- Sanitize topic strings (validate against MQTT topic rules: no wildcards, no null bytes)
- Validate client ID length (1-23 characters per MQTT spec)
- Reject malformed packets without crashing

**Resource Protection:**
- Enforce maximum packet size limits (prevent DoS via large packets)
- Implement connection rate limiting (prevent connection flood attacks)
- Use buffer pooling to prevent unbounded memory growth
- Timeout all network operations (prevent hanging)

**Protocol Security:**
- Validate packet flags against message type (prevent malformed packets)
- Implement proper QoS handling (prevent message loss/duplication)
- Handle malformed UTF-8 strings gracefully
- Log security events (authentication failures, protocol violations)

**Future Security Enhancements:**
- SSL/TLS support for encrypted connections
- Authentication mechanisms (username/password, client certificates)
- Access control for topic publish/subscribe permissions
- Message payload encryption for sensitive data

---

## 10. Artifacts This Phase Produces

**New Classes:**
- `BetriebsmittelPublisher.Models.MqttMessage`
- `BetriebsmittelPublisher.Models.MqttConnectMessage` 
- `BetriebsmittelPublisher.Models.MqttPublishMessage`
- `BetriebsmittelPublisher.Services.MqttPacketBuilder`
- `BetriebsmittelPublisher.Services.MqttPacketParser`
- `BetriebsmittelPublisher.Services.ConnectionManager`
- `BetriebsmittelPublisher.Services.MqttClient`

**New Enums:**
- `BetriebsmittelPublisher.Models.MqttPacketType`
- `BetriebsmittelPublisher.Models.MqttQoS`

**Modified Classes:**
- `BetriebsmittelPublisher.Models.SettingsModel` (add MQTT connection fields)
- `BetriebsmittelPublisher.Services.SettingsPersistence` (persist MQTT settings)
- `BetriebsmittelPublisher.UI.SettingsWindow` (MQTT configuration UI)

**New Configuration Fields:**
- `SettingsModel.MqttBrokerHost`
- `SettingsModel.MqttBrokerPort` 
- `SettingsModel.MqttClientId`
- `SettingsModel.MqttUsername`
- `SettingsModel.MqttPassword`