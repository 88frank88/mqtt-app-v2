# Betriebsmittel Publisher Requirements

## Functional Requirements

### FR-001: Settings Configuration
- Application shall provide a settings window for configuration
- Users shall configure publish topics for 4 operating resources
- Application shall automatically parse station numbers
- Settings shall persist to local storage
- Settings shall be loadable on application startup

### FR-002: PG-Number Automation
- Application shall provide an automation window for PG number management
- Users shall input motor numbers via dedicated field
- Application shall display 10-row data table
- Table shall support PG number automation
- Application shall generate XML for published data

### FR-003: MQTT Publishing
- Application shall implement MQTT 3.1.1 protocol
- Application shall establish TCP connections using TcpClient
- Application shall handle NetworkStream communication
- Application shall publish messages to configured topics
- Application shall maintain connection state

### FR-004: XML Generation
- Application shall generate XML from PG number data
- XML shall follow structured format for industrial systems
- XML shall be publishable via MQTT
- Application shall validate XML structure before publishing

## Non-Functional Requirements

### NFR-001: Single File Deployment
- Application shall compile to single .exe file
- Build shall use PublishSingleFile=true
- Build shall use SelfContained=true
- Application shall not require external DLL files

### NFR-002: Zero Dependencies
- Application shall have zero NuGet package dependencies
- Build shall use custom NuGet.Config with `<clear/>`
- All functionality shall be implemented in-house
- Protocol implementation shall be custom

### NFR-003: Offline Capability
- Application shall build without internet connection
- Application shall run without internet connection
- All resources shall be embedded in executable
- Fonts shall be embedded as resources

### NFR-004: Design System
- Application shall implement dark mode theme
- Primary background shall be #1a1d29
- Accent color shall be #ff5c5c
- Design shall be flat and modern
- JetBrains Mono and Inter fonts shall be embedded

### NFR-005: Performance
- Application shall respond to user input within 100ms
- MQTT connection shall establish within 5 seconds
- XML generation shall complete within 500ms
- Application shall consume < 50MB memory

## Technical Requirements

### TR-001: .NET Framework
- Application shall target .NET 10
- Application shall use C# language
- Application shall use WinForms for UI
- Application shall support Windows 10+

### TR-002: Protocol Implementation
- MQTT implementation shall use TcpClient
- MQTT implementation shall use NetworkStream
- Implementation shall support MQTT 3.1.1 specification
- Connection handling shall be robust

### TR-003: Resource Management
- Fonts shall be embedded as assembly resources
- Images shall be embedded as resources
- Configuration files shall use local storage
- No external resource files shall be required

## Security Requirements

### SR-001: Data Protection
- Settings shall be stored locally only
- No data shall be transmitted to external servers
- Connection credentials shall be stored securely
- Application shall not collect telemetry

### SR-002: Input Validation
- All user inputs shall be validated
- Motor numbers shall be numeric only
- Topic strings shall follow MQTT conventions
- XML shall be validated before generation

## Compliance Requirements

### CR-001: Standards
- MQTT 3.1.1 protocol compliance
- XML formatting standards compliance
- Windows application guidelines compliance
- .NET coding standards compliance

## User Experience Requirements

### UXR-001: Interface Design
- Windows shall be resizable and responsive
- Controls shall follow consistent spacing
- Dark mode shall be consistent across all windows
- Text shall be readable in dark mode
- Navigation shall be intuitive

### UXR-002: Error Handling
- Application shall display user-friendly error messages
- Connection failures shall be clearly indicated
- Invalid inputs shall be highlighted
- Application shall not crash on user errors

## Quality Requirements

### QR-001: Code Quality
- Code shall follow C# naming conventions
- Methods shall be < 50 lines where possible
- Classes shall have single responsibility
- Code shall be commented where complex

### QR-002: Testing
- Core functionality shall be unit testable
- MQTT implementation shall be testable
- Settings persistence shall be verifiable
- XML generation shall be validated