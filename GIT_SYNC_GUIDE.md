# Git Sync Guide for QuickNotes

This guide explains how to set up and use the Git Sync feature in QuickNotes to backup and synchronize your notes with a GitHub repository.

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Usage](#usage)
- [Troubleshooting](#troubleshooting)
- [How It Works](#how-it-works)

---

## 🎯 Overview

Git Sync allows you to:
- **Backup** your notes to a GitHub repository
- **Sync** notes across multiple devices
- **Track history** of all changes to your notes
- **Restore** notes from the cloud

## ✅ Prerequisites

### 1. Create a GitHub Repository

1. Go to [GitHub](https://github.com) and sign in
2. Click **"New repository"** (green button)
3. Name it `notes` (or any name you prefer)
4. Choose **Private** for personal notes
5. **Do NOT** initialize with README, .gitignore, or license
6. Click **"Create repository"**

### 2. Set Up Authentication

Choose **ONE** of these methods:

#### Option A: SSH (Recommended)

1. Generate SSH key (if you don't have one):
   ```bash
   ssh-keygen -t ed25519 -C "your_email@example.com"
   ```
2. Add SSH key to GitHub:
   - Go to GitHub → Settings → SSH and GPG keys
   - Click "New SSH key"
   - Copy content of `~/.ssh/id_ed25519.pub` and paste it
3. Test connection:
   ```bash
   ssh -T git@github.com
   ```
   You should see: "Hi username! You've successfully authenticated..."

#### Option B: HTTPS with Personal Access Token

1. Generate token:
   - Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Click "Generate new token (classic)"
   - Select scopes: `repo` (all checkboxes)
   - Generate and **copy the token** (you won't see it again!)
2. Windows will prompt for credentials on first push
   - Use your GitHub **username**
   - Paste the **token** as password

---

## ⚙️ Setup Instructions

### Step 1: Enable Git Sync in QuickNotes

1. Press `Alt + Space` to open PowerToys Run
2. Type `qq` and press Enter
3. Open PowerToys Settings:
   - Press `Win + I` → PowerToys → PowerToys Run → Plugins
   - Find **QuickNotes** in the list
   - Click the **gear icon** ⚙️ next to QuickNotes

### Step 2: Configure Git Settings

In the QuickNotes settings panel, fill in:

| Setting | Description | Example |
|---------|-------------|---------|
| **Enable Git Sync** | Check this box | ☑️ |
| **Git Repository URL** | Your repository URL | See below |
| **Git Branch** | Branch name | `main` (default) |
| **Git Username** | Your name for commits | `John Doe` (optional) |
| **Git Email** | Your email for commits | `john@example.com` (optional) |

#### Repository URL Examples:

**For SSH:**
```
git@github.com:yourusername/notes.git
```

**For HTTPS:**
```
https://github.com/yourusername/notes.git
```

### Step 3: Save Settings

Click **"Save"** or close the settings window. PowerToys will apply the changes.

---

## 🚀 Usage

### Sync Notes to GitHub

To push your local notes to GitHub:

1. Press `Alt + Space`
2. Type `qq sync`
3. Press `Enter`

**First Time Only:** The plugin will:
- Initialize a Git repository in your notes folder
- Add your existing `notes.txt` file
- Create an initial commit
- Push to GitHub

**Subsequent Times:** The plugin will:
- Stage changes to `notes.txt`
- Create a commit with timestamp
- Push to GitHub

### Restore Notes from GitHub

To pull notes from GitHub (overwrites local notes):

1. Press `Alt + Space`
2. Type `qq restore`
3. Press `Enter` to confirm

⚠️ **Warning:** This will replace your local notes with the version from GitHub. A backup will be created automatically.

---

## 🔧 Troubleshooting

### Error: "Git CLI not found"

**Solution:** The plugin now shells out to the Git executable. Windows must have Git for Windows installed and available on `PATH`.

1. Install [Git for Windows](https://git-scm.com/download/win) if it's missing
2. Re-open PowerToys after installation
3. Verify `git --version` works in a new terminal

### Error: "Authentication failed"

**For SSH:**
1. Verify SSH key is added to GitHub
2. Test: `ssh -T git@github.com`
3. Ensure you're using the SSH URL format: `git@github.com:username/notes.git`

**For HTTPS:**
1. Verify Personal Access Token is correct
2. Check Windows Credential Manager for saved credentials
3. Try re-entering credentials on next sync

### Error: "Repository not configured"

**Solution:** Git settings are not saved properly.

1. Open QuickNotes settings again
2. Check **"Enable Git Sync"** checkbox
3. Re-enter the **Git Repository URL**
4. Click **"Save"**
5. Restart PowerToys

### Error: "Failed to initialize repository"

**Possible causes:**
- Repository already exists on GitHub with content
- No internet connection
- Incorrect repository URL
- Permission issues with local notes folder

**Solution:**
1. Verify the repository URL is correct
2. Ensure the GitHub repository is **empty** (no README or initial files)
3. Check your internet connection
4. Verify you have write permissions to `%LOCALAPPDATA%\Microsoft\PowerToys\QuickNotes`

---

## 📖 How It Works

### File Location

Your notes are stored locally at:
```
C:\Users\YourUsername\AppData\Local\Microsoft\PowerToys\QuickNotes\notes.txt
```

### First Sync (Initialize)

When you run `qq sync` for the first time:

1. **Initialize Git**: Creates `.git` folder in the QuickNotes directory
2. **Add Remote**: Configures `origin` pointing to your GitHub repository
3. **Stage Files**: Adds your existing `notes.txt` to Git
4. **Create Commit**: Makes an initial commit with your notes
5. **Push**: Uploads to GitHub

### Regular Sync

On subsequent syncs:

1. **Stage Changes**: Git detects changes to `notes.txt`
2. **Create Commit**: Creates a commit with timestamp (e.g., "Auto-sync: 2025-10-10 15:30:00")
3. **Push**: Uploads changes to GitHub

### Restore

When you run `qq restore`:

1. **Backup**: Creates a local backup of current notes (e.g., `notes_backup_20251010_153000.txt`)
2. **Fetch**: Downloads latest changes from GitHub
3. **Reset**: Replaces local notes with GitHub version

### Background Operations

All Git operations run in the background to avoid freezing the PowerToys Run interface. You'll see a notification when the operation completes.

---

## 🎯 Best Practices

1. **Sync Regularly**: Run `qq sync` after making important changes
2. **Private Repository**: Keep your notes repository private for security
3. **Backup Before Restore**: The plugin creates automatic backups, but you can also manually copy `notes.txt`
4. **Review Commits**: Check your GitHub repository occasionally to see the sync history
5. **One Device at a Time**: If using multiple devices, sync before making changes to avoid conflicts

---

## 🔐 Security Notes

- **Private Repositories**: Always use a private GitHub repository for personal notes
- **Sensitive Data**: Be careful about storing passwords or API keys in notes
- **SSH vs HTTPS**: SSH is more secure for automation, HTTPS requires token management
- **Local Storage**: Notes are stored unencrypted locally on your device

---

## 📞 Support

If you encounter issues:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review PowerToys logs: `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Logs\`
3. Open an issue on [GitHub](https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/issues)

---

## 📝 Example Workflow

### Initial Setup

```
1. Create GitHub repository "notes"
2. Set up SSH key or Personal Access Token
3. Open QuickNotes settings in PowerToys
4. Enable Git Sync
5. Enter: git@github.com:yourusername/notes.git
6. Save settings
7. Run: qq sync
8. ✅ Notes are now on GitHub!
```

### Daily Use

```
Morning:
- qq restore          → Get latest notes from GitHub

During the day:
- qq buy milk        → Create note
- qq meeting at 3pm  → Create note
- qq #work deadline  → Create tagged note

Evening:
- qq sync            → Backup to GitHub
```

---

Made with ❤️ by the QuickNotes team
