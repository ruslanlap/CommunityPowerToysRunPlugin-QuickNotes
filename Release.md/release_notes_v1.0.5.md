# 🐛 QuickNotes v1.0.5 - Critical Delete Bug Fix

## 🔧 Bug Fixes

### ✅ **Fixed Critical Delete Index Mismatch**
- **Issue**: When attempting to delete notes (e.g., "Test10"), the application incorrectly deleted wrong notes due to index mismatch between display and deletion logic
- **Root Cause**: Display showed original file positions but delete used filtered list positions after removing empty lines
- **Fix**: Rebuilt note indices to ensure consistency between display numbers and deletion targets
- **Impact**: Now "what you see is what you delete" - no more unexpected deletions! 🎯

### 🔄 **Affected Operations**
- **Delete Notes** (`qq del <number>`) - Now deletes the correct note
- **Edit Notes** (`qq edit <number>`) - Now edits the correct note  
- **View Notes** (`qq view <number>`) - Now views the correct note
- **Pin/Unpin Notes** (`qq pin/unpin <number>`) - Now pins/unpins the correct note
- **Context Menu Actions** - All right-click operations now target correct notes

### 📝 **Technical Details**
- Modified `ReadNotes()` to rebuild indices sequentially after filtering empty lines
- Updated all note operations to find notes by `OriginalIndex` instead of filtered list position
- Improved error messages to show available note numbers when note not found
- Ensured display numbers consistently match deletion targets

### 🧪 **Test Scenario**
**Before Fix:**
```
File: Test1, Test2, Test3, [empty line], Test4...Test10
User sees: [1] Test1, [2] Test2, [3] Test3, [5] Test4...[10] Test10  
qq del 10: ❌ "Note number 10 is invalid. Max index is 7"
```

**After Fix:**
```
File: Test1, Test2, Test3, [empty line], Test4...Test10
User sees: [1] Test1, [2] Test2, [3] Test3, [4] Test4...[10] Test10
qq del 10: ✅ Correctly deletes Test10
```

## 📥 Installation

1. Download the latest release
2. Extract to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\`
3. Restart PowerToys
4. Start using with `Alt+Space` then type `qq`

## 🙏 Thanks

Thanks to the community for reporting this critical issue! This fix ensures reliable note management without unexpected deletions.

---

**Full Changelog**: https://github.com/ruslanlap/CommunityPowerToysRunPlugin-QuickNotes/compare/v1.0.4...v1.0.5 