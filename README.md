# 📝 PowerToys Run: QuickNotes Plugin

<div align="center">
  <img src="assets/demo.gif" alt="QuickNotes Demo" width="650">
  
  <img src="https://raw.githubusercontent.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/master/QuickNotes/Community.PowerToys.Run.Plugin.QuickNotes/Images/quicknotes.dark.png" alt="QuickNotes Icon" width="128" height="128">
  
  <h1>✨ QuickNotes for PowerToys Run ✨</h1>
  <h3>Create, manage, and search notes directly from PowerToys Run</h3>
  
  ![PowerToys Compatible](https://img.shields.io/badge/PowerToys-Compatible-blue)
  ![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  ![Maintenance](https://img.shields.io/maintenance/yes/2025)
  ![C#](https://img.shields.io/badge/C%23-.NET-512BD4)
  ![Version](https://img.shields.io/badge/version-1.0.3-brightgreen)
  ![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)
  [![GitHub stars](https://img.shields.io/github/stars/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/stargazers)
  [![GitHub issues](https://img.shields.io/github/issues/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/issues)
  [![GitHub release (latest by date)](https://img.shields.io/github/v/release/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/latest)
  [![GitHub all releases](https://img.shields.io/github/downloads/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/total)](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases)
  ![Made with Love](https://img.shields.io/badge/Made%20with-❤️-red)
  ![Awesome](https://img.shields.io/badge/Awesome-Yes-orange)
</div>

<div align="center">
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.3/QuickNotes-v1.0.3-x64.zip">
    <img src="https://img.shields.io/badge/Download%20Latest%20Release-x64-blue?style=for-the-badge&logo=github" alt="Download Latest Release" />
  </a>
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.3/QuickNotes-v1.0.3-arm64.zip">
    <img src="https://img.shields.io/badge/Download%20Latest%20Release-ARM64-blue?style=for-the-badge&logo=github" alt="Download Latest Release" />
  </a>
</div>

## 📋 Table of Contents

- [📋 Overview](#-overview)
- [⚡ Easy Install](#-easy-install)
- [✨ Features](#-features)
- [🎬 Demo Gallery](#-demo-gallery)
- [🚀 Installation](#-installation)
- [🔧 Usage](#-usage)
- [📁 Data Storage](#-data-storage)
- [🛠️ Building from Source](#️-building-from-source)
- [📊 Project Structure](#-project-structure)
- [🤝 Contributing](#-contributing)
- [❓ FAQ](#-faq)
- [✨ Why You'll Love QuickNotes](#-why-youll-love-quicknotes)
- [📄 License](#-license)
- [🙏 Acknowledgements](#-acknowledgements)
- [🛠️ Implementation Details](#-implementation-details)

## 📋 Overview

QuickNotes is a plugin for [Microsoft PowerToys Run](https://github.com/microsoft/PowerToys) that allows you to quickly create, manage, and search notes directly from your PowerToys Run interface. Simply type `qq` followed by your note text to save it, or use various commands to manage your notes collection.

<div align="center">
  <img src="icon.png" alt="PowerToys" width="200">
</div>

## ⚡ Easy Install

<div align="">
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/download/v1.0.3/QuickNotes-v1.0.3-x64.zip">
    <img src="https://img.shields.io/badge/⬇️_DOWNLOAD-QUICKNOTES_PLUGIN-blue?style=for-the-badge&logo=github" alt="Download QuickNotes Plugin">
  </a>
  
  <p>
    <b>Quick Installation Steps:</b><br>
    1. Download using the button above<br>
    2. Extract to <code>%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\</code><br>
    3. Restart PowerToys<br>
    4. Start using with <code>Alt+Space</code> then type <code>qq</code>
  </p>
</div>

## ✨ Features

- 📝 **Quick Note Creation** - Instantly save notes with a simple command
- 🔍 **Powerful Search** - Find notes with highlighted search terms
- 🏷️ **Tag Support** - Add #tags to notes and search by tag
- 📌 **Pin Important Notes** - Pin critical notes to keep them at the top
- 🔄 **Sorting Options** - Sort notes by date or alphabetically
- ✏️ **Easy Editing** - Modify existing notes with a simple interface
- 🗑️ **Note Management** - Delete individual notes or clear all notes
- ↩️ **Undo Delete** - Restore recently deleted notes
- 💾 **Backup & Export** - Create backups of your notes collection
- 📋 **Clipboard Integration** - Copy notes to clipboard with a single click
- 🌓 **Theme Support** - Works with both light and dark PowerToys themes
- ⏱️ **Timestamp Recording** - Each note is saved with a timestamp for easy reference
- 🔔 **Notification System** - Get confirmation when notes are saved, edited, or deleted
- 🔗 **URL Detection** - Automatically detects and allows opening URLs in notes
- ✨ **Text Formatting** - Format notes with Markdown-style syntax for bold, italic, and highlights
- 🏷️ **Customizable Tag Style** - Toggle between bold or italic formatting for tags
- 💡 **Command Auto-suggestions** - Get real-time command suggestions as you type use Tab key it's easy

## 🎬 Demo Gallery

<div align="center">
  <h3>📝 Creating Notes</h3>
  <p><img src="assets/demo-create notes.gif" width="650" alt="Creating Notes Demo"/></p>
  <p><i>Simply type <code>qq</code> followed by your note text to save it instantly</i></p>
  
  <h3>🔍 Searching Notes</h3>
  <p><img src="assets/demo-qq search.gif" width="650" alt="Searching Notes Demo"/></p>
  <p><i>Use <code>qq search &lt;term&gt;</code> to find notes with highlighted matches</i></p>

  <h3>✏️ Editing Notes</h3>
  <p><img src="assets/demo-qq-edit.gif" width="650" alt="Editing Notes Demo"/></p>
  <p><img src="assets/demo.gif" width="650" alt="Editing Notes Demo"/></p>
  <p><i>Edit existing notes with <code>qq edit &lt;number&gt;</code></i></p>

  <h3>🗑️ Deleting Notes</h3>
  <p><img src="assets/demo-qq delete.gif" width="650" alt="Deleting Notes Demo"/></p>
  <p><i>Delete notes with <code>qq del &lt;number&gt;</code> or all notes with <code>qq delall</code></i></p>

  <h3>💾 Exporting Notes</h3>
  <p><img src="assets/demo-qq export.gif" width="650" alt="Exporting Notes Demo"/></p>
  <p><i>Create backups with <code>qq backup</code> or <code>qq export</code></i></p>
  
  <h3>📋 Copying Notes</h3>
  <p><img src="assets/copy with timestamp.png" width="650" alt="Copy with Timestamp"/></p>
  <p><i>Copy notes with timestamp included</i></p>
  
  <p><img src="assets/copy-without timestamp and tag.png" width="650" alt="Copy without Timestamp and Tags"/></p>
  <p><i>Copy notes without timestamp and tags</i></p>
  
  <h3>👀 Viewing Notes</h3>
  <p><i>View note details with <code>qq view &lt;number&gt;</code></i></p>
    <p><img src="assets/demo-auto-suggestions-1.gif" width="650" alt="Auto-suggestions Demo"/></p>
  
  <h3>❓ Help Command</h3>
  <p><img src="assets/demo-qq help.gif" width="650" alt="Help Command Demo"/></p>
  <p><i>Get help on available commands with <code>qq help</code></i></p>
  
  <h3>✨ Text Formatting</h3>
  <p><i>Format your notes with <code>**bold**</code>, <code>*italic*</code>, <code>==highlight==</code>, and <code>#tags</code></i></p>
  
  <h3>💡 Command Auto-suggestions</h3>
  <p><img src="assets/demo-auto-suggestions.gif" width="650" alt="Auto-suggestions Demo"/></p>
  <p><i>Get command suggestions as you type for faster note-taking</i></p>
</div>

## 🚀 Installation

### 📋 Prerequisites

- [Microsoft PowerToys](https://github.com/microsoft/PowerToys/releases) installed
- Windows 10 or later

### 📥 Installation Steps

1. Download the latest release from the [Releases page](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/latest)
2. Extract the ZIP file to:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\
   ```
3. Restart PowerToys

<div align="center">
  <a href="https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/latest">
    <img src="https://img.shields.io/badge/⬇️_Download-Latest_Release-blue?style=for-the-badge&logo=github" alt="Download Latest Release">
  </a>
</div>

## 🔧 Usage

1. Open PowerToys Run (default: <kbd>Alt</kbd> + <kbd>Space</kbd>)
2. Use the following commands:

<div align="center">

| Command | Description | Example |
|---------|-------------|---------|
| `qq <text>` | Create a new note | `qq Buy milk and eggs` |
| `qq help` | Show help information | `qq help` |
| `qq search <term>` | Search notes (matched words highlighted) | `qq search milk` |
| `qq searchtag <tag>` | Search notes by tag | `qq searchtag work` |
| `qq view <number>` | View note details | `qq view 1` |
| `qq edit <number>` | Edit a specific note | `qq edit 2` |
| `qq del <number>` | Delete a specific note | `qq del 3` |
| `qq delall` | Delete all notes | `qq delall` |
| `qq undo` | Restore last deleted note | `qq undo` |
| `qq pin <number>` | Pin a note to the top | `qq pin 4` |
| `qq unpin <number>` | Unpin a note | `qq unpin 4` |
| `qq sort date` | Sort notes by date | `qq sort date` |
| `qq sort alpha` | Sort notes alphabetically | `qq sort alpha` |
| `qq backup` or `qq export` | Backup notes (opens folder and file) | `qq backup` |
| `qq tagstyle bold` | Set tag style to bold | `qq tagstyle bold` |
| `qq tagstyle italic` | Set tag style to italic | `qq tagstyle italic` |

</div>

### 🎯 Quick Tips

- Press <kbd>Enter</kbd> on a note to copy it to clipboard
- Right-click on a note for additional options (copy, edit, delete, pin/unpin)
- Add #tags to your notes (e.g., `qq Meeting with John #work #important`)
- Use `qq searchtag work` to find all notes with the #work tag
- Notes are automatically saved with timestamps
- Pinned notes always appear at the top of your notes list
- Sort notes with `qq sort date` (newest first) or `qq sort alpha` (A-Z)
- Add `desc` to sort in reverse order (e.g., `qq sort date desc`)
- Use `qq undo` to restore the last deleted note
- URLs in notes are automatically detected and can be opened via the context menu
- Use `qq help` anytime to see all available commands
- Type any command partially to see auto-suggestions (e.g., type `qq s` to see `search`, `sort`, etc.)
- Format your notes with Markdown-style syntax:
  - **Bold text**: Use `**text**` or `__text__`
  - *Italic text*: Use `*text*` or `_text_`
  - ==Highlighted text==: Use `==text==`
  - #tags are automatically formatted based on your tag style setting
- Toggle tag formatting style with `qq tagstyle bold` or `qq tagstyle italic`

- **Intelligent Copy Options**:
  - Press <kbd>Enter</kbd> on a note to copy clean content (without timestamp and tags)
  - Use <kbd>Ctrl</kbd>+<kbd>C</kbd> to copy the full note with timestamp
  - Use <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>C</kbd> for clean content (alternative to Enter)
  - Right-click menu provides all copy options with clear descriptions

## 📁 Data Storage

QuickNotes stores all your notes in a simple text file at:
```
%LOCALAPPDATA%\Microsoft\PowerToys\QuickNotes\notes.txt
```

Each note is stored with a timestamp for easy reference in the format:
```
[YYYY-MM-DD HH:MM:SS] Your note text here
```

Pinned notes are stored with a special prefix:
```
[PINNED] [YYYY-MM-DD HH:MM:SS] Your important note here
```

## 🛠️ Building from Source

### 📋 Prerequisites

- Visual Studio 2022 or later
- .NET SDK

### 🔨 Build Steps

1. Clone the repository:
   ```
   git clone https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes.git
   ```

2. Open the solution in Visual Studio

3. Build the solution:
   ```
   dotnet build -c Release
   ```

4. Find the output in the `bin/Release` directory

## 📊 Project Structure

The QuickNotes plugin has a clean, modular architecture designed for maintainability and extensibility.

<div align="center">
  <img src="assets/structure-1.png" alt="QuickNotes Project Structure" width="650">
  <p><i>High-level project structure overview</i></p>
  
  <img src="assets/structure-2.png" alt="QuickNotes Component Diagram" width="650">
  <p><i>Component interaction diagram</i></p>
</div>

For a detailed breakdown of the project structure, see the [structure.md](structure.md) file.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request


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

## ✨ Why You'll Love QuickNotes

- **⚡ Lightning Fast** - From thought to saved note in under 3 seconds
- **🔍 Smart Search** - Find notes with highlighted search terms as you type
- **🧠 Distraction-Free** - No need to switch applications or contexts
- **🔄 Seamless Workflow** - Create, find, edit, and manage notes without leaving your keyboard
- **📋 Clipboard Integration** - Copy any note to clipboard with a single keystroke
- **🛠️ Powerfully Simple** - Sophisticated features hidden behind simple commands

<div align="center">
  <img src="assets/new-features.png" alt="QuickNotes New Features" width="650">
</div>

<div align="center">
  <img src="assets/functions.png" alt="QuickNotes Functions" width="650">
  <p><i>**Powerful context menu features: Edit notes, Copy to clipboard, Pin important items, Delete notes, and Detect URL and Open URLs directly from your notes in your browser**</i></p>
</div>

**Feature Ideas We'd Love to See:**
- ~~Tag system for notes~~
- Rich text formatting
- Cloud sync options
- Note categories
- Reminder functionality

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- [Microsoft PowerToys](https://github.com/microsoft/PowerToys) team for creating the extensible PowerToys Run platform
- All contributors who have helped improve this plugin
- Icons and visual elements from various open-source projects

## 🛠️ Implementation Details

The QuickNotes plugin is built around a robust and well-structured implementation in the `Main.cs` file, which serves as the heart of the plugin. Here's what makes it special:

### 🏗️ Architecture

- **Modular Design**: The code follows a clean, modular approach with well-defined responsibilities
- **NoteEntry Structure**: Uses a dedicated class to handle note metadata (timestamps, pinning status, etc.)
- **Command Pattern**: Implements a command-based architecture for all operations

### 🔑 Key Components

- **Command Processing**: Centralized command handling via the `GetCommandResults` method
- **Text Formatting**: Rich text formatting with support for bold, italic, highlighting, and tags
- **Context Menu**: Comprehensive right-click options for each note
- **File I/O**: Robust file operations with proper error handling
- **Theme Support**: Adapts to PowerToys light/dark themes automatically

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

---

<div align="center">
  <p>Made with ❤️ by <a href="https://github.com/ruslanlap">ruslanlap</a></p>
  
  <a href="#-powertoys-run-quicknotes-plugin">Back to top ⬆️</a>
</div>
