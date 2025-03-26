# 📁 QuickNotes Plugin Structure

This document provides an overview of the structure and organization of the QuickNotes plugin for Microsoft PowerToys Run.

## 📂 Project Structure

```
QuickNotes/
├── Community.PowerToys.Run.Plugin.QuickNotes/         # Main plugin project
│   ├── Images/                                        # Plugin icons
│   │   ├── quicknotes.dark.png                        # Dark theme icon
│   │   └── quicknotes.light.png                       # Light theme icon
│   ├── Community.PowerToys.Run.Plugin.QuickNotes.csproj  # Project file
│   ├── Main.cs                                        # Main plugin implementation
│   └── plugin.json                                    # Plugin metadata and configuration
│
├── Community.PowerToys.Run.Plugin.QuickNotes.UnitTests/  # Unit tests project
│   ├── Community.PowerToys.Run.Plugin.QuickNotes.UnitTests.csproj  # Test project file
│   └── MainTests.cs                                   # Test implementation
│
├── QuickNotes.sln                                     # Solution file
├── Release.zip                                        # Release package
├── dotnet-install.sh                                  # .NET installation script
└── scr/                                               # Additional resources
```

## 🔍 Key Components

### 1. Main Plugin Project

The `Community.PowerToys.Run.Plugin.QuickNotes` project contains the core functionality of the plugin:

- **Main.cs**: The primary implementation file that contains:
  - Plugin initialization logic
  - Command handling (add, edit, delete, search notes)
  - Context menu functionality
  - File operations for note storage
  - User interface interactions

- **plugin.json**: Configuration file that defines:
  - Plugin ID: `2083308C581F4D36B0C02E69A2FD91D7`
  - Action keyword: `qq`
  - Plugin metadata (name, author, version)
  - Icon paths for different themes

- **Images/**: Contains icons for the plugin in both light and dark themes

### 2. Unit Tests Project

The `Community.PowerToys.Run.Plugin.QuickNotes.UnitTests` project contains tests to verify the plugin's functionality:

- **MainTests.cs**: Contains unit tests for:
  - Query functionality
  - Context menu loading

### 3. Project Configuration

- **Community.PowerToys.Run.Plugin.QuickNotes.csproj**:
  - Targets .NET 9.0 for Windows 10.0.22621.0
  - Supports both x64 and ARM64 architectures
  - References Community.PowerToys.Run.Plugin.Dependencies (v0.89.0)
  - Configures build settings and output paths

- **Community.PowerToys.Run.Plugin.QuickNotes.UnitTests.csproj**:
  - Targets the same framework as the main project
  - References MSTest (v3.6.3) for testing
  - References System.IO.Abstractions (v21.0.29) for file system operations

## 🔄 Plugin Workflow

1. **Initialization**:
   - Plugin initializes when PowerToys Run loads
   - Creates a notes storage directory if it doesn't exist
   - Sets up theme-specific icons

2. **Command Processing**:
   - Listens for the `qq` keyword
   - Parses user input to determine the intended action
   - Executes the appropriate function based on the command

3. **Data Storage**:
   - Notes are stored in a text file at `%LOCALAPPDATA%\Microsoft\PowerToys\QuickNotes\notes.txt`
   - Each note includes a timestamp for reference
   - File operations are handled with appropriate error checking

## 🧪 Testing Approach

The unit tests verify:
- That query results are properly returned
- That context menus are correctly loaded

## 🛠️ Build Configuration

The plugin is configured to:
- Target Windows 10.0.22621.0
- Support both x64 and ARM64 architectures
- Use WPF for UI components
- Handle both light and dark themes

## 📦 Dependencies

- **Community.PowerToys.Run.Plugin.Dependencies**: Core dependencies for PowerToys Run plugins
- **MSTest**: Testing framework
- **System.IO.Abstractions**: Abstraction layer for file system operations

## 🔌 Integration with PowerToys Run

The plugin integrates with PowerToys Run through:
- Implementation of the `IPlugin` interface for core functionality
- Implementation of the `IContextMenu` interface for right-click menu options
- Implementation of the `IPluginI18n` interface for internationalization support
- Implementation of the `IDisposable` interface for proper resource cleanup
