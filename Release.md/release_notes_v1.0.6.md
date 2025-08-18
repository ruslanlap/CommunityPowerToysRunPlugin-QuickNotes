# QuickNotes v1.0.6 Release Notes

![Version](https://img.shields.io/badge/version-1.0.6-brightgreen)
![Release Date](https://img.shields.io/badge/release_date-May_2025-blue)
![Status](https://img.shields.io/badge/status-stable-green)

## 🚀 Major Improvements

This release introduces a complete rework of the note management system, significantly improving reliability and addressing critical bugs in how notes are handled.

### 🔍 Major Changes

- **Completely Redesigned Note Identification System**: 
  - Notes are now reliably identified by their content rather than their position in the file
  - All operations (delete, edit, view, pin) will always target the correct note, regardless of file changes
  - More robust handling of note searching and identification prevents accidental deletions or modifications

- **Improved Error Handling**:
  - Enhanced error messages provide clearer information when operations fail
  - Better file access handling to prevent data loss
  - More consistent backup behavior to protect your notes

- **Enhanced UI Messages**:
  - Better user feedback when operations succeed or fail
  - Clearer instructions for recovering from errors
  - More descriptive tooltips for note details

## 🛠️ Technical Improvements

- **Code Architecture**:
  - Removed dependency on file position (FileLineIndex) for note identification
  - Implemented content-based identification for all note operations
  - Enhanced search algorithms to find the correct note even if the file structure changes

- **Reliability**:
  - More robust checks before performing operations on notes
  - Better validation of note content during operations
  - Improved compatibility with external file changes

## 🐛 Bug Fixes

- **Critical**: Fixed issue where notes could be deleted incorrectly when targeting by index
- **Critical**: Fixed problem where editing a note could overwrite the wrong note if the file structure changed
- **Critical**: Fixed issue where pinning a note would fail if the file had been modified
- **Important**: Fixed problem where undoing a delete could restore the note to the wrong position

## 💡 Recommendations

- This update is **strongly recommended** for all users due to the critical nature of the bug fixes
- After updating, you may want to verify your notes are intact by viewing all notes with `qq list`
- If you've experienced issues with notes being deleted incorrectly in the past, this update should resolve those problems

## 📋 Installation

1. Download the latest version from the [Releases Page](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/releases/latest)
2. Extract to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\`
3. Restart PowerToys
4. Start using with `Alt+Space` then type `qq`

---

Thank you for using QuickNotes! If you encounter any issues, please [report them on GitHub](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/issues).

Made with ❤️ by [ruslanlap](https://github.com/ruslanlap)
