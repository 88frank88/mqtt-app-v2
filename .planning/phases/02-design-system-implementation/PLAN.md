# PLAN.md: Phase 2 - Design System Implementation

## Wave 1: Font Embedding and Management

### Plan: Font Resource Setup and FontManager Implementation

**wave**: 1
**depends_on**: []
**files_modified**: [Resources/JetBrainsMono-Regular.ttf, Resources/JetBrainsMono-Bold.ttf, Resources/Inter-Regular.ttf, Resources/Inter-Bold.ttf, Core/FontManager.cs, BetriebsmittelPublisher.csproj]
**autonomous**: true

## Tasks

### Task 2.1: Add Font Files to Resources Directory

<read_first>
- No existing files to read
</read_first>

<action>
Create Resources/ directory and add font files:
- Resources/JetBrainsMono-Regular.ttf (download from JetBrains Mono repository)
- Resources/JetBrainsMono-Bold.ttf (download from JetBrains Mono repository)
- Resources/Inter-Regular.ttf (download from Inter font repository)
- Resources/Inter-Bold.ttf (download from Inter font repository)
</action>

<acceptance_criteria>
- Resources/ directory exists
- JetBrainsMono-Regular.ttf exists in Resources/
- JetBrainsMono-Bold.ttf exists in Resources/
- Inter-Regular.ttf exists in Resources/
- Inter-Bold.ttf exists in Resources/
- All .ttf files have file size > 0 bytes
- Font files are valid TrueType format (not corrupted)
</acceptance_criteria>

---

### Task 2.2: Configure Font Files as Embedded Resources in .csproj

<read_first>
- BetriebsmittelPublisher.csproj (for current project configuration)
</read_first>

<action>
Update BetriebsmittelPublisher.csproj to include:
- EmbeddedResource item for Resources/JetBrainsMono-Regular.ttf
- EmbeddedResource item for Resources/JetBrainsMono-Bold.ttf
- EmbeddedResource item for Resources/Inter-Regular.ttf
- EmbeddedResource item for Resources/Inter-Bold.ttf
</action>

<acceptance_criteria>
- BetriebsmittelPublisher.csproj contains `<EmbeddedResource Include="Resources\JetBrainsMono-Regular.ttf" />`
- BetriebsmittelPublisher.csproj contains `<EmbeddedResource Include="Resources\JetBrainsMono-Bold.ttf" />`
- BetriebsmittelPublisher.csproj contains `<EmbeddedResource Include="Resources\Inter-Regular.ttf" />`
- BetriebsmittelPublisher.csproj contains `<EmbeddedResource Include="Resources\Inter-Bold.ttf" />`
- `dotnet build` succeeds without errors
- Embedded resources can be accessed via Assembly.GetManifestResourceStream()
</acceptance_criteria>

---

### Task 2.3: Implement FontManager Class with PrivateFontCollection

<read_first>
- Core/DesignSystem.cs (for integration with design system)
</read_first>

<action>
Create Core/FontManager.cs with:
- Static class FontManager
- Private static field: PrivateFontCollection _privateFontCollection
- Private static fields: FontFamily _jetBrainsMonoFamily, FontFamily _interFamily
- Public static method: Initialize() that loads all fonts from embedded resources
- Private method: LoadFontFromResource(string resourcePath) that returns FontFamily
- Public static method: GetJetBrainsMonoFont(float size, FontStyle style) with fallback to Consolas
- Public static method: GetInterFont(float size, FontStyle style) with fallback to Segoe UI
- Private static method: GetFont(FontFamily family, float size, FontStyle style)
- Public static property: JetBrainsMonoFamily (returns FontFamily or null)
- Public static property: InterFamily (returns FontFamily or null)
- Exception handling for font loading failures with fallback to system fonts
- Memory pinning using Marshal.UnsafeAddrOfPinnedArrayElement for AddMemoryFont
</action>

<acceptance_criteria>
- Core/FontManager.cs exists
- class FontManager is static
- contains static Initialize() method
- contains static GetJetBrainsMonoFont(float, FontStyle) method
- contains static GetInterFont(float, FontStyle) method
- contains static JetBrainsMonoFamily property
- contains static InterFamily property
- GetJetBrainsMonoFont returns Font with JetBrains Mono family when available
- GetJetBrainsMonoFont returns Font with Consolas family when JetBrains Mono unavailable
- GetInterFont returns Font with Inter family when available
- GetInterFont returns Font with Segoe UI family when Inter unavailable
- Fonts can be loaded from embedded resources without exceptions
- FontManager.Initialize() can be called without throwing exceptions
</acceptance_criteria>

---

### Task 2.4: Add Font Constants to DesignSystem

<read_first>
- Core/DesignSystem.cs (for existing color constants)
</read_first>

<action>
Update Core/DesignSystem.cs to add:
- Nested static class Typography with:
  - DefaultMonoFontSize: 9.0f
  - DefaultSansFontSize: 9.5f
  - HeadingFontSize: 12.0f
  - MonoFontFamily reference to FontManager.JetBrainsMonoFamily
  - SansFontFamily reference to FontManager.InterFamily
- Public static method: GetMonoFont(float size = 9.0f, FontStyle style = FontStyle.Regular)
- Public static method: GetSansFont(float size = 9.5f, FontStyle style = FontStyle.Regular)
</action>

<acceptance_criteria>
- Core/DesignSystem.cs contains nested static class Typography
- Typography.DefaultMonoFontSize equals 9.0f
- Typography.DefaultSansFontSize equals 9.5f
- Typography.HeadingFontSize equals 12.0f
- DesignSystem.GetMonoFont() returns Font from JetBrains Mono family
- DesignSystem.GetSansFont() returns Font from Inter family
- GetMonoFont(float, FontStyle) returns Font with specified parameters
- GetSansFont(float, FontStyle) returns Font with specified parameters
- Font methods call FontManager methods internally
</acceptance_criteria>

---

## Wave 2: Extended Control Theming System

### Plan: Comprehensive Control Styling and Theme Extensions

**wave**: 2
**depends_on**: [2.3, 2.4]
**files_modified**: [UI/BaseForm.cs, Core/DesignSystem.cs, Core/ThemeManager.cs]
**autonomous**: true

### Task 2.5: Extend BaseForm Control Theming with Complete Control Type Coverage

<read_first>
- UI/BaseForm.cs (for existing theming methods)
- Core/DesignSystem.cs (for color constants)
</read_first>

<action>
Update UI/BaseForm.cs to extend ApplyThemeToControl(Control control) with comprehensive styling for:
- Button: FlatStyle.Flat, 1px border, hover states, accent color for primary buttons
- TextBox: FlatStyle.Flat, consistent border, proper text colors, focus handling
- ComboBox: FlatStyle.Flat, dropdown styling, item colors
- CheckBox: FlatStyle.Flat, custom checkbox appearance, consistent check colors
- RadioButton: FlatStyle.Flat, custom radio appearance, consistent selection colors
- Label: Proper font assignment, consistent text colors, no background
- ListBox: Flat border, item colors, scrollbar styling
- DataGridView: Flat appearance, header styling, row colors, grid lines
- GroupBox: Flat border, title styling, background colors
- TabControl: Flat appearance, tab styling, selected tab colors
- ProgressBar: Custom colors, smooth styling
- TrackBar: Custom track and thumb styling
- NumericUpDown: Flat border, button styling, text colors
- DateTimePicker: Flat appearance, calendar colors
- TreeView: Node colors, selection styling, border
- LinkLabel: Link colors, hover states
- Panel: Background color, border styling
- SplitContainer: Splitter styling, panel colors
</action>

<acceptance_criteria>
- ApplyThemeToControl handles Button type with FlatStyle.Flat
- ApplyThemeToControl handles TextBox type with consistent border
- ApplyThemeToControl handles ComboBox type with FlatStyle.Flat
- ApplyThemeToControl handles CheckBox type with FlatStyle.Flat
- ApplyThemeToControl handles RadioButton type with FlatStyle.Flat
- ApplyThemeToControl handles Label type with proper font
- ApplyThemeToControl handles ListBox type with flat border
- ApplyThemeToControl handles DataGridView type with flat appearance
- ApplyThemeToControl handles GroupBox type with flat border
- ApplyThemeToControl handles TabControl type with flat appearance
- ApplyThemeToControl handles ProgressBar type with custom colors
- ApplyThemeToControl handles TrackBar type with custom styling
- ApplyThemeToControl handles NumericUpDown type with flat border
- ApplyThemeToControl handles DateTimePicker type with flat appearance
- ApplyThemeToControl handles TreeView type with node colors
- ApplyThemeToControl handles LinkLabel type with link colors
- ApplyThemeToControl handles Panel type with background color
- ApplyThemeToControl handles SplitContainer type with splitter styling
- All styled controls use DesignSystem.Colors constants
- All text controls use DesignSystem.Typography fonts
</acceptance_criteria>

---

### Task 2.6: Create ThemeManager for Advanced Theming Capabilities

<read_first>
- Core/DesignSystem.cs (for color and typography constants)
- UI/BaseForm.cs (for understanding existing theming patterns)
</read_first>

<action>
Create Core/ThemeManager.cs with:
- Static class ThemeManager
- Public static method: ApplyTheme(Control control) - applies theme to single control
- Public static method: ApplyThemeRecursive(Control parent) - applies theme to control and all children
- Public static method: ApplyThemeToForm(Form form) - applies theme to entire form
- Public static method: SetControlPrimaryStyle(Button button) - primary button styling with accent color
- Public static method: SetControlSecondaryStyle(Button button) - secondary button styling
- Public static method: SetControlWarningStyle(Control control) - warning state styling
- Public static method: SetControlErrorStyle(Control control) - error state styling
- Public static method: SetControlDisabledStyle(Control control) - disabled state styling
- Public static method: CreateStyledLabel(string text, float fontSize = 9.5f, FontStyle style = FontStyle.Regular)
- Public static method: CreateStyledButton(string text, ButtonStyle style = ButtonStyle.Primary)
- Public static enum: ButtonStyle (Primary, Secondary, Warning, Danger)
- Private static method: ApplyButtonTheme(Button button, ButtonStyle style)
- Private static method: ApplyTextControlTheme(TextBoxBase control)
- Private static method: ApplySelectionControlTheme(ListControl control)
- Event handler subscriptions for hover states on buttons
</action>

<acceptance_criteria>
- Core/ThemeManager.cs exists
- class ThemeManager is static
- contains ApplyTheme(Control) method
- contains ApplyThemeRecursive(Control) method
- contains ApplyThemeToForm(Form) method
- contains SetControlPrimaryStyle(Button) method
- contains SetControlSecondaryStyle(Button) method
- contains SetControlWarningStyle(Control) method
- contains SetControlErrorStyle(Control) method
- contains SetControlDisabledStyle(Control) method
- contains CreateStyledLabel(string, float, FontStyle) method
- contains CreateStyledButton(string, ButtonStyle) method
- ButtonStyle enum exists with Primary, Secondary, Warning, Danger values
- SetControlPrimaryStyle applies DesignSystem.Colors.Accent background
- SetControlWarningStyle applies DesignSystem.Colors.Warning styling
- SetControlErrorStyle applies DesignSystem.Colors.Error styling
- SetControlDisabledStyle applies DesignSystem.Colors.TextDisabled styling
- CreateStyledLabel returns Label with proper font and colors
- CreateStyledButton returns Button with proper styling based on ButtonStyle
- ApplyThemeRecursive applies theme to all child controls recursively
</acceptance_criteria>

---

### Task 2.7: Add Spacing and Layout Constants to DesignSystem

<read_first>
- Core/DesignSystem.cs (for existing structure)
</read_first>

<action>
Update Core/DesignSystem.cs to add:
- Nested static class Spacing with:
  - Tiny: 4
  - Small: 8
  - Medium: 12
  - Large: 16
  - XLarge: 24
  - XXLarge: 32
  - ControlPadding: 6
  - FormPadding: 16
  - GroupPadding: 12
- Nested static class Layout with:
  - DefaultLabelWidth: 100
  - DefaultControlWidth: 200
  - DefaultButtonWidth: 120
  - DefaultButtonHeight: 32
  - DefaultRowHeight: 28
  - MinimumFormWidth: 600
  - MinimumFormHeight: 400
  - DefaultFormWidth: 800
  - DefaultFormHeight: 600
</action>

<acceptance_criteria>
- Core/DesignSystem.cs contains nested static class Spacing
- DesignSystem.Spacing.Tiny equals 4
- DesignSystem.Spacing.Small equals 8
- DesignSystem.Spacing.Medium equals 12
- DesignSystem.Spacing.Large equals 16
- DesignSystem.Spacing.XLarge equals 24
- DesignSystem.Spacing.XXLarge equals 32
- DesignSystem.Spacing.ControlPadding equals 6
- DesignSystem.Spacing.FormPadding equals 16
- DesignSystem.Spacing.GroupPadding equals 12
- DesignSystem.Spacing.DefaultLabelWidth equals 100
- Core/DesignSystem.cs contains nested static class Layout
- DesignSystem.Layout.DefaultControlWidth equals 200
- DesignSystem.Layout.DefaultButtonWidth equals 120
- DesignSystem.Layout.DefaultButtonHeight equals 32
- DesignSystem.Layout.DefaultRowHeight equals 28
- DesignSystem.Layout.MinimumFormWidth equals 600
- DesignSystem.Layout.MinimumFormHeight equals 400
- DesignSystem.Layout.DefaultFormWidth equals 800
- DesignSystem.Layout.DefaultFormHeight equals 600
- All spacing constants are static readonly int
- All layout constants are static readonly int
</acceptance_criteria>

---

## Wave 3: UI Component Styling Patterns and Utilities

### Plan: Reusable UI Components and Styling Utilities

**wave**: 3
**depends_on**: [2.5, 2.6, 2.7]
**files_modified**: [UI/Components/, Core/UIUtilities.cs, Core/DesignSystem.cs]
**autonomous**: true

### Task 2.8: Create Reusable UI Component Classes

<read_first>
- Core/DesignSystem.cs (for design constants)
- Core/ThemeManager.cs (for theming methods)
- UI/BaseForm.cs (for form patterns)
</read_first>

<action>
Create UI/Components/ directory with reusable component classes:
- UI/Components/StyledLabel.cs with:
  - Constructor taking text, fontSize, fontStyle, foreColor parameters
  - AutoSize property set to true
  - Font assigned from DesignSystem.Typography
  - ForeColor configurable with default to TextPrimary
- UI/Components/StyledButton.cs with:
  - Constructor taking text, ButtonStyle, width, height parameters
  - FlatStyle.Flat applied
  - ThemeManager styling based on ButtonStyle
  - Consistent sizing and padding
  - Hover state handling
- UI/Components/StyledTextBox.cs with:
  - Constructor taking placeholderText, width, isPassword parameters
  - Flat border styling
  - Font from DesignSystem.Typography
  - Placeholder text handling
  - Focus state styling
- UI/Components/StyledComboBox.cs with:
  - Constructor taking items, width, selectedIndex parameters
  - FlatStyle.Flat applied
  - Font from DesignSystem.Typography
  - Custom dropdown styling
  - Item colors and hover states
- UI/Components/StyledGroupBox.cs with:
  - Constructor taking title, padding parameters
  - Flat border styling
  - Title font and color
  - Background color from DesignSystem.Colors
  - Proper padding application
</action>

<acceptance_criteria>
- UI/Components/ directory exists
- UI/Components/StyledLabel.cs exists and compiles
- UI/Components/StyledButton.cs exists and compiles
- UI/Components/StyledTextBox.cs exists and compiles
- UI/Components/StyledComboBox.cs exists and compiles
- UI/Components/StyledGroupBox.cs exists and compiles
- StyledLabel inherits from Label
- StyledButton inherits from Button
- StyledTextBox inherits from TextBox
- StyledComboBox inherits from ComboBox
- StyledGroupBox inherits from GroupBox
- StyledLabel uses DesignSystem.Typography fonts
- StyledButton uses ThemeManager styling
- StyledTextBox has placeholder text support
- StyledComboBox has flat styling
- StyledGroupBox has flat border and title
- All components apply dark mode colors
- All components are instantiable without exceptions
</acceptance_criteria>

---

### Task 2.9: Create UIUtilities Helper Class

<read_first>
- Core/DesignSystem.cs (for constants)
- Core/ThemeManager.cs (for theming)
</read_first>

<action>
Create Core/UIUtilities.cs with:
- Static class UIUtilities
- Public static method: CreateHorizontalPanel(int spacing = 0) - returns FlowLayoutPanel with FlowDirection.LeftToRight
- Public static method: CreateVerticalPanel(int spacing = 0) - returns FlowLayoutPanel with FlowDirection.TopDown
- Public static method: CreateLabelledControl(string labelText, Control control, int labelWidth = 100) - returns Panel with label and control
- Public static method: CreateButtonRow(params Button[] buttons) - returns Panel with evenly spaced buttons
- Public static method: CreateSeparator(bool horizontal = true) - returns Panel with separator line
- Public static method: ApplyStandardSpacing(Control parent, int spacing = DesignSystem.Spacing.Medium)
- Public static method: CenterControlInParent(Control control)
- Public static method: SetControlSize(Control control, int width, int height)
- Public static method: SetControlLocation(Control control, int x, int y)
- Public static method: CreateFormHeader(string title, string subtitle = "") - returns Panel with styled title and optional subtitle
- Public static method: CreateStatusPanel(string message, StatusType type = StatusType.Info) - returns Panel with status indicator
- Public static enum: StatusType (Info, Success, Warning, Error)
- Private static method: GetStatusColor(StatusType type)
</action>

<acceptance_criteria>
- Core/UIUtilities.cs exists
- class UIUtilities is static
- contains CreateHorizontalPanel(int) method
- contains CreateVerticalPanel(int) method
- contains CreateLabelledControl(string, Control, int) method
- contains CreateButtonRow(params Button[]) method
- contains CreateSeparator(bool) method
- contains ApplyStandardSpacing(Control, int) method
- contains CenterControlInParent(Control) method
- contains SetControlSize(Control, int, int) method
- contains SetControlLocation(Control, int, int) method
- contains CreateFormHeader(string, string) method
- contains CreateStatusPanel(string, StatusType) method
- StatusType enum exists with Info, Success, Warning, Error values
- CreateHorizontalPanel returns FlowLayoutPanel with FlowDirection.LeftToRight
- CreateVerticalPanel returns FlowLayoutPanel with FlowDirection.TopDown
- CreateLabelledControl returns Panel with label and control side-by-side
- CreateButtonRow returns Panel with buttons evenly spaced
- CreateSeparator returns Panel with line drawing
- CreateFormHeader returns Panel with styled title text
- CreateStatusPanel returns Panel with appropriate status color
- ApplyStandardSpacing applies spacing to child controls
- CenterControlInParent centers control in parent
- All utility methods return non-null controls
</acceptance_criteria>

---

### Task 2.10: Create Design System Documentation and Style Guide

<read_first>
- Core/DesignSystem.cs (for all constants and methods)
- Core/FontManager.cs (for font usage)
- Core/ThemeManager.cs (for theming patterns)
- Core/UIUtilities.cs (for utility patterns)
- UI/Components/ (for component patterns)
</read_first>

<action>
Create DESIGN_SYSTEM.md with comprehensive documentation:
- Overview and purpose of design system
- Color palette with hex values and usage guidelines
- Typography system with font families, sizes, and use cases
- Spacing system with values and application patterns
- Layout system with standard dimensions and patterns
- Component library with usage examples for each styled component
- Theming guidelines and best practices
- Control styling patterns for all WinForms control types
- Utility method reference with examples
- Code examples for common UI patterns
- Dark mode implementation details
- Accessibility considerations
- Performance notes and recommendations
</action>

<acceptance_criteria>
- DESIGN_SYSTEM.md exists in project root
- file is valid markdown format
- contains section "Color Palette" with all 12 colors listed
- contains section "Typography" with font family and size information
- contains section "Spacing System" with all spacing constants
- contains section "Layout System" with all layout constants
- contains section "Component Library" documenting all styled components
- contains section "Theming Guidelines" with usage examples
- contains section "Control Styling Patterns" with control type coverage
- contains section "Utility Methods" with method signatures and examples
- contains code examples for StyledLabel usage
- contains code examples for StyledButton usage
- contains code examples for UIUtilities usage
- documents dark mode implementation approach
- includes accessibility guidelines
- includes performance recommendations
- file is well-organized and readable
- all code examples are syntactically correct
</acceptance_criteria>

---

## Verification Criteria

### Font System Verification
- [ ] All 4 font files are present in Resources/ directory
- [ ] Font files are valid TrueType format (not corrupted)
- [ ] FontManager.Initialize() loads all fonts without exceptions
- [ ] FontManager.GetJetBrainsMonoFont() returns JetBrains Mono font when available
- [ ] FontManager.GetInterFont() returns Inter font when available
- [ ] Fallback fonts (Consolas, Segoe UI) work when embedded fonts fail
- [ ] Fonts can be loaded from embedded resources in single-file deployment
- [ ] DesignSystem.Typography provides convenient font access methods
- [ ] Font sizes are appropriate for UI elements (9.0f - 12.0f range)

### Theming System Verification
- [ ] BaseForm.ApplyThemeToControl() handles all 17+ WinForms control types
- [ ] All controls receive FlatStyle.Flat where applicable
- [ ] All controls use DesignSystem.Colors constants for consistent coloring
- [ ] All text controls use DesignSystem.Typography fonts
- [ ] ThemeManager provides all required styling methods
- [ ] ThemeManager.ApplyThemeRecursive() applies theme to entire control tree
- [ ] ButtonStyle enum provides Primary, Secondary, Warning, Danger options
- [ ] Status control methods (warning, error, disabled) apply correct colors
- [ ] Hover states work correctly on buttons
- [ ] Focus states work correctly on text controls

### Component Library Verification
- [ ] All 5 styled components exist and compile without errors
- [ ] StyledLabel applies correct font and colors
- [ ] StyledButton applies correct styling based on ButtonStyle
- [ ] StyledTextBox supports placeholder text and focus styling
- [ ] StyledComboBox has flat styling and custom dropdown appearance
- [ ] StyledGroupBox has flat border and proper title styling
- [ ] All components inherit from appropriate WinForms base classes
- [ ] All components are reusable without requiring additional setup

### Utility System Verification
- [ ] UIUtilities provides all 10+ utility methods
- [ ] CreateHorizontalPanel() returns FlowLayoutPanel with correct direction
- [ ] CreateVerticalPanel() returns FlowLayoutPanel with correct direction
- [ ] CreateLabelledControl() creates properly aligned label-control pairs
- [ ] CreateButtonRow() creates evenly spaced button layouts
- [ ] CreateSeparator() creates visible separator lines
- [ ] CreateFormHeader() creates styled header panels
- [ ] CreateStatusPanel() creates status indicators with correct colors
- [ ] Layout utilities (CenterControlInParent, SetControlSize, SetControlLocation) work correctly
- [ ] ApplyStandardSpacing() applies consistent spacing to control collections

### Design System Integration Verification
- [ ] DesignSystem.Colors contains all 12 required color constants
- [ ] DesignSystem.Typography contains font constants and helper methods
- [ ] DesignSystem.Spacing contains all 10+ spacing constants
- [ ] DesignSystem.Layout contains all 10+ layout constants
- [ ] All constants are static readonly and follow naming conventions
- [ ] DesignSystem provides convenient access to all design tokens
- [ ] ThemeManager integrates with DesignSystem constants
- [ ] FontManager integrates with DesignSystem.Typography
- [ ] UIUtilities uses DesignSystem constants for consistency

### Documentation Verification
- [ ] DESIGN_SYSTEM.md exists and is comprehensive
- [ ] Documentation covers all color constants with usage guidelines
- [ ] Documentation covers all typography settings with use cases
- [ ] Documentation covers all spacing and layout constants
- [ ] Documentation covers all styled components with examples
- [ ] Documentation covers all utility methods with signatures
- [ ] Code examples are syntactically correct and runnable
- [ ] Dark mode implementation is documented
- [ ] Accessibility considerations are included
- [ ] Performance recommendations are provided

### Build and Integration Verification
- [ ] All new code compiles without errors or warnings
- [ ] Font files are properly embedded as resources
- [ ] Application builds successfully with single-file configuration
- [ ] No NuGet packages are added (maintains zero-dependency policy)
- [ ] Design system integrates with existing Phase 1 code (BaseForm, DesignSystem)
- [ ] FontManager.Initialize() can be called from Program.cs without issues
- [ ] ThemeManager can be used in any form without additional setup
- [ ] Styled components can be instantiated and added to forms

## must_haves

- All 4 font files (JetBrains Mono and Inter, regular and bold weights) embedded as resources
- FontManager class with PrivateFontCollection implementation and fallback fonts
- DesignSystem.Typography with font constants and helper methods
- BaseForm.ApplyThemeToControl() extended to handle 17+ WinForms control types
- ThemeManager class with comprehensive theming methods and utility functions
- DesignSystem.Spacing and DesignSystem.Layout constant classes
- 5 reusable styled components (Label, Button, TextBox, ComboBox, GroupBox)
- UIUtilities class with 10+ helper methods for common UI patterns
- Comprehensive DESIGN_SYSTEM.md documentation
- Complete dark mode styling for all control types
- Flat design aesthetic (no 3D effects, 1px borders)
- Consistent color application using DesignSystem.Colors constants
- Consistent font application using DesignSystem.Typography methods
- Integration with Phase 1 BaseForm and DesignSystem foundation
- Zero additional NuGet dependencies
- Single-file deployment compatibility maintained

## Artifacts this Phase Produces

### Files Created
- Resources/JetBrainsMono-Regular.ttf - JetBrains Mono regular weight font
- Resources/JetBrainsMono-Bold.ttf - JetBrains Mono bold weight font
- Resources/Inter-Regular.ttf - Inter regular weight font
- Resources/Inter-Bold.ttf - Inter bold weight font
- Core/FontManager.cs - Font loading and management system
- Core/ThemeManager.cs - Advanced theming capabilities
- Core/UIUtilities.cs - UI helper methods and utilities
- UI/Components/StyledLabel.cs - Styled label component
- UI/Components/StyledButton.cs - Styled button component
- UI/Components/StyledTextBox.cs - Styled text box component
- UI/Components/StyledComboBox.cs - Styled combo box component
- UI/Components/StyledGroupBox.cs - Styled group box component
- DESIGN_SYSTEM.md - Comprehensive design system documentation

### Files Modified
- BetriebsmittelPublisher.csproj - Added embedded resource entries for font files
- Core/DesignSystem.cs - Added Typography, Spacing, and Layout nested classes
- UI/BaseForm.cs - Extended ApplyThemeToControl() for complete control type coverage

### Directories Created
- UI/Components/ - Reusable UI component library

### Symbols Created
- `BetriebsmittelPublisher.Core.FontManager` - Font management class
- `BetriebsmittelPublisher.Core.FontManager.Initialize()` - Font initialization method
- `BetriebsmittelPublisher.Core.FontManager.GetJetBrainsMonoFont(float, FontStyle)` - Mono font getter
- `BetriebsmittelPublisher.Core.FontManager.GetInterFont(float, FontStyle)` - Sans font getter
- `BetriebsmittelPublisher.Core.FontManager.JetBrainsMonoFamily` - JetBrains Mono FontFamily property
- `BetriebsmittelPublisher.Core.FontManager.InterFamily` - Inter FontFamily property
- `BetriebsmittelPublisher.Core.DesignSystem.Typography` - Typography constants class
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.DefaultMonoFontSize` - Default mono font size
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.DefaultSansFontSize` - Default sans font size
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.HeadingFontSize` - Heading font size
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.MonoFontFamily` - Mono font family reference
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.SansFontFamily` - Sans font family reference
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.GetMonoFont(float, FontStyle)` - Mono font helper
- `BetriebsmittelPublisher.Core.DesignSystem.Typography.GetSansFont(float, FontStyle)` - Sans font helper
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing` - Spacing constants class
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.Tiny` - 4px spacing constant
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.Small` - 8px spacing constant
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.Medium` - 12px spacing constant
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.Large` - 16px spacing constant
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.XLarge` - 24px spacing constant
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.XXLarge` - 32px spacing constant
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.ControlPadding` - 6px control padding
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.FormPadding` - 16px form padding
- `BetriebsmittelPublisher.Core.DesignSystem.Spacing.GroupPadding` - 12px group padding
- `BetriebsmittelPublisher.Core.DesignSystem.Layout` - Layout constants class
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultLabelWidth` - 100px label width
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultControlWidth` - 200px control width
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultButtonWidth` - 120px button width
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultButtonHeight` - 32px button height
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultRowHeight` - 28px row height
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.MinimumFormWidth` - 600px minimum form width
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.MinimumFormHeight` - 400px minimum form height
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultFormWidth` - 800px default form width
- `BetriebsmittelPublisher.Core.DesignSystem.Layout.DefaultFormHeight` - 600px default form height
- `BetriebsmittelPublisher.Core.ThemeManager` - Theme management class
- `BetriebsmittelPublisher.Core.ThemeManager.ApplyTheme(Control)` - Single control theming
- `BetriebsmittelPublisher.Core.ThemeManager.ApplyThemeRecursive(Control)` - Recursive theming
- `BetriebsmittelPublisher.Core.ThemeManager.ApplyThemeToForm(Form)` - Form-level theming
- `BetriebsmittelPublisher.Core.ThemeManager.SetControlPrimaryStyle(Button)` - Primary button style
- `BetriebsmittelPublisher.Core.ThemeManager.SetControlSecondaryStyle(Button)` - Secondary button style
- `BetriebsmittelPublisher.Core.ThemeManager.SetControlWarningStyle(Control)` - Warning style
- `BetriebsmittelPublisher.Core.ThemeManager.SetControlErrorStyle(Control)` - Error style
- `BetriebsmittelPublisher.Core.ThemeManager.SetControlDisabledStyle(Control)` - Disabled style
- `BetriebsmittelPublisher.Core.ThemeManager.CreateStyledLabel(string, float, FontStyle)` - Label factory
- `BetriebsmittelPublisher.Core.ThemeManager.CreateStyledButton(string, ButtonStyle)` - Button factory
- `BetriebsmittelPublisher.Core.ThemeManager.ButtonStyle` - Button style enum (Primary, Secondary, Warning, Danger)
- `BetriebsmittelPublisher.Core.UIUtilities` - UI utilities class
- `BetriebsmittelPublisher.Core.UIUtilities.CreateHorizontalPanel(int)` - Horizontal panel factory
- `BetriebsmittelPublisher.Core.UIUtilities.CreateVerticalPanel(int)` - Vertical panel factory
- `BetriebsmittelPublisher.Core.UIUtilities.CreateLabelledControl(string, Control, int)` - Labelled control factory
- `BetriebsmittelPublisher.Core.UIUtilities.CreateButtonRow(params Button[])` - Button row factory
- `BetriebsmittelPublisher.Core.UIUtilities.CreateSeparator(bool)` - Separator factory
- `BetriebsmittelPublisher.Core.UIUtilities.ApplyStandardSpacing(Control, int)` - Spacing utility
- `BetriebsmittelPublisher.Core.UIUtilities.CenterControlInParent(Control)` - Centering utility
- `BetriebsmittelPublisher.Core.UIUtilities.SetControlSize(Control, int, int)` - Size utility
- `BetriebsmittelPublisher.Core.UIUtilities.SetControlLocation(Control, int, int)` - Location utility
- `BetriebsmittelPublisher.Core.UIUtilities.CreateFormHeader(string, string)` - Form header factory
- `BetriebsmittelPublisher.Core.UIUtilities.CreateStatusPanel(string, StatusType)` - Status panel factory
- `BetriebsmittelPublisher.Core.UIUtilities.StatusType` - Status type enum (Info, Success, Warning, Error)
- `BetriebsmittelPublisher.UI.Components.StyledLabel` - Styled label component
- `BetriebsmittelPublisher.UI.Components.StyledButton` - Styled button component
- `BetriebsmittelPublisher.UI.Components.StyledTextBox` - Styled text box component
- `BetriebsmittelPublisher.UI.Components.StyledComboBox` - Styled combo box component
- `BetriebsmittelPublisher.UI.Components.StyledGroupBox` - Styled group box component

### Design System Artifacts
- Complete font embedding system with 4 fonts and fallback support
- Comprehensive color system with 12 color constants
- Typography system with 2 font families, 3 size constants, and 2 helper methods
- Spacing system with 10 spacing constants
- Layout system with 10 layout constants
- Theming system supporting 17+ WinForms control types
- Component library with 5 reusable styled components
- Utility library with 10+ helper methods
- Complete design system documentation

### Integration Artifacts
- Extended BaseForm with complete control type coverage
- Enhanced DesignSystem with Typography, Spacing, and Layout
- Seamless integration with Phase 1 foundation
- Zero additional dependencies maintained
- Single-file deployment compatibility preserved
