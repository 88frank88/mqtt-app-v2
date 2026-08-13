 Betriebsmittel Publisher v1.0.0

## Version Information
- **Version**: 1.0.0
- **Product**: Betriebsmittel Publisher
- **Company**: Industrial Automation
- **Copyright**: © 2026 Industrial Automation

## Features
- Dark mode UI with #1a1d29 background and #ff5c5c accent
- MQTT 3.1.1 client with zero dependencies
- PG number automation with 10-row data table
- XML generation and publishing
- Settings persistence
- Single-file deployment

## Technical Details
- **Framework**: .NET 10 WinForms
- **Target**: net10.0-windows
- **Deployment**: Single executable with embedded resources
- **Dependencies**: Zero NuGet packages

## Build
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```