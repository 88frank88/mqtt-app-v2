# Research Report: Phase 1 - Project Initialization

## Executive Summary

This research document provides technical findings and recommendations for initializing a .NET 10 WinForms project with specific constraints for the Betriebsmittel Publisher application. The research covers single-file publishing, zero-dependency builds, font embedding, dark mode implementation, and custom MQTT protocol implementation.

## Research Findings

### 1. .NET 10 WinForms Project Structure and Configuration

#### Technical Findings:
- **Target Framework**: `net10.0-windows` - required for WinForms applications
- **Project SDK**: `Microsoft.NET.Sdk` (standard for .NET projects)
- **Output Type**: `WinExe` (Windows executable)
- **Use Windows Forms**: `true` property enables WinForms functionality
- **Supported Platforms**: Windows 10+ (as specified in requirements)

#### Recommended .csproj Structure:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  </PropertyGroup>
</Project>
```

#### Rationale:
- .NET 10 provides the latest features and performance improvements
- `net10.0-windows` ensures Windows-specific optimizations
- `WinExe` output type prevents console window from appearing
- `UseWindowsForms` enables WinForms support without explicit package references

#### Potential Risks:
- **Risk**: .NET 10 may not be available on all build machines
- **Mitigation**: Document .NET 10 SDK requirement in build documentation

---

### 2. Single-File Publishing Configuration for Offline Deployment

#### Technical Findings:
- **PublishSingleFile**: Bundles all application dependencies into a single executable
- **SelfContained**: Includes .NET runtime in the executable (no framework dependency)
- **RuntimeIdentifier**: Specifies target platform (win-x64 for 64-bit Windows)
- **IncludeNativeLibrariesForSelfExtract**: Optional for extracting native libraries
- **EnableCompressionInSingleFile**: Optional for reducing executable size
- **API Incompatibilities**: Some file-based APIs return empty strings or throw exceptions

#### Recommended Configuration:
```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

#### Build Command:
```bash
dotnet publish -c Release -r win-x64 --no-restore
```

#### API Incompatibility Workarounds:
- Use `AppContext.BaseDirectory` instead of `Assembly.Location`
- Use `Environment.ProcessPath` (or `Environment.GetCommandLineArgs()[0]`) instead of `Assembly.CodeBase`
- Use embedded resources instead of accessing files next to executable

#### Rationale:
- Single-file deployment simplifies distribution in offline environments
- Self-contained ensures no framework dependency on target machines
- Compression reduces file size while maintaining functionality
- Native library extraction ensures proper runtime behavior

#### Potential Risks:
- **Risk**: Large executable size (100MB+ due to embedded runtime)
- **Mitigation**: Enable compression, consider trimming if needed
- **Risk**: Startup time increase due to decompression
- **Mitigation**: Test and document expected startup time
- **Risk**: Some APIs may not work as expected in single-file mode
- **Mitigation**: Use recommended workarounds and test thoroughly

---

### 3. Custom NuGet.Config Setup for Zero-Dependency Builds

#### Technical Findings:
- **Configuration Scope**: Project-level `nuget.config` in project root
- **Clear Directive**: `<clear />` removes all inherited package sources
- **Package Sources Section**: Define zero or empty sources
- **Offline Capability**: No network calls to NuGet.org or other sources
- **Settings Inheritance**: Machine and user-level configs are ignored when `<clear />` is used

#### Recommended NuGet.Config:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
  </packageSources>
  <disabledPackageSources>
    <clear />
  </disabledPackageSources>
</configuration>
```

#### Build Process:
```bash
dotnet restore --no-cache --no-dependencies
dotnet build --no-restore
dotnet publish -c Release -r win-x64 --no-restore
```

#### Rationale:
- `<clear />` ensures no external package sources are accessed
- Empty configuration guarantees zero NuGet dependencies
- `--no-restore` flag prevents any package download attempts
- Project-level config ensures consistency across all development environments

#### Potential Risks:
- **Risk**: Build fails if project references any NuGet package
- **Mitigation**: Enforce zero-dependency policy through code review
- **Risk**: IDE may attempt to restore packages automatically
- **Mitigation**: Disable automatic package restore in IDE settings
- **Risk**: Some project templates include implicit package references
- **Mitigation**: Start from minimal project template and verify all references

---

### 4. Font Embedding Techniques in WinForms Applications

#### Technical Findings:
- **PrivateFontCollection**: System.Drawing.Text class for loading custom fonts
- **AddFontFile Method**: Loads font from file path (not suitable for single-file)
- **AddMemoryFont Method**: Loads font from memory pointer (suitable for embedded resources)
- **Embedded Resources**: Font files added as embedded resources in project
- **Resource Access**: Use `Assembly.GetManifestResourceStream()` to access embedded fonts
- **FontFamily**: Create FontFamily objects from loaded fonts
- **Font Construction**: Use Font objects with custom FontFamily

#### Recommended Implementation:
```csharp
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;

public class FontManager
{
    private static PrivateFontCollection _privateFontCollection;
    private static FontFamily _jetBrainsMonoFamily;
    private static FontFamily _interFamily;

    public static void Initialize()
    {
        _privateFontCollection = new PrivateFontCollection();

        // Load JetBrains Mono from embedded resource
        var jetBrainsMonoStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("YourNamespace.Resources.JetBrainsMono-Regular.ttf");
        var jetBrainsMonoData = new byte[jetBrainsMonoStream.Length];
        jetBrainsMonoStream.Read(jetBrainsMonoData, 0, jetBrainsMonoData.Length);
        var jetBrainsMonoPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(jetBrainsMonoData, 0);
        _privateFontCollection.AddMemoryFont(jetBrainsMonoPtr, jetBrainsMonoData.Length);

        // Load Inter from embedded resource
        var interStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("YourNamespace.Resources.Inter-Regular.ttf");
        var interData = new byte[interStream.Length];
        interStream.Read(interData, 0, interData.Length);
        var interPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(interData, 0);
        _privateFontCollection.AddMemoryFont(interPtr, interData.Length);

        // Get FontFamily objects
        _jetBrainsMonoFamily = _privateFontCollection.Families[0];
        _interFamily = _privateFontCollection.Families[1];
    }

    public static Font GetJetBrainsMonoFont(float size, FontStyle style)
    {
        return new Font(_jetBrainsMonoFamily, size, style);
    }

    public static Font GetInterFont(float size, FontStyle style)
    {
        return new Font(_interFamily, size, style);
    }
}
```

#### .csproj Configuration for Font Resources:
```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\JetBrainsMono-Regular.ttf" />
  <EmbeddedResource Include="Resources\JetBrainsMono-Bold.ttf" />
  <EmbeddedResource Include="Resources\Inter-Regular.ttf" />
  <EmbeddedResource Include="Resources\Inter-Bold.ttf" />
</ItemGroup>
```

#### Fallback Strategy:
```csharp
public static Font GetJetBrainsMonoFont(float size, FontStyle style)
{
    try
    {
        return new Font(_jetBrainsMonoFamily, size, style);
    }
    catch
    {
        return new Font("Consolas", size, style);
    }
}

public static Font GetInterFont(float size, FontStyle style)
{
    try
    {
        return new Font(_interFamily, size, style);
    }
    catch
    {
        return new Font("Segoe UI", size, style);
    }
}
```

#### Rationale:
- Memory font loading works with single-file deployment
- Embedded resources ensure offline capability
- PrivateFontCollection isolates fonts from system fonts
- Fallback fonts ensure application continues working if embedding fails

#### Potential Risks:
- **Risk**: Font file size increases executable size
- **Mitigation**: Use only necessary font weights/styles
- **Risk**: Font licensing compliance
- **Mitigation**: Include license files and verify redistribution rights
- **Risk**: Memory management with pinned arrays
- **Mitigation**: Keep PrivateFontCollection instance alive for application lifetime

---

### 5. Dark Mode WinForms Design Patterns

#### Technical Findings:
- **Color System**: Use predefined color constants for consistency
- **Control Styling**: Set `FlatStyle = Flat` on controls for modern appearance
- **BackColor/ForeColor**: Override default colors on all controls
- **Visual Styles**: Application-wide visual style configuration
- **Professional Colors**: System colors override for consistent theming
- **Form Rendering**: Override `OnPaintBackground` for custom backgrounds

#### Recommended Color Constants:
```csharp
public static class DesignSystem
{
    public static class Colors
    {
        // Primary Colors
        public static readonly Color Background = Color.FromArgb(0x1a, 0x1d, 0x29); // #1a1d29
        public static readonly Color Accent = Color.FromArgb(0xff, 0x5c, 0x5c);     // #ff5c5c
        public static readonly Color Secondary = Color.FromArgb(0x5b, 0x64, 0x78);  // #5b6478

        // Text Colors
        public static readonly Color TextPrimary = Color.FromArgb(0xff, 0xff, 0xff);
        public static readonly Color TextSecondary = Color.FromArgb(0xcc, 0xcc, 0xcc);
        public static readonly Color TextDisabled = Color.FromArgb(0x88, 0x88, 0x88);

        // Control Colors
        public static readonly Color ControlBackground = Color.FromArgb(0x22, 0x25, 0x33);
        public static readonly Color ControlBorder = Color.FromArgb(0x3a, 0x3e, 0x50);
        public static readonly Color ControlHover = Color.FromArgb(0x2a, 0x2e, 0x3d);

        // Status Colors
        public static readonly Color Success = Color.FromArgb(0x4a, 0x7c, 0x59);
        public static readonly Color Warning = Color.FromArgb(0xc9, 0x8b, 0x35);
        public static readonly Color Error = Color.FromArgb(0xd9, 0x5c, 0x5c);
    }
}
```

#### Base Form Class:
```csharp
public class DarkModeForm : Form
{
    public DarkModeForm()
    {
        ApplyDarkModeTheme();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using (var brush = new SolidBrush(DesignSystem.Colors.Background))
        {
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }

    private void ApplyDarkModeTheme()
    {
        BackColor = DesignSystem.Colors.Background;
        ForeColor = DesignSystem.Colors.TextPrimary;

        // Apply theme to all controls
        ApplyThemeToControls(this.Controls);
    }

    private void ApplyThemeToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            ApplyThemeToControl(control);
            ApplyThemeToControls(control.Controls);
        }
    }

    private void ApplyThemeToControl(Control control)
    {
        control.BackColor = DesignSystem.Colors.ControlBackground;
        control.ForeColor = DesignSystem.Colors.TextPrimary;

        if (control is Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;
            button.BackColor = DesignSystem.Colors.ControlBackground;
        }
        else if (control is TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = DesignSystem.Colors.Background;
            textBox.ForeColor = DesignSystem.Colors.TextPrimary;
        }
        else if (control is ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = DesignSystem.Colors.Background;
            comboBox.ForeColor = DesignSystem.Colors.TextPrimary;
        }
        // Add more control types as needed
    }
}
```

#### Application-wide Theme Application:
```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Apply dark mode system-wide colors
        ApplySystemWideDarkMode();

        Application.Run(new MainForm());
    }

    private static void ApplySystemWideDarkMode()
    {
        // Override system colors
        var colors = typeof(SystemColors).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var color in colors)
        {
            // Map system colors to dark mode equivalents
            if (color.Name == "Control")
                color.SetValue(null, DesignSystem.Colors.ControlBackground);
            else if (color.Name == "ControlText")
                color.SetValue(null, DesignSystem.Colors.TextPrimary);
            // Add more mappings as needed
        }
    }
}
```

#### Rationale:
- Consistent color system ensures visual coherence
- Flat style eliminates default 3D appearance
- Base form class reduces repetitive styling code
- System-wide color override ensures consistent appearance
- Custom background painting handles edge cases

#### Potential Risks:
- **Risk**: Some third-party controls may not respect custom styling
- **Mitigation**: Test all control types and provide custom styling where needed
- **Risk**: Performance impact from extensive control iteration
- **Mitigation**: Apply styling during form load, not runtime
- **Risk**: Theme inconsistencies across different Windows versions
- **Mitigation**: Test on target Windows versions and adjust as needed

---

### 6. MQTT 3.1.1 Protocol Implementation Using TcpClient/NetworkStream

#### Technical Findings:
- **TcpClient**: Provides TCP connection functionality
- **NetworkStream**: Stream-based communication over TCP
- **Protocol Version**: MQTT 3.1.1 (OASIS Standard)
- **Port**: Default 1883 (non-encrypted)
- **Packet Structure**: Fixed header + variable header + payload
- **Connection Flow**: CONNECT → CONNACK → PUBLISH operations
- **Quality of Service**: At most once (QoS 0), at least once (QoS 1), exactly once (QoS 2)

#### Recommended Implementation Architecture:

##### Connection Manager:
```csharp
public class MqttConnectionManager : IDisposable
{
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private bool _isConnected;

    public async Task<bool> ConnectAsync(string host, int port, int timeoutMs = 5000)
    {
        try
        {
            _tcpClient = new TcpClient();
            var connectTask = _tcpClient.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeoutMs);

            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                _tcpClient.Close();
                return false;
            }

            _networkStream = _tcpClient.GetStream();
            _isConnected = true;
            return true;
        }
        catch
        {
            _isConnected = false;
            return false;
        }
    }

    public void Disconnect()
    {
        _isConnected = false;
        _networkStream?.Close();
        _tcpClient?.Close();
    }

    public void Dispose()
    {
        Disconnect();
    }
}
```

##### MQTT Packet Classes:
```csharp
public abstract class MqttPacket
{
    public abstract byte[] Encode();
    public static MqttPacket Decode(byte[] data)
    {
        // Implementation for packet decoding
        var fixedHeader = new MqttFixedHeader(data[0]);
        // Parse variable header and payload based on packet type
        // Return appropriate packet type
        return null;
    }
}

public class MqttFixedHeader
{
    public MqttPacketType PacketType { get; }
    public bool DupFlag { get; }
    public MqttQosLevel QosLevel { get; }
    public bool Retain { get; }
    public int RemainingLength { get; }

    public MqttFixedHeader(byte headerByte)
    {
        PacketType = (MqttPacketType)((headerByte >> 4) & 0x0F);
        DupFlag = (headerByte & 0x08) != 0;
        QosLevel = (MqttQosLevel)((headerByte >> 1) & 0x03);
        Retain = (headerByte & 0x01) != 0;
    }
}

public class MqttConnectPacket : MqttPacket
{
    public string ProtocolName { get; } = "MQTT";
    public byte ProtocolLevel { get; } = 0x04; // MQTT 3.1.1
    public MqttConnectFlags ConnectFlags { get; set; }
    public ushort KeepAlive { get; set; } = 60;
    public string ClientId { get; set; }

    public override byte[] Encode()
    {
        // Encode CONNECT packet according to MQTT 3.1.1 spec
        var variableHeader = EncodeVariableHeader();
        var payload = EncodePayload();
        return BuildPacket(variableHeader, payload);
    }
}

public class MqttPublishPacket : MqttPacket
{
    public string Topic { get; set; }
    public byte[] Payload { get; set; }
    public ushort PacketId { get; set; }
    public MqttQosLevel QosLevel { get; set; } = MqttQosLevel.AtMostOnce;
    public bool Retain { get; set; }

    public override byte[] Encode()
    {
        // Encode PUBLISH packet according to MQTT 3.1.1 spec
        var variableHeader = EncodeVariableHeader();
        var payload = Payload;
        return BuildPacket(variableHeader, payload);
    }
}
```

##### MQTT Client Implementation:
```csharp
public class MqttClient : IDisposable
{
    private readonly MqttConnectionManager _connectionManager;
    private string _clientId;

    public MqttClient()
    {
        _connectionManager = new MqttConnectionManager();
        _clientId = GenerateClientId();
    }

    public async Task<bool> ConnectAsync(string host, int port)
    {
        if (!await _connectionManager.ConnectAsync(host, port))
            return false;

        var connectPacket = new MqttConnectPacket
        {
            ClientId = _clientId,
            ConnectFlags = new MqttConnectFlags
            {
                CleanSession = true,
                UsernameFlag = false,
                PasswordFlag = false
            }
        };

        await SendPacketAsync(connectPacket);

        var connAck = await ReadPacketAsync() as MqttConnAckPacket;
        return connAck != null && connAck.ReturnCode == MqttConnectReturnCode.Accepted;
    }

    public async Task PublishAsync(string topic, string payload)
    {
        var publishPacket = new MqttPublishPacket
        {
            Topic = topic,
            Payload = Encoding.UTF8.GetBytes(payload),
            QosLevel = MqttQosLevel.AtMostOnce
        };

        await SendPacketAsync(publishPacket);
    }

    private async Task SendPacketAsync(MqttPacket packet)
    {
        var data = packet.Encode();
        await _connectionManager.NetworkStream.WriteAsync(data, 0, data.Length);
    }

    private async Task<MqttPacket> ReadPacketAsync()
    {
        // Read fixed header
        var fixedHeaderByte = await ReadByteAsync();
        var remainingLength = await DecodeRemainingLengthAsync();

        // Read packet data
        var packetData = new byte[remainingLength];
        await _connectionManager.NetworkStream.ReadAsync(packetData, 0, remainingLength);

        return MqttPacket.Decode(packetData);
    }

    public void Dispose()
    {
        _connectionManager?.Dispose();
    }
}
```

#### MQTT 3.1.1 Protocol Key Points:
- **Fixed Header**: 1-5 bytes, contains packet type and flags
- **Remaining Length**: Variable-length encoding (1-4 bytes)
- **CONNECT Packet**: Protocol name, level, flags, keep-alive, client ID
- **CONNACK Packet**: Session present flag, return code
- **PUBLISH Packet**: Topic name, packet ID (for QoS 1/2), payload
- **QoS Levels**: 0 (fire and forget), 1 (at least once), 2 (exactly once)
- **Keep-Alive**: Client sends PINGREQ if no other packets sent within keep-alive period

#### Rationale:
- TcpClient/NetworkStream provide low-level TCP control needed for MQTT
- Custom implementation ensures zero dependencies and offline capability
- Packet-based architecture aligns with MQTT protocol design
- Async/await pattern ensures responsive UI
- Connection manager handles network robustness

#### Potential Risks:
- **Risk**: Protocol implementation complexity and potential bugs
- **Mitigation**: Implement core CONNECT/PUBLISH first, test thoroughly, expand incrementally
- **Risk**: Network reliability and connection management
- **Mitigation**: Implement proper error handling, timeouts, and reconnection logic
- **Risk**: MQTT protocol compliance
- **Mitigation**: Follow OASIS MQTT 3.1.1 specification closely, test with MQTT broker
- **Risk**: Performance with large payloads
- **Mitigation**: Implement streaming for large payloads, buffer management

---

## Code Examples

### Complete .csproj File:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <ItemGroup>
    <EmbeddedResource Include="Resources\JetBrainsMono-Regular.ttf" />
    <EmbeddedResource Include="Resources\JetBrainsMono-Bold.ttf" />
    <EmbeddedResource Include="Resources\Inter-Regular.ttf" />
    <EmbeddedResource Include="Resources\Inter-Bold.ttf" />
  </ItemGroup>
</Project>
```

### Complete NuGet.Config File:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
  </packageSources>
  <disabledPackageSources>
    <clear />
  </disabledPackageSources>
</configuration>
```

### Build Script (BUILD.bat):
```batch
@echo off
echo Building Betriebsmittel Publisher...
dotnet restore --no-cache --no-dependencies
if errorlevel 1 (
    echo Restore failed!
    exit /b 1
)

dotnet build -c Release --no-restore
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

dotnet publish -c Release -r win-x64 --no-restore
if errorlevel 1 (
    echo Publish failed!
    exit /b 1
)

echo Build completed successfully!
echo Output: bin\Release\net10.0-windows\win-x64\publish\
```

---

## References to Official Documentation

### .NET and WinForms:
- [.NET Single File Deployment Overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
- [.NET Application Publishing](https://learn.microsoft.com/en-us/dotnet/core/deploying/)
- [Windows Forms Overview](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/)
- [PrivateFontCollection Class](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.text.privatefontcollection?view=net-11.0-pp)

### NuGet Configuration:
- [NuGet.Config File Reference](https://learn.microsoft.com/en-us/nuget/reference/nuget-config-file)
- [Configuring NuGet Behavior](https://learn.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior)

### Network Programming:
- [TcpClient Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-10.0)
- [NetworkStream Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=net-10.0)

### MQTT Protocol:
- [MQTT 3.1.1 Specification (OASIS)](http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html)
- [MQTT.org Official Specification](https://mqtt.org/mqtt-specification/)

---

## Risk Assessment Summary

| Risk Area | Risk Level | Mitigation Strategy |
|-----------|-----------|---------------------|
| .NET 10 Availability | Low | Document SDK requirements |
| Single File Size | Medium | Enable compression, test performance |
| API Incompatibilities | Medium | Use recommended workarounds |
| Zero Dependency Enforcement | Low | Code review, automated checks |
| Font Embedding Complexity | Low | Use proven pattern, fallback fonts |
| MQTT Implementation | High | Incremental development, thorough testing |
| Dark Mode Consistency | Low | Base class pattern, systematic testing |

---

## Recommendations for Phase 1 Implementation

1. **Start with minimal project structure**: Create basic .csproj with required properties
2. **Implement single-file publishing first**: Verify build produces single .exe
3. **Set up NuGet.Config early**: Ensure zero dependencies from the start
4. **Create base dark mode infrastructure**: Implement DesignSystem and base form class
5. **Prepare font embedding framework**: Set up resource structure and FontManager
6. **Begin MQTT connection layer**: Start with TcpClient wrapper before protocol implementation
7. **Test build process offline**: Verify no internet connection needed for build/runtime
8. **Document build and deployment process**: Create comprehensive build documentation

---

## Next Steps

1. Create project directory structure following .NET conventions
2. Implement initial .csproj file with all required properties
3. Set up NuGet.Config in project root
4. Create base WinForms application with dark mode styling
5. Implement font loading and management system
6. Begin MQTT connection manager implementation
7. Create initial build script and test offline build capability
8. Verify single-file deployment works as expected

---

**RESEARCH COMPLETE**