# 📝 PowerToys Run: QuickNotes Plugin

<div align="center">
  <img src="assets/demo.gif" alt="QuickNotes Demo" style="width:100%;max-width:700px;">
  <img src="assets/quicknotes.dark.png" alt="QuickNotes Icon" width="100" height="100">
  
  <h2>Create, manage, and search notes directly from PowerToys Run</h2>
  
  <div>
    <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.9/QuickNotes-1.0.9-x64.zip">
      <img src="https://img.shields.io/badge/⬇_Download_x64-0078D7?style=for-the-badge&logo=windows&logoColor=white" alt="Download x64">
    </a>
    <div>
     ![PowerToys Compatible](https://img.shields.io/badge/PowerToys-Compatible-blue)
  ![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  ![Maintenance](https://img.shields.io/maintenance/yes/2025)
  [![Build Status](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/actions)
  ![C#](https://img.shields.io/badge/C%23-.NET-512BD4)
  ![Version](https://img.shields.io/badge/version-1.0.9-brightgreen)
  ![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)
  [![GitHub stars](https://img.shields.io/github/stars/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/stargazers)
  [![GitHub issues](https://img.shields.io/github/issues/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/issues)
  [![GitHub release (latest by date)](https://img.shields.io/github/v/release/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes?label=latest)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/latest)
  [![GitHub all releases](https://img.shields.io/github/downloads/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/total)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases)
  ![Made with Love](https://img.shields.io/badge/Made%20with-❤️-red)
  ![Awesome](https://img.shields.io/badge/Awesome-Yes-orange)
  <a href="https://github.com/hlaueriksson/awesome-powertoys-run-plugins">
    <img src="https://awesome.re/mentioned-badge.svg" alt="Mentioned in Awesome PowerToys Run Plugins">
</div>

<details>
<summary>SHA256 Checksums</summary>

```text
# Checksums will be updated after v1.0.9 release
# QuickNotes-1.0.9-x64.zip
# QuickNotes-1.0.9-arm64.zip
```
</details>

> 🚀 **v1.0.9**: Improved multi-line notes with better code snippet support. [Full changelog](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/blob/main/Release.md/CHANGELOG.md)
>
> 🔄 **v1.0.8**: PowerToys Run plugin validation compliance, optimized dependencies
>
> 🔄 **v1.0.7**: Enhanced note deletion with better confirmation dialogs and ID-based identification
>
> 📝 **v1.0.6**: Reworked note management system fixing critical bugs with deletion and editing

## 📋 Overview

QuickNotes is a plugin for [Microsoft PowerToys Run](https://github.com/microsoft/PowerToys) that allows you to quickly create, manage, and search notes directly from your PowerToys Run interface. Simply type `qq` followed by your note text to save it, or use various commands to manage your notes collection.

## 📚 Documentation

For detailed documentation, visit the [QuickNotes Wiki](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/wiki).

## ⚡ Installation

### Quick Install

1. Download the [x64](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.9/QuickNotes-1.0.9-x64.zip) or [ARM64](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.9/QuickNotes-1.0.9-arm64.zip) version
2. Extract to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\`
3. Restart PowerToys
4. Start using with `Alt+Space` then type `qq`

### PowerShell Installation

```powershell
# Download and install the latest version (x64)
$url = "https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.9/QuickNotes-1.0.9-x64.zip"
$pluginPath = "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\QuickNotes"
New-Item -ItemType Directory -Force -Path $pluginPath | Out-Null
Invoke-WebRequest -Uri $url -OutFile "$env:TEMP\QuickNotes.zip"
Expand-Archive -Path "$env:TEMP\QuickNotes.zip" -DestinationPath $pluginPath -Force
Remove-Item "$env:TEMP\QuickNotes.zip"
Write-Host "QuickNotes plugin has been installed. Please restart PowerToys." -ForegroundColor Green
```

## ✨ Features

- 📝 **Quick Note Creation** - Instantly save notes with a simple command
- 🔍 **Powerful Search** - Find notes with highlighted search terms
- 🏷️ **Tag Support** - Add #tags to notes and search by tag
- 📌 **Pin Important Notes** - Pin critical notes to keep them at the top
- ✨ **Full Markdown Support** - Format with headers, code blocks, lists, and more
- 📝 **Multi-Line Notes** - Rich editor with live preview for longer notes
- 📋 **Clipboard Integration** - Copy notes with a single click
- 🔄 **Undo Delete** - Restore recently deleted notes
- 💾 **Simple Backup** - Create backups of your notes collection

## 🔧 Usage

Open PowerToys Run (default: <kbd>Alt</kbd> + <kbd>Space</kbd>) and use these commands:

| Command | Description |
|---------|-------------|
| `qq <text>` | Create a new note |
| `qq help` | Show help information |
| `qq search <term>` | Search notes with highlighted matches |
| `qq searchtag <tag>` | Search notes by tag |
| `qq view <number>` | View note details |
| `qq edit <number>` | Edit a specific note |
| `qq del <number>` | Delete a specific note |
| `qq delall` | Delete all notes |
| `qq undo` | Restore last deleted note |
| `qq pin <number>` | Pin a note to the top |
| `qq unpin <number>` | Unpin a note |
| `qq sort date` | Sort notes by date |
| `qq sort alpha` | Sort notes alphabetically |
| `qq backup` | Backup notes |
| `qq markdown` | Create multi-line markdown note |
### 👉 Quick Tips

- Press <kbd>Enter</kbd> on a note to copy it to clipboard
- Right-click for more options (copy, edit, delete, pin/unpin)
- Add #tags to notes: `qq Meeting with John #work #important`
- Format with Markdown:
  - **Bold**: `**text**` or `__text__`
  - *Italic*: `*text*` or `_text_`
  - `Code`: `` `code` ``
  - Headers: `# Heading`
  - Lists: `- item` or `1. item`
- Use <kbd>Ctrl</kbd>+<kbd>C</kbd> to copy with timestamp
- Type `qq` then press <kbd>Tab</kbd> for command suggestions
- URLs in notes are automatically detected and clickable

## 🎬 Demo

<div align="center">
  <p><img src="assets/demo.gif" width="650" alt="QuickNotes Demo"/></p>
  <p><i>QuickNotes in action</i></p>
  
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/tree/master/assets">
    <img src="https://img.shields.io/badge/View_More_Demos-4285F4?style=for-the-badge&logo=video&logoColor=white" alt="More Demos">
  </a>
</div>

## 📁 Data Storage

QuickNotes stores all your notes in a simple text file at:
```
%LOCALAPPDATA%\Microsoft\PowerToys\QuickNotes\notes.txt
```

## 🛠️ Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET SDK
- [Microsoft PowerToys](https://github.com/microsoft/PowerToys) installed
- Windows 10 or later

### Build Steps
1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution: `dotnet build -c Release`

## 🤝 Contributing

Contributions are welcome! Please check the [Contributing Guidelines](wiki/Contributing.md) for more information.

## ❓ FAQ

<details>
  <summary><b>How do I update the plugin?</b></summary>
  <p>Download the latest release and replace the files in your PowerToys Plugins directory. Restart PowerToys afterward.</p>
</details>

<details>
  <summary><b>Can I sync my notes across devices?</b></summary>
  <p>The plugin doesn't have built-in sync, but you can place the notes.txt file in a cloud-synced folder and create a symbolic link to it.</p>
</details>

<details>
  <summary><b>What if I accidentally delete all my notes?</b></summary>
  <p>If you've created backups using the <code>qq backup</code> command, you can restore from those. Otherwise, you might be able to recover from Windows File History if enabled.</p>
</details>

<details>
  <summary><b>Can I change the storage location?</b></summary>
  <p>Currently, the storage location is fixed. A future update may add customizable storage locations.</p>
</details>

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- [Microsoft PowerToys](https://github.com/microsoft/PowerToys) team for creating the extensible PowerToys Run platform
- All contributors who have helped improve this plugin
- Icons and visual elements from various open-source projects

---

<div align="center">
  <h2>📥 Download Latest Version</h2>
  
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.9/QuickNotes-1.0.9-x64.zip">
    <img src="https://img.shields.io/badge/Download-x64_64-0078D7?style=for-the-badge&logo=windows&logoColor=white" alt="Download x64">
  </a>
  
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.9/QuickNotes-1.0.9-arm64.zip">
    <img src="https://img.shields.io/badge/Download-ARM64-0078D7?style=for-the-badge&logo=windows&logoColor=white" alt="Download ARM64">
  </a>
  
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/latest">
    <img src="https://img.shields.io/badge/View_All_Releases-181717?style=for-the-badge&logo=github&logoColor=white" alt="View All Releases">
  </a>
  
  <p>Made with ❤️ by <a href="https://github.com/ruslanlap">ruslanlap</a></p>
  
  <a href="#-powertoys-run-quicknotes-plugin">Back to top ⬆️</a>
</div>


## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- [Microsoft PowerToys](https://github.com/microsoft/PowerToys) team for creating the extensible PowerToys Run platform
- All contributors who have helped improve this plugin
- Icons and visual elements from various open-source projects

### 🧩 Notable Features

- **Timestamp Management**: Automatically adds timestamps to notes and provides options to display or hide them
- **Tag Detection**: Identifies and formats #tags with customizable styling (bold or italic)
- **URL Detection**: Uses regex to find and make URLs clickable in notes
- **Undo Functionality**: Tracks deleted notes to enable undo operations
- **Sort Capabilities**: Implements flexible sorting by date or alphabetically
- **Autocomplete**: Provides intelligent command suggestions as you type

The implementation prioritizes user experience with features like:
- Clean content copying (stripping timestamps and tags)
- Intelligent display of pinned vs. regular notes
- Comprehensive error handling
- Helpful tooltips and notifications
- Flexible search capabilities

This robust architecture makes QuickNotes not just a simple note-taking plugin, but a powerful productivity tool that seamlessly integrates with PowerToys Run.

For more detailed implementation information, see the [IMPLEMENTATION_SUMMARY.md](docs/IMPLEMENTATION_SUMMARY.md) file.

