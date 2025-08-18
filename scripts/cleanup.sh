#!/bin/bash

# This script removes files from git tracking while keeping them locally

# Remove image files
git rm --cached icon.png 2>/dev/null || true
git rm --cached terminal.png 2>/dev/null || true
git rm --cached vs-new-project.png 2>/dev/null || true
git rm --cached vs.png 2>/dev/null || true

# Remove script files
git rm --cached dotnet-install.sh 2>/dev/null || true

# Remove any remaining zip files
git rm --cached *.zip 2>/dev/null || true
git rm --cached *.zip.sha256 2>/dev/null || true
git rm --cached *.sha256 2>/dev/null || true
git rm --cached checksums.txt 2>/dev/null || true

# Remove any other unnecessary files
git rm --cached .DS_Store 2>/dev/null || true
git rm --cached Thumbs.db 2>/dev/null || true

# Commit the changes
git commit -m "Remove files that should be ignored from git tracking"

# Push to GitHub
git push origin master

echo "Cleanup complete! Files have been removed from git tracking but kept locally."
