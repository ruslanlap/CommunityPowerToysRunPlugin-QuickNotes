# Changelog

All notable changes to this project will be documented in this file.

## [1.0.10] - 2025-01-10

### Added

- ☁️ **Git Sync Feature**: Sync your notes to a Git repository for backup and multi-device access
  - `qq sync` - Commit and push notes to remote repository with timestamp-based commit messages
  - `qq restore` - Pull and restore notes from remote repository (creates local backup first)
  - Configurable via PowerToys Settings (Additional Options):
    - Enable Git Sync checkbox
    - Git Repository URL (HTTPS/SSH)
    - Git Branch (default: main)
    - Git Username (optional, uses global config if empty)
    - Git Email (optional, uses global config if empty)
- Automatic fallback to global git config for username/email if not specified
- Better error handling for authentication failures

### Changed

- Upgraded System.Text.Json from 8.0.3 to 9.0.0 (fixed security vulnerabilities)
- Improved ISettingProvider implementation using AdditionalOptions
- Removed custom Settings UI panel in favor of PowerToys native settings integration
- Enhanced credential handling with DefaultCredentials for git operations

### Fixed

- Fixed missing ISettingProvider interface members (AdditionalOptions, UpdateSettings)
- Fixed DialogResult property hiding warning in MultiLineInputDialog
- Fixed duplicate XAML Page entries in project file
- Improved settings persistence and loading mechanism

## [1.0.8] - 2024-06-19

### Fixed

- Fixed issue with duplicate timestamps when editing notes (Thanks @yomingpan)
- Fixed issue with edit dialog opening twice when using the edit command
- Improved export functionality to exclude note IDs for cleaner output (Thanks @yomingpan)

### Changed

- Updated edit functionality to prevent duplicate dialog windows
- Improved error handling during note editing

## [1.0.7] - 2024-06-17

### Added

- Initial release of QuickNotes plugin
