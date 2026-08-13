# Phase 3: Settings Window Development - Context

## Design System Foundation (Phase 2)
**Completed in Phase 2**: Complete design system implementation with embedded fonts, color scheme constants, and reusable UI control library.

**Key Components Available**:
- `DesignSystem.cs`: Font management and color constants (#1a1d29 background, #ff5c5c accent)
- `BaseForm.cs`: Base styling for all forms with dark mode theme
- `DesignPatterns.cs`: Reusable UI component patterns (status labels, horizontal grouping, createTextBox)

**Thematic Styles**: 
- Flat, modern appearance with 2px border
- Consistent spacing (6px, 8px, 10px spacing constants)
- JetBrains Mono for UI, Inter for text rendering
- Background color: `DesignSystem.Colors.DarkBackground` (#1a1d29)

## Settings Configuration Requirements
From REQUIREMENTS.md FR-001 and UXR-001, UXR-002:

**Functional Requirements**:
- Settings window for configuration
- 4 configurable publish topics for operating resources
- Automatic station number parsing from topics
- Local storage persistence
- Load settings on application startup

**UI/UX Requirements**:
- Resizable and responsive window
- Consistent dark mode theme
- Readable text in dark mode
- User-friendly error messages
- Invalid inputs highlighted
- No crashes on user errors

**Data Model Requirements**:
- Settings structure: `betriebsmittel[1-4]` topics
- Station number extraction from topic strings
- Validation for MQTT topic conventions
- Persistence format: settings.ini (simple key-value)

## Technical Constraints
- Single .exe deployment (no external files)
- No NuGet dependencies (implement parser manually)
- Offline capable (no external services)
- Windows 10+ support
- < 50MB memory consumption

## Integration Points
- **MainForm**: Will trigger settings window via menu/button
- **DesignSystem**: All UI components must use design system styling
- **Future MQTT Phase**: Settings provide connection parameters for Phase 4

## Input Validation Requirements (SR-002)
- All inputs validated before saving
- MQTT topic conventions enforced (no spaces, valid characters)
- Station numbers numeric only
- Empty fields rejected
- Validation errors displayed in UI (not alerts)