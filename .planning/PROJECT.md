# Betriebsmittel Publisher

## Project Overview
Windows WinForms desktop application for publishing operating resource data via MQTT 3.1.1 protocol in offline industrial environments.

## Technical Stack
- **Platform**: Windows desktop application
- **Framework**: .NET 10, C# WinForms
- **Build Type**: Single-file executable (PublishSingleFile=true, SelfContained=true)
- **Dependency Policy**: Zero NuGet dependencies (custom NuGet.Config with `<clear/>`)
- **Runtime**: Completely offline-capable for both build and runtime
- **Protocol**: Custom MQTT 3.1.1 implementation using TcpClient/NetworkStream
- **Design System**: Dark mode with embedded fonts

## Application Architecture

### Main Components
1. **Settings Window** - Configuration interface for MQTT topics and station numbers
2. **Automation Window** - PG number automation with motor number input and 10-row table
3. **MQTT Client** - Custom protocol implementation
4. **Settings Persistence** - Local configuration storage

### Design System Specifications
- **Background**: #1a1d29 (dark navy)
- **Accent Color**: #ff5c5c (coral)
- **Design Style**: Flat, modern UI
- **Fonts**: JetBrains Mono and Inter (embedded as resources)

## Key Features

### Settings Window
- Configure publish topics for 4 operating resources
- Automatic station number parsing
- MQTT connection settings
- Local settings persistence

### PG-Number Automation Window
- Motor number input field
- 10-row data table with PG number automation
- XML generation and publishing
- Real-time status indicators

### MQTT Implementation
- Custom MQTT 3.1.1 protocol implementation
- TcpClient/NetworkStream-based communication
- No external protocol dependencies
- Offline connection management

## Technical Constraints

### Build Requirements
- Single-file .exe output
- Self-contained deployment
- No NuGet package dependencies
- Embedded resource management
- Offline build capability

### Runtime Requirements
- Windows desktop environment
- .NET 10 runtime
- Local file system access
- TCP/IP network connectivity

## Project Scope

### In Scope
- Settings configuration UI
- PG number automation logic
- MQTT connection management
- XML publishing functionality
- Local settings persistence
- Dark mode design system implementation
- Embedded font integration

### Out of Scope
- Online dependencies or cloud services
- Third-party MQTT libraries
- Database persistence (file-based only)
- Web-based interfaces
- Mobile platform support

## Success Criteria
- Single executable file deployment
- Zero external dependencies
- Offline build and runtime capability
- MQTT 3.1.1 protocol compliance
- Complete dark mode UI implementation
- Reliable settings persistence
- Stable XML publishing workflow