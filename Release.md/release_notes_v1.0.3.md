# 📝 QuickNotes v1.0.3

![QuickNotes Logo](https://raw.githubusercontent.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/master/QuickNotes/Community.PowerToys.Run.Plugin.QuickNotes/Images/quicknotes.dark.png)

## ✨ What's New

### 💡 Command Auto-suggestions
- Real-time command suggestions as you type
- Type partial commands to see matching options
- Faster and more intuitive note-taking experience
- Ukrainian language support for command descriptions

### ✨ Text Formatting
- Format your notes with Markdown-style syntax:
  - **Bold text**: Use `**text**` or `__text__`
  - *Italic text*: Use `*text*` or `_text_`
  - ==Highlighted text==: Use `==text==`
  - #tags are automatically formatted
- Toggle tag formatting style with `qq tagstyle bold` or `qq tagstyle italic`

### 🐛 Bug Fixes
- Fixed nullable reference type warnings
- Improved code organization and readability
- Enhanced command handling logic

## 📥 Installation

1. Download the ZIP file for your platform (x64 or ARM64)
2. Extract to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\`
3. Restart PowerToys
4. Start using with `Alt+Space` then type `qq`

## 🔍 Quick Commands

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

## 🙏 Thank You

Thank you for using QuickNotes! If you encounter any issues or have suggestions, please [open an issue](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/issues).

Made with ❤️ by [ruslanlap](https://github.com/ruslanlap)
