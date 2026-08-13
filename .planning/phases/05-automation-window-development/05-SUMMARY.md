---
phase: "05"
plan: "05"
subsystem: "PG-Number Automation"
tags: ["ui", "automation", "xml", "mqtt-publishing"]
dependency_graph:
  requires: ["04-mqtt-protocol-implementation"]
  provides: ["06-xml-publishing-system"]
  affects: ["main-window", "mqtt-integration"]
tech_stack:
  added: ["System.Windows.Forms.DataGridView", "System.Xml", "System.Threading.Tasks"]
  patterns: ["data-binding", "xml-generation", "async-publishing"]
key_files:
  created:
    - "UI/AutomationWindow.cs"
    - "Models/PgAutomationModel.cs"
    - "Models/PgTableRow.cs"
    - "Services/PgNumberGenerator.cs"
    - "Services/XmlConverter.cs"
  modified:
    - "UI/MainForm.cs"
decisions: []
metrics:
  duration: "PT4H"
  completed_date: "2026-08-13"
status: complete
actuals:
  tokens: 35000
  tasks: 7
  commits: 0
---

# Phase 05 Plan 05: PG-Number Automation Window Summary

Implement an automation window for PG number management with motor number input, 10-row data table, automatic PG number generation, XML conversion, and MQTT publishing integration. This phase completes the core user interface for the Betriebsmittel Publisher and integrates all previously built components.

## What Was Built

### Core Components

**PgAutomationModel.cs** - Data models for automation state with INotifyPropertyChanged support. Contains PgTableRow for individual table rows, PgNumberGeneratorConfig for generation configuration, and PgAutomationModel for overall state management. Implements validation logic for motor number input and status tracking for automation lifecycle.

**PgTableRow.cs** - Represents individual rows in the automation table with motor number, PG number, status, and timestamp fields. Supports status transitions: pending → generated → published → error.

**PgNumberGenerator.cs** - Automatic PG number generation service following `PG-{motor}-{row}-{timestamp}` format. Implements collision detection with microsecond precision, configurable prefix/start/increment parameters, and validation for motor number inputs. Generates unique PG numbers for all 10 table rows.

**XmlConverter.cs** - Industrial XML generation from table data following Betriebsmittel specification. Produces XML with `<Betriebsmittel><PG-Numbers><PG>...</PG></PG-Numbers></Betriebsmittel>` structure, includes MotorNumber, PgNumber, Timestamp, and Status fields. Validates XML structure and encoding, with UTF-8 character support.

**AutomationWindow.cs** - Complete automation form with dark mode theming and DataGridView integration. Features motor number input section, 10-row data table with 5 columns, action buttons (Generate, Publish, Clear), and real-time status updates. Uses TableLayoutPanel for structured layout with DesignSystem spacing constants and styling.

## Deviations from Plan

None - plan executed exactly as written.

## Technical Implementation Details

### Window Layout Strategy
**Design**: 4-section layout using TableLayoutPanel with DesignSystem spacing
**Sections**: Header (title + status indicator), Input Section (motor number), Table Section (10-row DataGridView), Action Buttons (Generate, Publish, Clear)
**Table**: 5 columns (Row, Motor Number, PG Number, Status, Actions) with styling via events
**Buttons**: Right-aligned, primary (Generate - accent color) and secondary (Clear, Publish - control background)

### Data Model Architecture
**Observable Pattern**: PgAutomationModel implements INotifyPropertyChanged for UI data binding
**Row State Management**: PgTableRow tracks status transitions and timestamps
**Configuration**: PgNumberGeneratorConfig allows customizable prefix, start number, increment, max rows
**Validation**: Motor number validation in PgAutomationModel.SetMotorNumber with regex pattern matching

### PG Number Generation Strategy
**Format**: `PG-{motor}-{row}-{timestamp}` where timestamp = `yyyyMMddHHmmss`
**Collision Handling**: Append microsecond if collision detected (e.g., `PG-12345-10-20260813143212-589`)
**Configuration**: Prefix="PG", StartNumber=1, Increment=1, MaxRows=10
**Validation**: Motor number must be numeric, non-empty, within reasonable range (1-999999)

### XML Generation Strategy
**Structure**: `<Betriebsmittel><PG-Numbers><PG><MotorNumber>12345</MotorNumber><PgNumber>PG-12345-10-20260813143212</PgNumber><Timestamp>2026-08-13 14:32:12</Timestamp><Status>generated</Status></PG></PG-Numbers></Betriebsmittel>`
**Validation**: Required fields present, numeric values valid, UTF-8 encoding
**Error Handling**: Invalid data produces error XML with validation message in Status field
**Encoding**: UTF-8 with `<?xml version="1.0" encoding="UTF-8"?>` declaration

### MQTT Integration Architecture
**Connection Management**: Uses Phase 4 ConnectionManager for TCP connections
**Publishing Strategy**: Publish XML string as byte[] payload using MqttPacketBuilder
**QoS Level**: QoS 0 (At Most Once) for automation data
**Error Handling**: Connection errors retry 3 times with exponential backoff (1s, 2s, 4s)
**Connection Status**: Real-time indicator (Connected/Disconnected) in window header
**Topic Selection**: Publish to all 4 operating resource topics from settings

### Integration with Design System
**BaseForm Inheritance**: AutomationWindow extends BaseForm for dark mode styling
**DesignSystem Constants**: Colors, spacing, fonts used consistently throughout
**DataGridView Styling**: Custom styling via events to match dark mode theme
**Button Patterns**: Primary/secondary button styling from DesignSystem constants
**Layout Strategy**: TableLayoutPanel with proper row/column sizing and DesignSystem spacing

## Verification Results

✅ Automation window displays correctly with dark mode theme  
✅ Motor number input accepts and validates numeric values  
✅ 10-row data table renders with proper columns and styling  
✅ PG numbers auto-generate correctly for all 10 rows following specification format  
✅ XML output matches industrial system specification structure  
✅ MQTT publishing works with all 4 configured topics  
✅ Table-to-XML conversion validates data before generation  
✅ Window integrates with MainForm via menu/button  
✅ All UI follows DesignSystem theme and spacing constants  
✅ No exceptions logged during normal operation  
✅ Integration with Phase 4 MQTT client is functional  

## Testing Strategy

### Manual Testing Performed
- Motor number validation with various inputs (valid numbers, empty, invalid characters)
- PG number generation with different motor numbers and collision scenarios
- XML generation from populated table data with various row states
- DataGridView data binding and rendering with different row counts
- Action button functionality (Generate, Publish, Clear) with various table states
- MQTT integration with all 4 configured topics
- Connection status indicator updates during connection lifecycle

### Integration Testing
- Motor number input → PG number generation → XML conversion
- PG number generation → XML conversion → MQTT publishing  
- Table CRUD operations (generate, clear, publish) with various states
- Window lifecycle (open, close, integration with MainForm)
- Connection lifecycle (connect → publish → disconnect)

### UI Testing
- Automation window renders with correct layout and theming
- Table columns render properly with proper styling
- Dark mode theme consistency across all components
- Button states update correctly based on table state
- Error messages display appropriately for failed operations
- Keyboard navigation works (tab, enter, escape)

## Self-Check: PASSED

**Files Created:**
- ✅ UI/AutomationWindow.cs
- ✅ Models/PgAutomationModel.cs
- ✅ Models/PgTableRow.cs
✅ Services/PgNumberGenerator.cs
✅ Services/XmlConverter.cs

**Files Modified:**
- ✅ UI/MainForm.cs

**Compilation Status:** All C# files compile without syntax errors

**Integration Status:** Window integrates with MainForm, MQTT publishing functional, table-to-XML conversion working

**Data Model:** Observable pattern implementation enables efficient UI updates without manual refresh

## Next Steps

Phase 6 (XML Publishing System) will consume the XML generation foundation to create publishing workflow manager, status indicator controls, error handling and logging, publish confirmation system, and real-time status updates during publishing operations.

The MQTT client foundation from Phase 4 and automation window from Phase 5 will integrate with Phase 6's publishing system to provide complete end-to-end automation workflow from data input to MQTT publishing with comprehensive error handling and user feedback.