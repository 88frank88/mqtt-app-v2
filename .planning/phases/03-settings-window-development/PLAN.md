# Phase 3: Settings Window Development - Execution Plan

## 1. Plan Overview

### Summary
Implement a comprehensive settings window allowing users to configure 4 MQTT publish topics for operating resources, with automatic station number extraction, persistent storage, and input validation. This phase builds on the design system foundation (Phase 2) to create the first functional user interaction component.

### Key Deliverables
- `Models/SettingsModel.cs`: Configuration data structure
- `UI/SettingsWindow.cs`: Main settings form with validation
- `Services/SettingsPersistence.cs`: Save/load settings to local storage
- `Services/StationNumberParser.cs`: Extract station numbers from topic strings
- Integration with BaseForm and DesignSystem styling

### Success Criteria
- [ ] Settings window renders with consistent dark mode theme
- [ ] 4 topic input fields accept and validate MQTT topic strings
- [ ] Station numbers auto-extract and display correctly
- [ ] Settings persist to `settings.ini` in application directory
- [ ] Settings load on window initialization
- [ ] Invalid inputs trigger inline error states
- [ ] Window dismisses with "Cancel" or applies with "Save"

---

## 2. Task Breakdown

### Task 1: Create Settings Data Model
**File**: `Models/SettingsModel.cs`
**Dependencies**: None
**Description**: Define the settings data structure with 4 topic properties and validation rules.
**Implementation**:
```csharp
public class SettingsModel
{
    public string Betriebsmittel1Topic { get; set; } = "procon/bm1/data";
    public string Betriebsmittel2Topic { get; set; } = "procon/bm2/data";
    public string Betriebsmittel3Topic { get; set; } = "procon/bm3/data";
    public string Betriebsmittel4Topic { get; set; } = "procon/bm4/data";
    public int? StationNumber1 => StationNumberParser.ExtractStationNumber(Betriebsmittel1Topic);
    // ... similar for 2-4
}
```
**Verification**: Model compiles, default values set, null-safe station number accessors work

---

### Task 2: Implement Station Number Parser
**File**: `Services/StationNumberParser.cs`
**Dependencies**: None
**Description**: Parse station numbers from MQTT topic strings (e.g., "procon/bm1/station/42" → 42).
**Implementation**:
- Pattern match: extract numeric digits following `/station/` or format `procon/bm{N}/station/{NUM}`
- Fallback: extract first numeric sequence in topic path segments
- Return `int?` for missing/invalid numbers
**Verification**: Parser correctly extracts 42 from "procon/bm1/station/42", returns null for "procon/bm1/data"

---

### Task 3: Implement Settings Persistence Service
**File**: `Services/SettingsPersistence.cs`
**Dependencies**: Task 1 (SettingsModel)
**Description**: Save and load settings to/from `settings.ini` file in application directory.
**Implementation**:
- Save: Write `key=value` pairs (betriebsmittel1_topic=procon/bm1/data)
- Load: Parse INI file, populate SettingsModel
- Create file if missing (use defaults)
- Handle file I/O errors gracefully
**Verification**: Settings persist across application restarts, default settings work on first run

---

### Task 4: Create Settings Window Form
**File**: `UI/SettingsWindow.cs`
**Dependencies**: Task 1 (SettingsModel), Phase 2 (BaseForm, DesignSystem)
**Description**: Build the main settings form with 4 topic input fields and station number displays.
**Implementation**:
- Inherit from BaseForm for consistent dark mode styling
- Layout: Vertical stack of 4 topic input groups (each: label, TextBox, station number Label)
- Bottom row: Save/Cancel buttons
- Use DesignSystem.CreateTextBox for consistent styling
- Apply DesignSystem spacing constants
**Verification**: Window renders with correct layout, controls styled per design system

---

### Task 5: Add Topic Input Validation
**Files**: `UI/SettingsWindow.cs`, `Models/SettingsModel.cs`
**Dependencies**: Task 4 (SettingsWindow)
**Description**: Validate MQTT topic strings on input and on save.
**Implementation**:
- Validate characters: alphanumeric, `/`, `_`, `-` only
- Reject empty strings
- Reject spaces
- Provide inline validation error (red border, status text)
- Disable Save button if any validation fails
**Verification**: Invalid inputs show error states, valid inputs clear errors, Save button state updates correctly

---

### Task 6: Implement Save and Load Actions
**File**: `UI/SettingsWindow.cs`
**Dependencies**: Task 3 (SettingsPersistence), Task 5 (Validation)
**Description**: Wire up Save and Cancel button actions with persistence.
**Implementation**:
- Save button: Validate all inputs, create SettingsModel from form data, call SettingsPersistence.Save, close window
- Cancel button: Close window without saving
- OnLoad: Load settings from persistence, populate form fields
- Handle exceptions (file write errors) with inline error messages
**Verification**: Saving writes to settings.ini, loading reads from settings.ini, Cancel discards changes

---

### Task 7: Integrate with MainForm (Launcher)
**File**: `UI/MainForm.cs`
**Dependencies**: Task 6 (SettingsWindow fully functional)
**Description**: Add menu item/button to open SettingsWindow from MainForm.
**Implementation**:
- Add "Settings" menu item or button to MainForm
- On click: `using (var settings = new SettingsWindow()) { settings.ShowDialog(); }`
- Optionally refresh MainForm if settings affect UI (not needed in this phase)
**Verification**: SettingsWindow opens from MainForm, returns control when closed

---

## 3. Dependency Graph

```
Task 1: SettingsModel (no dependencies)
  └─> Task 3: SettingsPersistence (depends on Task 1)

Task 2: StationNumberParser (no dependencies)
  └─> Task 4: SettingsWindow (depends on Task 2, Task 1, Phase 2)

Task 4: SettingsWindow (depends on Task 1, Task 2, Phase 2)
  └─> Task 5: Topic Validation (depends on Task 4)
       └─> Task 6: Save/Load Actions (depends on Task 5, Task 3)
            └─> Task 7: MainForm Integration (depends on Task 6)
```

**Parallel Execution Opportunities**:
- Tasks 1, 2 can run in parallel (both independent)
- Task 3 can start after Task 1 completes
- Task 4 can start after Task 2 completes (Task 1 also needed, but that's done)
- Task 5, 6, 7 are strictly sequential after Task 4

---

## 4. Implementation Approach

### Settings Window Layout Strategy
**Design**: Vertical stack using TableLayoutPanel or FlowLayoutPanel with DesignSystem spacing constants.
**Component Grouping**: Each topic input group contains:
- Label (e.g., "Betriebsmittel 1 Topic:")
- TextBox (width: 400px, horizontal alignment: Fill)
- Station number display Label (smaller font, accent color)
**Button Row**: Right-aligned Save (primary) and Cancel (secondary) buttons

### Data Model Design
**Simple POCO**: No serialization attributes needed (manual INI parsing)
**Null Safety**: Station numbers as `int?` to handle missing values gracefully
**Immutability Consideration**: SettingsModel can be immutable for thread safety (future proofing)

### Persistence Mechanism
**File Format**: Simple INI-style `key=value` pairs
**File Location**: Application base directory (`AppDomain.CurrentDomain.BaseDirectory`)
**Format Example**:
```ini
[Betriebsmittel]
topic1=procon/bm1/station/42
topic2=procon/bm2/station/43
```
**Error Handling**: 
- File not found → Use defaults
- Parse errors → Use default for that field, log warning
- Write errors → Show inline error, don't crash

### Station Number Parsing Algorithm
**Primary Pattern**: Extract numeric value after `/station/` in topic path
**Fallback Pattern**: Extract first numeric segment in topic (e.g., "procon/bm1/data" → 1)
**Implementation**:
```csharp
// Regex approach (no external deps)
var match = Regex.Match(topic, @"/station/(\d+)");
if (match.Success) return int.Parse(match.Groups[1].Value);
// Fallback: find digits
var fallback = Regex.Match(topic, @"\d+");
return fallback.Success ? int.Parse(fallback.Value) : null;
```
**Performance**: Regex is cached by .NET runtime, acceptable for this use case

### UI Component Usage from Phase 2
**BaseForm**: All forms inherit for dark mode background, sizing, title bar
**DesignSystem.CreateTextBox**: Standard textboxes with flat border, correct colors
**DesignSystem spacing**: Use 8px or 10px between control groups, 6px for tight groups
**Color constants**: Use DesignSystem.Colors.DarkBackground, AccentColor for UI elements

### Integration with BaseForm
**Inheritance**: `SettingsWindow : BaseForm`
**Constructor**: Call `base.InitializeComponents()`, then add custom controls
**Theming**: All controls must be styled (background, foreground, border) to match BaseForm theme
**Window Size**: Approximate 600x500 pixels (adjust during implementation)

---

## 5. Risk Mitigation

### Risk 1: User Inputs Invalid MQTT Topics
**Mitigation**: 
- Real-time validation on TextChanged event
- Inline error display (red border, status label)
- Save button disabled until all valid
- Helper text showing valid format examples

### Risk 2: File I/O Errors (permissions, disk full)
**Mitigation**:
- Try-catch around all file operations
- Graceful degradation: use defaults if save fails
- Show user-friendly error message in UI
- Log errors (future: logging service)

### Risk 3: Station Number Parsing Ambiguity
**Mitigation**:
- Primary pattern: `/station/{N}` format
- Fallback: first numeric segment
- Document expected topic format in UI helper text
- Allow manual override (future phase)

### Risk 4: Design System Changes in Future
**Mitigation**:
- Use DesignSystem constants consistently (no hardcoded colors)
- Follow BaseForm inheritance pattern
- Group UI logic into methods (e.g., `CreateTopicInputGroup`)

### Risk 5: Application Startup Without Existing Settings File
**Mitigation**:
- Detect missing file on load
- Use default settings from SettingsModel defaults
- Create file on first save
- No first-run wizard needed (silent defaults acceptable)

### Risk 6: Memory Leaks in Form Handling
**Mitigation**:
- Use `using` pattern for SettingsWindow in MainForm
- Dispose properly in BaseForm if not using `using`
- Avoid event handler leaks (detach when not needed)

---

## 6. Testing Strategy

### Unit Testing (Manual, No Framework)
- Test StationNumberParser with various topic formats
- Test SettingsPersistence save/load roundtrip
- Test SettingsModel default values
- Test validation rules (valid/invalid inputs)

### Integration Testing
- Open SettingsWindow from MainForm
- Input valid topics, save, reload, verify persistence
- Input invalid topics, verify error states
- Cancel changes, verify no persistence
- Modify settings.ini manually, verify load

### UI Testing
- Verify dark mode consistency
- Verify control styling matches DesignSystem
- Verify window responsiveness (resize, keyboard navigation)
- Verify button states (Save disabled with invalid input)

---

## 7. Estimated Timeline
**Task 1**: 30 minutes
**Task 2**: 45 minutes
**Task 3**: 1 hour
**Task 4**: 1.5 hours
**Task 5**: 1 hour
**Task 6**: 1 hour
**Task 7**: 30 minutes
**Total**: ~6 hours (1 day with buffer)

---

## 8. Exit Criteria
Phase 3 is complete when:
- [ ] SettingsWindow.cs compiles and runs without errors
- [ ] 4 topic inputs accept, validate, and display values
- [ ] Station numbers auto-extract and update on input change
- [ ] Settings persist to settings.ini and reload correctly
- [ ] Validation errors display inline, Save button state correct
- [ ] Window integrates with MainForm via menu/button
- [ ] All UI follows DesignSystem theme and spacing
- [ ] No exceptions logged during normal operation