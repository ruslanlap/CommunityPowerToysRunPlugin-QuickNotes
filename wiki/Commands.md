# 🔧 Main Commands & Usage

## Creating Notes

- Type `qq` followed by your note text to save it instantly.
  - Example: `qq Buy groceries`

## Quick Command Table

| Command                       | Description                            | Example                |
|-------------------------------|----------------------------------------|------------------------|
| `qq <text>`                   | Create a new note                      | `qq Buy milk and eggs` |
| `qq help`                     | Show help information                  | `qq help`              |
| `qq search <term>`            | Search notes (highlighted matches)     | `qq search milk`       |
| `qq searchtag <tag>`          | Search notes by tag                    | `qq searchtag work`    |
| `qq view <number>`            | View note details                      | `qq view 1`            |
| `qq edit <number>`            | Edit a specific note                   | `qq edit 2`            |
| `qq del <number>`             | Delete a specific note                 | `qq del 3`             |
| `qq delall`                   | Delete all notes                       | `qq delall`            |
| `qq undo`                     | Restore last deleted note              | `qq undo`              |
| `qq pin <number>`             | Pin a note to the top                  | `qq pin 4`             |
| `qq unpin <number>`           | Unpin a note                           | `qq unpin 4`           |
| `qq sort date`                | Sort notes by date                     | `qq sort date`         |
| `qq sort alpha`               | Sort notes alphabetically              | `qq sort alpha`        |
| `qq backup`                   | Backup notes (opens folder and file)   | `qq backup`            |
| `qq export`                   | Export notes (silent backup, no windows)| `qq export`            |
| `qq tagstyle bold`            | Set tag style to bold                  | `qq tagstyle bold`     |
| `qq tagstyle italic`          | Set tag style to italic                | `qq tagstyle italic`   |

## Quick Tips

- Press <kbd>Enter</kbd> on a note to copy it to clipboard.
- Use `qq searchtag work` to find all notes with the #work tag.
- Notes are automatically saved with timestamps.
- Pinned notes always appear at the top of your notes list.
- Sort notes with `qq sort date` (newest first) or `qq sort alpha` (A-Z).
- Add `desc` to sort in reverse order (e.g., `qq sort date desc`).
- Use `qq undo` to restore the last deleted note.
- URLs in notes are automatically detected and can be opened via the context menu.
- Use `qq help` anytime to see all available commands.
- Type any command partially to see auto-suggestions (e.g., type `qq s` to see `search`, `sort`, etc.).
