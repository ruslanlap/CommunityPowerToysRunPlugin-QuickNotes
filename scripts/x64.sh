#!/bin/bash

# Check if the current directory is not QuickNotes
if [ "$(basename "$PWD")" != "QuickNotes" ]; then
  # Navigate to QuickNotes directory
  cd ~/workspace/QuickNotes || exit
fi

# Perform the zip operation
zip -r Release.zip Community.PowerToys.Run.Plugin.QuickNotes/bin/x64/Release

# Return to the previous directory
cd - || exit
