---
wave: 1
depends_on: ["04-mqtt-protocol-implementation"]
files_modified: ["UI/AutomationWindow.cs", "Models/PgAutomationModel.cs", "Services/PgNumberGenerator.cs", "Services/XmlConverter.cs"]
autonomous: true
requirements: ["FR-002", "FR-003", "FR-004"]
---

# Phase 5: PG-Number Automation Window Development - Execution Plan

## 1. Plan Overview

### Summary
Build an automation window for PG number management with motor number input, 10-row data table, automatic PG number generation, XML conversion, and MQTT publishing integration. This phase completes the core user interface for the Betriebsmittel Publisher.

### Key Deliverables
- `UI/AutomationWindow.cs`: Automation form with motor input, table, and controls
- `Models/PgAutomationModel.cs`: Data model for automation state
- `Services/PgNumberGenerator.cs`: PG number automation logic
- `Services/XmlConverter.cs`: XML generation from table data
- Integration with Phase 4 MQTT client for publishing

### Success Criteria
- [ ] Automation window displays correctly with dark mode theme
- [ ] Motor number input accepts numeric values
- [ ] 10-row data table renders with proper columns
- [ ] PG numbers auto-generate correctly for each row
- [ ] XML output matches industrial system specification
- [ ] MQTT publishing integrates with configured topics
- [ ] Table-to-XML conversion handles validation

---

## 2. Task Breakdown

### Task 1: Create Automation Window Form Structure
**File**: `UI/AutomationWindow.cs`
**Dependencies**: Phase 2 (BaseForm, DesignSystem)
**Description**: Build the main automation window form with layout and controls.
**Implementation**:
- Inherit from BaseForm for consistent dark mode styling
- Layout: Header with title, motor number input section, 10-row table, action buttons
- Use TableLayoutPanel for structured layout with DesignSystem spacing
- Action buttons: Generate PG Numbers, Publish XML, Clear Table
- Apply DesignSystem.CreateTextBox for input styling
- Apply DesignSystem color constants for theming
**Verification**: Window renders with correct layout, controls styled per design system

---

### Task 2: Create PG Automation Data Model
**File**: `Models/PgAutomationModel.cs`
**Dependencies**: None
**Description**: Define data structures for automation state and table rows.
**Implementation**:
- `PgTableRow` class: MotorNumber, PgNumber, Status, LastUpdated
- `PgAutomationModel` class: CurrentMotorNumber, TableRows[], LastGenerated
- `PgNumberGeneratorConfig` class: Prefix, StartNumber, Increment, MaxRows
- Validation logic for motor number input
- Row status tracking (pending, generated, published, error)
**Verification**: Model compiles, validation works, status tracking functions

---

### Task 3: Implement PG Number Generator Service
**File**: `Services/PgNumberGenerator.cs`
**Dependencies**: Task 2
**Description**: Generate PG numbers based on motor number and configuration.
**Implementation**:
- `GeneratePgNumbers(motorNumber, config)` - returns array of PG numbers
- Pattern matching: `PG-{motor}-{row}-{timestamp}` format
- Increment logic: configurable start number and increment step
- Collision detection: prevent duplicate PG numbers
- Validation: ensure PG numbers match expected format
- Error handling: invalid inputs return empty array with error message
**Verification**: Generator produces unique PG numbers, pattern matches specification, invalid inputs handled

---

### Task 4: Implement XML Conversion Service
**File**: `Services/XmlConverter.cs`
**Dependencies**: Task 2
**Description**: Convert table data to industrial system XML format.
**Implementation**:
- `GenerateXml(tableRows, motorNumber)` - produces XML string
- XML structure: `<Betriebsmittel><PG-Numbers><PG>...</PG></PG-Numbers></Betriebsmittel>`
- Element mapping: MotorNumber, PgNumber, Timestamp, Status
- XML validation: proper encoding, required fields present
- Error handling: invalid data produces error XML with validation message
- UTF-8 encoding for international character support
**Verification**: XML structure matches specification, validation works, encoding correct

---

### Task 5: Add Table UI Components
**File**: `UI/AutomationWindow.cs`
**Dependencies**: Task 1, Task 2
**Description**: Add data table with 10 rows and column headers.
**Implementation**:
- Use DataGridView for 10-row table display
- Columns: Row, Motor Number, PG Number, Status, Actions
- Data binding to PgAutomationModel
- Row styling: alternate row colors, status-based highlighting
- Cell editing: motor number column editable, others read-only
- Button column: per-row actions (generate, publish, clear)
- Auto-sizing columns with minimum widths
**Verification**: Table displays 10 rows, columns render correctly, data binding works

---

### Task 6: Implement Automation Logic Integration
**File**: `UI/AutomationWindow.cs`
**Dependencies**: Task 3, Task 4, Task 5
**Description**: Wire up PG generation, XML conversion, and publishing actions.
**Implementation**:
- Generate PG Numbers button: calls PgNumberGenerator, updates table
- Publish XML button: calls XmlConverter, publishes via MQTT client
- Clear Table button: resets table to default state
- Motor number input change: validates input, clears table
- Per-row actions: individual row generation/publishing
- Status updates: real-time status display in table
- Error handling: inline error messages for failed operations
**Verification**: Buttons trigger correct actions, table updates properly, errors displayed

---

### Task 7: Integrate with MQTT Client for Publishing
**File**: `UI/AutomationWindow.cs`, `Services/XmlConverter.cs`
**Dependencies**: Task 4, Task 6, Phase 4 (MqttClient)
**Description**: Add MQTT publishing functionality to automation window.
**Implementation**:
- Load MQTT client settings from SettingsModel
- Connect to MQTT broker on window load
- Publish XML to configured operating resource topics
- Handle publish confirmation and errors
- Disconnect on window close
- Connection status indicator (connected/disconnected)
- Retry logic for failed publishes
- Topic selection: publish to specific or all topics
**Verification**: MQTT publishing works, connection status updates, errors handled

---

## 3. Dependency Graph

```
Task 1: AutomationWindow Form (no dependencies)
  └─> Task 5: Table UI Components (depends on Task 1, Task 2)

Task 2: PgAutomationModel (no dependencies)
  └─> Task 3: PgNumberGenerator (depends on Task 2)
  └─> Task 4: XmlConverter (depends on Task 2)

Task 3: PgNumberGenerator (depends on Task 2)
  └─> Task 6: Automation Logic Integration (depends on Task 3, Task 4, Task 5)

Task 4: XmlConverter (depends on Task 2)
  └─> Task 6: Automation Logic Integration (depends on Task 3, Task 4, Task 5)

Task 5: Table UI Components (depends on Task 1, Task 2)
  └─> Task 6: Automation Logic Integration (depends on Task 3, Task 4, Task 5)

Task 6: Automation Logic Integration (depends on Task 3, Task 4, Task 5)
  └─> Task 7: MQTT Integration (depends on Task 4, Task 6, Phase 4)
```

**Parallel Execution Opportunities:**
- Tasks 1, 2 can run in parallel (both independent)
- Task 3, 4 can run in parallel after Task 2 completes
- Task 5 can start after Task 1, Task 2 completes
- Tasks 6, 7 are strictly sequential after Task 5

---

## 4. Implementation Approach

### Window Layout Strategy
**Design**: Header + Input Section + Table + Action Buttons (4 sections)
**Header**: Title "PG-Number Automation" with status indicator
**Input Section**: Motor number TextBox (width: 200px), help text
**Table**: 10-row DataGridView with 5 columns (Row, Motor, PG Number, Status, Actions)
**Action Buttons**: Generate, Publish XML, Clear (right-aligned, primary/secondary styling)

### Data Model Design
**Simple State Management**: PgAutomationModel holds all state
**Observable Pattern**: Implement INotifyPropertyChanged for UI updates
**Row Objects**: Each table row is a PgTableRow instance
**Validation**: Motor number validation in PgAutomationModel.SetMotorNumber()

### PG Number Generation Strategy
**Pattern**: `PG-{motor}-{row}-{timestamp}` where timestamp = `yyyyMMddHHmmss`
**Motor Number**: User-provided base number (e.g., 12345)
**Row Number**: 1-10 for 10-row table
**Collision Handling**: Add microsecond if collision detected
**Configuration**: Prefix="PG", StartNumber=1, Increment=1, MaxRows=10

### XML Generation Strategy
**Format**: Industrial XML structure for Betriebsmittel systems
**Root Element**: `<Betriebsmittel>`
**Container**: `<PG-Numbers>` containing `<PG>` elements
**Fields**: `<MotorNumber>`, `<PgNumber>`, `<Timestamp>`, `<Status>`
**Encoding**: UTF-8 with `<?xml version="1.0" encoding="UTF-8"?>` declaration
**Validation**: All required fields present, numeric values valid

### MQTT Integration Approach
**Connection Management**: Use Phase 4 MqttClient with SettingsModel parameters
**Publishing Strategy**: Publish XML string as byte[] payload
**Topic Selection**: Use all 4 operating resource topics from settings
**QoS Level**: QoS 0 (At Most Once) for non-critical automation data
**Error Handling**: Connection errors retry 3 times with exponential backoff

### Integration with Design System
**BaseForm Inheritance**: AutomationWindow extends BaseForm for dark mode
**DesignSystem Usage**: All colors, fonts, spacing from DesignSystem constants
**Control Styling**: Custom TextBoxes, DataGridView styling via events
**Button Patterns**: Primary/secondary button styling from DesignSystem

---

## 5. Risk Mitigation

### Risk 1: Invalid Motor Number Input
**Mitigation**: 
- Real-time validation on TextChanged event
- Numeric-only input (KeyPress filter)
- Inline error display with red border
- Generate button disabled until valid input

### Risk 2: PG Number Collisions
**Mitigation**:
- Timestamp-based uniqueness (microsecond precision)
- Collision detection with auto-increment fallback
- Validation before adding to table
- Error message for collision scenarios

### Risk 3: XML Structure Mismatches
**Mitigation**:
- Follow industrial XML specification exactly
- Validate XML structure before publishing
- Schema validation (basic structural validation)
- Error handling for malformed XML generation

### Risk 4: MQTT Connection Failures
**Mitigation**:
- Connection status indicator in UI
- Retry logic with exponential backoff (1s, 2s, 4s)
- Graceful degradation (continue offline, queue publishes)
- User notification of connection failures

### Risk 5: Table Performance Issues
**Mitigation**:
- Limit to 10 rows (fixed size, no dynamic expansion)
- Data binding optimization (INotifyPropertyChanged)
- Virtual mode for large datasets (not needed for 10 rows)
- Batch updates for efficiency

### Risk 6: Thread Safety Issues
**Mitigation**:
- UI operations on UI thread (Invoke/BeginInvoke)
- MQTT operations on background thread (Task.Run)
- Thread-safe model updates (lock-based synchronization)
- Async operations with proper cancellation

---

## 6. Testing Strategy

### Unit Testing (Manual, No Framework)
- Test PgNumberGenerator with various motor numbers
- Test XmlConverter table-to-XML conversion
- Test PgAutomationModel validation logic
- Test status tracking and transitions

### Integration Testing
- Motor number input → PG number generation
- PG number generation → XML conversion
- XML conversion → MQTT publishing
- Table CRUD operations (generate, clear, publish)
- Connection lifecycle (connect → publish → disconnect)

### UI Testing
- Automation window renders with correct layout
- Dark mode theme consistency
- Table row/column styling matches design system
- Button states update correctly
- Error messages display appropriately
- Keyboard navigation works (tab, enter, escape)

### MQTT Integration Testing
- Connect to MQTT broker on window load
- Publish XML to all 4 configured topics
- Handle connection failures gracefully
- Verify MQTT message format and content
- Test connection status indicator updates

---

## 7. Estimated Timeline
**Task 1**: 1.5 hours
**Task 2**: 1 hour
**Task 3**: 1.5 hours
**Task 4**: 1.5 hours
**Task 5**: 2 hours
**Task 6**: 2 hours
**Task 7**: 1.5 hours
**Total**: ~11 hours (2-3 days)

---

## 8. Exit Criteria
Phase 5 is complete when:
- [ ] AutomationWindow.cs compiles and runs without errors
- [ ] Motor number input accepts and validates numeric values
- [ ] 10-row data table renders with all columns and styling
- [ ] PG numbers auto-generate correctly for all 10 rows
- [ ] XML output matches industrial system specification format
- [ ] MQTT publishing works with all 4 configured topics
- [ ] Table-to-XML conversion validates data before generation
- [ ] Integration with Phase 4 MQTT client is functional
- [ ] Window integrates with MainForm via menu/button
- [ ] All UI follows DesignSystem theme and spacing constants
- [ ] No exceptions logged during normal operation

---

## 9. Threat Model

### Security Requirements

**Input Validation:**
- Motor numbers validated as numeric only
- Table row count fixed at 10 (no dynamic expansion)
- XML generation escapes special characters
- MQTT topic validation from Phase 3 prevents injection

**Resource Protection:**
- MQTT connections limited (single connection per session)
- Table size limited (10 rows max, prevents memory exhaustion)
- XML payload size limited (prevents buffer overflow attacks)
- Connection timeout configured (prevents hanging connections)

**Protocol Security:**
- MQTT publishing uses QoS 0 (minimal overhead, no sensitive data)
- No authentication credentials exposed in automation phase
- XML content contains only operational data (no secrets)
- Connection failures handled gracefully (no information leakage)

**Data Integrity:**
- PG number collision detection prevents data corruption
- XML validation ensures structured output
- Status tracking prevents inconsistent state
- Error messages sanitized before display

---

## 10. Artifacts This Phase Produces

**New Classes:**
- `BetriebsmittelPublisher.UI.AutomationWindow`
- `BetriebsmittelPublisher.Models.PgAutomationModel`
- `BetriebsmittelPublisher.Models.PgTableRow`
- `BetriebsmittelPublisher.Services.PgNumberGenerator`
- `BetriebsmittelPublisher.Services.XmlConverter`

**New Enums:**
- `BetriebsmittelPublisher.Models.PgRowStatus` (pending, generated, published, error)

**Modified Classes:**
- `BetriebsmittelPublisher.UI.MainForm` (add Automation menu item)
- `BetriebsmittelPublisher.Services.MqttClient` (publishing method integration)
- `BetriebsmittelPublisher.Models.SettingsModel` (add automation settings if needed)

**New Configuration Fields:**
- `PgAutomationModel.MotorNumber`
- `PgAutomationModel.TableRows[]`
- `PgAutomationModel.LastGenerated`
- `PgNumberGeneratorConfig.Prefix`, `StartNumber`, `Increment`, `MaxRows`