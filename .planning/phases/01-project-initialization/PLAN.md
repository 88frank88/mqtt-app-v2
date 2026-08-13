# PLAN.md: Phase 1 - Project Initialization

## Wave 1: Foundation Setup

### Plan: Project Structure and Build Configuration

**wave**: 1
**depends_on**: []
**files_modified**: [BetriebsmittelPublisher.csproj, nuget.config]
**autonomous**: true

## Tasks

### Task 1.1: Initialize .NET 10 WinForms Project with .csproj Configuration

<read_first>
- No existing files to read
</read_first>

<action>
Create BetriebsmittelPublisher.csproj file with:
- OutputType: WinExe
- TargetFramework: net10.0-windows
- UseWindowsForms: true
- PublishSingleFile: true
- SelfContained: true
- RuntimeIdentifier: win-x64
- EnableCompressionInSingleFile: true
- IncludeNativeLibrariesForSelfExtract: true
- ImplicitUsings: enable
- Nullable: enable
</action>

<acceptance_criteria>
- BetriebsmittelPublisher.csproj exists and is valid XML
- file contains `<OutputType>WinExe</OutputType>`
- file contains `<TargetFramework>net10.0-windows</TargetFramework>`
- file contains `<UseWindowsForms>true</UseWindowsForms>`
- file contains `<PublishSingleFile>true</PublishSingleFile>`
- file contains `<SelfContained>true</SelfContained>`
- file contains `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`
- file contains `<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>`
- file contains `<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>`
- `dotnet build` succeeds without errors
- `dotnet publish -c Release -r win-x64` succeeds without errors
- Output directory contains single .exe file (no DLLs)
</acceptance_criteria>

---

### Task 1.2: Create Custom NuGet.Config with Zero Dependencies

<read_first>
- No existing files to read
</read_first>

<action>
Create nuget.config file in project root with:
- XML declaration: `<?xml version="1.0" encoding="utf-8"?>`
- `<configuration>` root element
- `<packageSources>` section with `<clear />` element
- `<disabledPackageSources>` section with `<clear />` element
</action>

<acceptance_criteria>
- nuget.config exists in project root
- file is valid XML format
- file contains `<packageSources>` section
- file contains `<clear />` within packageSources
- file contains `<disabledPackageSources>` section
- file contains `<clear />` within disabledPackageSources
- `dotnet restore --no-cache --no-dependencies` succeeds
- No package downloads occur during restore
- No NuGet package references exist in .csproj
</acceptance_criteria>

---

### Task 1.3: Establish Project Directory Structure

<read_first>
- No existing files to read
</read_first>

<action>
Create directory structure:
- Core/ (for core application classes)
- UI/ (for forms and UI components)
- Services/ (for business logic services)
- Resources/ (for embedded resources like fonts)
- Models/ (for data models)
</action>

<acceptance_criteria>
- Core/ directory exists
- UI/ directory exists
- Services/ directory exists
- Resources/ directory exists
- Models/ directory exists
- All directories are empty or contain only .gitkeep files
- Directory structure matches .NET project conventions
</acceptance_criteria>

---

### Task 1.4: Create Design System Color Constants

<read_first>
- No existing files to read
</read_first>

<action>
Create Core/DesignSystem.cs with:
- Static class DesignSystem
- Nested static class Colors with:
  - Background: Color.FromArgb(0x1a, 0x1d, 0x29)
  - Accent: Color.FromArgb(0xff, 0x5c, 0x5c)
  - Secondary: Color.FromArgb(0x5b, 0x64, 0x78)
  - TextPrimary: Color.FromArgb(0xff, 0xff, 0xff)
  - TextSecondary: Color.FromArgb(0xcc, 0xcc, 0xcc)
  - TextDisabled: Color.FromArgb(0x88, 0x88, 0x88)
  - ControlBackground: Color.FromArgb(0x22, 0x25, 0x33)
  - ControlBorder: Color.FromArgb(0x3a, 0x3e, 0x50)
  - ControlHover: Color.FromArgb(0x2a, 0x2e, 0x3d)
  - Success: Color.FromArgb(0x4a, 0x7c, 0x59)
  - Warning: Color.FromArgb(0xc9, 0x8b, 0x35)
  - Error: Color.FromArgb(0xd9, 0x5c, 0x5c)
</action>

<acceptance_criteria>
- Core/DesignSystem.cs exists
- class is static named DesignSystem
- contains nested static class Colors
- Colors.Background equals Color.FromArgb(0x1a, 0x1d, 0x29)
- Colors.Accent equals Color.FromArgb(0xff, 0x5c, 0x5c)
- Colors.Secondary equals Color.FromArgb(0x5b, 0x64, 0x78)
- Colors.TextPrimary equals Color.FromArgb(0xff, 0xff, 0xff)
- Colors.TextSecondary equals Color.FromArgb(0xcc, 0xcc, 0xcc)
- Colors.TextDisabled equals Color.FromArgb(0x88, 0x88, 0x88)
- Colors.ControlBackground equals Color.FromArgb(0x22, 0x25, 0x33)
- Colors.ControlBorder equals Color.FromArgb(0x3a, 0x3e, 0x50)
- Colors.ControlHover equals Color.FromArgb(0x2a, 0x2e, 0x3d)
- Colors.Success equals Color.FromArgb(0x4a, 0x7c, 0x59)
- Colors.Warning equals Color.FromArgb(0xc9, 0x8b, 0x35)
- Colors.Error equals Color.FromArgb(0xd9, 0x5c, 0x5c)
- All colors are readonly static fields
</acceptance_criteria>

---

### Task 1.5: Create Base Form Class with Dark Mode Support

<read_first>
- Core/DesignSystem.cs (for color constants)
</read_first>

<action>
Create UI/BaseForm.cs with:
- Class BaseForm inheriting from Form
- Constructor calls ApplyDarkModeTheme()
- Override OnPaintBackground with custom background painting using DesignSystem.Colors.Background
- ApplyDarkModeTheme() method sets:
  - BackColor to DesignSystem.Colors.Background
  - ForeColor to DesignSystem.Colors.TextPrimary
  - FormBorderStyle to Sizable
  - StartPosition to CenterScreen
- Private method ApplyThemeToControls(Control.ControlCollection controls) for recursive theme application
- Private method ApplyThemeToControl(Control control) applying basic theming
</action>

<acceptance_criteria>
- UI/BaseForm.cs exists
- class BaseForm inherits from Form
- constructor calls ApplyDarkModeTheme()
- OnPaintBackground is overridden
- ApplyDarkModeTheme sets BackColor to DesignSystem.Colors.Background
- ApplyDarkModeTheme sets ForeColor to DesignSystem.Colors.TextPrimary
- FormBorderStyle is set to Sizable
- StartPosition is set to CenterScreen
- ApplyThemeToControls method exists
- ApplyThemeToControl method exists
- BaseForm compiles without errors
</acceptance_criteria>

---

### Task 1.6: Create Main Application Form Shell

<read_first>
- UI/BaseForm.cs (for base form inheritance)
- Core/DesignSystem.cs (for color constants)
</read_first>

<action>
Create UI/MainForm.cs with:
- Class MainForm inheriting from BaseForm
- Constructor sets:
  - Text to "Betriebsmittel Publisher"
  - Size to new Size(800, 600)
  - MinimumSize to new Size(600, 400)
- Empty Load event handler (for future expansion)
</action>

<acceptance_criteria>
- UI/MainForm.cs exists
- class MainForm inherits from BaseForm
- Text property equals "Betriebsmittel Publisher"
- Size equals new Size(800, 600)
- MinimumSize equals new Size(600, 400)
- MainForm compiles without errors
- MainForm can be instantiated without exceptions
</acceptance_criteria>

---

### Task 1.7: Create Program Entry Point

<read_first>
- UI/MainForm.cs (for main form instantiation)
</read_first>

<action>
Create Program.cs with:
- static class Program
- [STAThread] attribute on Main method
- Main method calls:
  - Application.EnableVisualStyles()
  - Application.SetCompatibleTextRenderingDefault(false)
  - Application.Run(new MainForm())
</action>

<acceptance_criteria>
- Program.cs exists
- class Program is static
- Main method has [STAThread] attribute
- Main method calls Application.EnableVisualStyles()
- Main method calls Application.SetCompatibleTextRenderingDefault(false)
- Main method calls Application.Run(new MainForm())
- Program compiles without errors
- Application entry point is correctly configured
</acceptance_criteria>

## Verification Criteria

### Build Verification
- [ ] `dotnet build -c Release` succeeds without errors or warnings
- [ ] `dotnet publish -c Release -r win-x64 --no-restore` succeeds without errors
- [ ] Single .exe file is generated in publish directory
- [ ] No DLL files are present in publish directory
- [ ] .exe file size is approximately 100MB (due to embedded .NET runtime)

### Dependency Verification
- [ ] NuGet.Config contains `<clear />` in packageSources section
- [ ] NuGet.Config contains `<clear />` in disabledPackageSources section
- [ ] `dotnet restore` shows zero packages restored
- [ ] .csproj contains no PackageReference elements
- [ ] Build completes without internet connection

### Application Verification
- [ ] Compiled .exe launches successfully
- [ ] Application window displays with title "Betriebsmittel Publisher"
- [ ] Window shows dark mode background (#1a1d29)
- [ ] Window is resizable and minimizable
- [ ] No console window appears during execution
- [ ] Application closes cleanly without errors

### Code Quality Verification
- [ ] All classes use appropriate access modifiers
- [ ] All fields are readonly where appropriate
- [ ] Code follows C# naming conventions
- [ ] No compiler warnings
- [ ] All using statements are necessary
- [ ] No TODO comments remain

## must_haves

- Valid .csproj file with correct target framework and build settings
- Custom NuGet.Config with `<clear />` for zero dependencies
- Single-file deployment configuration verified
- Complete project directory structure
- DesignSystem class with all required color constants
- BaseForm class with dark mode support
- MainForm shell application
- Program entry point
- Successful build producing single .exe
- Application launches and displays window
- Dark mode theme applied visually
- Zero NuGet package dependencies
- Offline build capability

## Artifacts this Phase Produces

### Files Created
- BetriebsmittelPublisher.csproj - Project file with build configuration
- nuget.config - Custom NuGet configuration with zero dependencies
- Program.cs - Application entry point
- Core/DesignSystem.cs - Color constants and design system
- UI/BaseForm.cs - Base form class with dark mode support
- UI/MainForm.cs - Main application form shell

### Directories Created
- Core/ - Core application classes
- UI/ - Forms and UI components
- Services/ - Business logic services
- Resources/ - Embedded resources
- Models/ - Data models

### Symbols Created
- `BetriebsmittelPublisher.Program` - Static entry point class
- `BetriebsmittelPublisher.Program.Main()` - Application entry method
- `BetriebsmittelPublisher.Core.DesignSystem` - Design system constants class
- `BetriebsmittelPublisher.Core.DesignSystem.Colors` - Color constants class
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.Background` - Background color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.Accent` - Accent color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.Secondary` - Secondary color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.TextPrimary` - Primary text color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.TextSecondary` - Secondary text color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.TextDisabled` - Disabled text color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.ControlBackground` - Control background color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.ControlBorder` - Control border color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.ControlHover` - Control hover color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.Success` - Success color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.Warning` - Warning color constant
- `BetriebsmittelPublisher.Core.DesignSystem.Colors.Error` - Error color constant
- `BetriebsmittelPublisher.UI.BaseForm` - Base form class with dark mode
- `BetriebsmittelPublisher.UI.BaseForm()` - Constructor
- `BetriebsmittelPublisher.UI.BaseForm.ApplyDarkModeTheme()` - Theme application method
- `BetriebsmittelPublisher.UI.BaseForm.OnPaintBackground()` - Custom background painting
- `BetriebsmittelPublisher.UI.BaseForm.ApplyThemeToControls()` - Recursive control theming
- `BetriebsmittelPublisher.UI.BaseForm.ApplyThemeToControl()` - Individual control theming
- `BetriebsmittelPublisher.UI.MainForm` - Main application form
- `BetriebsmittelPublisher.UI.MainForm()` - Constructor

### Build Artifacts
- Single .exe file in bin/Release/net10.0-windows/win-x64/publish/
- Self-contained .NET runtime embedded in executable
- Compressed single-file deployment package