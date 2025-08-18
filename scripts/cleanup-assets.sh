#!/bin/bash

# This script removes large asset files from git tracking while keeping them locally

# Remove large GIF files
git rm --cached assets/*.gif 2>/dev/null || true
git rm --cached assets/copy-*.png 2>/dev/null || true
git rm --cached assets/structure-*.png 2>/dev/null || true
git rm --cached assets/new-features.png 2>/dev/null || true

# Commit the changes
git commit -m "Remove large asset files from git tracking"

# Push to GitHub
git push origin master

echo "Cleanup complete! Large asset files have been removed from git tracking but kept locally."
