# 📝 QuickNotes Markdown Features

## Overview

QuickNotes now supports full markdown formatting and multi-line notes! This enhancement allows you to create rich, structured notes with code snippets, headers, lists, and more.

## New Commands

| Command | Description | Usage |
|---------|-------------|-------|
| `qq markdown` | Open multi-line markdown editor | `qq markdown` |
| `qq md` | Shortcut for markdown editor | `qq md` |

## Markdown Syntax Support

### Text Formatting
- **Bold text**: `**bold**` or `__bold__` → 【bold】
- *Italic text*: `*italic*` or `_italic_` → 〈italic〉
- ==Highlighted text==: `==highlight==` → 《highlight》
- `Inline code`: `` `code` `` → ⟨code⟩

### Headers
- `# Header 1` → ▶▶▶ Header 1
- `## Header 2` → ▶▶ Header 2  
- `### Header 3` → ▶ Header 3

### Lists
- Unordered lists: `- Item` → • Item
- Ordered lists: `1. Item` → ① Item

### Code Blocks
```
```
Your code here
```
```

Displays as:
```
┌─ CODE ─┐
Your code here
└─────────┘
```

### Tags
- `#work` `#important` `#project` → 【#work】【#important】【#project】
- Toggle between bold/italic with `qq tagstyle bold` or `qq tagstyle italic`

## Multi-Line Editor Features

### Interface
- **Split-pane editor** with live preview
- **Markdown input** on the left
- **Formatted preview** on the right
- **Syntax help** expandable section
- **Dark theme** matching PowerToys

### Keyboard Shortcuts
- `Ctrl+Enter` - Save note
- `Escape` - Cancel and close
- `Tab` - Insert tab character
- Standard text editing shortcuts

### Storage
- Multi-line notes are automatically encoded for storage
- Seamlessly integrates with existing note management
- Preserves formatting when copying/editing

## Usage Examples

### Creating a Markdown Note
1. Type `qq markdown` or `qq md`
2. Multi-line editor opens
3. Write your markdown content
4. See live preview on the right
5. Press `Ctrl+Enter` to save

### Example Note Content
```markdown
# Meeting Notes #work

## Agenda
- Review project status
- Discuss next steps
- **Important**: Deadline is Friday

## Code Review
```python
def process_data(data):
    return data.strip().lower()
```

## Action Items
1. Update documentation
2. Fix bug in ==authentication module==
3. Schedule follow-up meeting

#meeting #urgent
```

### Display in QuickNotes
The note will appear in your notes list with:
- First line as title: "▶▶▶ Meeting Notes 【#work】..."
- Full formatted content in tooltip
- All markdown formatting preserved

## Integration with Existing Features

### Search
- Search works across multi-line content
- Markdown formatting is searchable
- Tags in markdown notes are indexed

### Copy Options
- **Enter**: Copy clean content (no timestamps/tags)
- **Ctrl+C**: Copy full note with formatting
- Multi-line formatting preserved in clipboard

### Editing
- Right-click → Edit opens the markdown editor
- Existing content pre-loaded
- Full markdown editing capabilities

### Pinning & Organization
- Pin markdown notes like regular notes
- Sort by date or alphabetically
- All existing commands work with markdown notes

## Technical Details

### Storage Format
- Multi-line content encoded with `⟨NL⟩` markers
- Backward compatible with existing notes
- Automatic encoding/decoding on save/load

### Performance
- Efficient regex-based formatting
- Live preview updates as you type
- Minimal memory footprint

### Compatibility
- Works with existing PowerToys themes
- Compatible with all current QuickNotes features
- No breaking changes to existing functionality

## Tips & Best Practices

1. **Use headers** to structure long notes
2. **Tag consistently** for better organization
3. **Code blocks** for snippets and examples
4. **Lists** for action items and checklists
5. **Bold/italic** for emphasis and importance

## Troubleshooting

### Common Issues
- **Editor won't open**: Check PowerToys permissions
- **Formatting not showing**: Ensure proper markdown syntax
- **Multi-line not saving**: Use `Ctrl+Enter` to save

### Support
- Use `qq help` to see all available commands
- Check tooltip for note details
- Right-click for context menu options

---

*This feature addresses GitHub issue #11 - Support for markdown notes and multi-line input.*
