using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics; // For Process.Start
using System.IO;
using System.Linq;
using System.Text; // For StringBuilder
using System.Text.RegularExpressions; // For highlighting and URL detection
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;
using Wox.Plugin.Logger;
using Microsoft.VisualBasic; // For InputBox

namespace Community.PowerToys.Run.Plugin.QuickNotes
{
    // Note structure to handle metadata like pinning and original index
    internal class NoteEntry
    {
        public string Text { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        public int OriginalIndex { get; set; } // Index in the original file
        public DateTime Timestamp { get; set; } = DateTime.MinValue; // Parsed timestamp

        // Simple parsing for pinned marker and timestamp
        internal static NoteEntry Parse(string line, int index)
        {
            var entry = new NoteEntry { OriginalIndex = index };
            string remainingText = line;

            // Check for pinned marker (e.g., "[PINNED] ")
            const string pinnedMarker = "[PINNED] ";
            if (remainingText.StartsWith(pinnedMarker, StringComparison.OrdinalIgnoreCase))
            {
                entry.IsPinned = true;
                remainingText = remainingText.Substring(pinnedMarker.Length);
            }

            // Check for timestamp marker (e.g., "[YYYY-MM-DD HH:MM:SS] ")
            if (remainingText.Length > 22 && remainingText[0] == '[' && remainingText[21] == ']')
            {
                if (DateTime.TryParse(remainingText.Substring(1, 19), out DateTime ts))
                {
                    entry.Timestamp = ts;
                    entry.Text = remainingText;
                }
                else
                {
                    entry.Text = remainingText; // Parsing failed, keep original
                }
            }
            else
            {
                entry.Text = remainingText; // No timestamp found
            }

            return entry;
        }

        internal string ToFileLine()
        {
            var sb = new StringBuilder();
            if (IsPinned)
            {
                sb.Append("[PINNED] ");
            }
            sb.Append(Text);
            return sb.ToString();
        }
    }

    public class Main : IPlugin, IContextMenu, IPluginI18n, IDisposable, IDelayedExecutionPlugin
    {
        // --- Constants ---
        public static string PluginID => "2083308C581F4D36B0C02E69A2FD91D7";
        private const string PinnedMarker = "[PINNED] ";
        private static readonly Regex UrlRegex = new Regex(@"\b(https?://|www\.)\S+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const string NotesFileName = "notes.txt"; // Centralize filename

        // --- Properties ---
        public string Name => "QuickNotes";
        public string Description => "Save, view, manage, search, tag, and pin quick notes";

        private PluginInitContext? Context { get; set; }
        private string? IconPath { get; set; }
        private bool Disposed { get; set; }

        private string _notesPath = string.Empty;
        private bool _isInitialized = false;

        // --- State for Undo ---
        private (string Text, int Index, bool WasPinned)? _lastDeletedNote;

        // For text formatting
        private bool _useItalicForTags = false; // Default: use bold formatting for tags

        // List of available commands for autocomplete
        private readonly List<string> _commands = new List<string>
        {
            "help",
            "backup",
            "export",
            "edit",
            "view",
            "delall",
            "del",
            "delete",
            "search",
            "searchtag",
            "pin",
            "unpin",
            "undo",
            "sort",
            "tagstyle"
        };

        // Dictionary for command descriptions
        private readonly Dictionary<string, string> _commandDescriptions = new Dictionary<string, string>
        {
            { "help", "Show help with available commands ℹ️" },
            { "backup", "Create a backup of notes 💾" },
            { "export", "Export notes to a file 💾" },
            { "edit", "Edit note by number 📝" },
            { "view", "View note details 👁️" },
            { "delall", "Delete all notes 💣" },
            { "del", "Delete note by number 🗑️" },
            { "delete", "Delete note by number 🗑️" },
            { "search", "Search notes by text 🔍" },
            { "searchtag", "Search notes by tag 🏷️" },
            { "pin", "Pin note to top of list 📌" },
            { "unpin", "Unpin note 📎" },
            { "undo", "Undo last deletion ↩️" },
            { "sort", "Sort notes by date or text 🔄" },
            { "tagstyle", "Change tag display style (bold/italic) ✨" }
        };

        // --- Initialization and Lifecycle ---
        public void Init(PluginInitContext context)
        {
            try
            {
                Context = context ?? throw new ArgumentNullException(nameof(context));
                UpdateIconPath(Context.API.GetCurrentTheme());
                Context.API.ThemeChanged += OnThemeChanged;

                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var powerToysPath = Path.Combine(appDataPath, "Microsoft", "PowerToys", "QuickNotes");
                if (!Directory.Exists(powerToysPath))
                {
                    Directory.CreateDirectory(powerToysPath);
                }

                _notesPath = Path.Combine(powerToysPath, NotesFileName);
                if (!File.Exists(_notesPath))
                {
                    File.WriteAllText(_notesPath, string.Empty);
                }

                try
                {
                    File.AppendAllText(_notesPath, string.Empty); // Test write access
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    _isInitialized = false;
                    Log.Exception("QuickNotes: Failed to access notes file", ex,
                                  typeof(Main),
                                  "Community.PowerToys.Run.Plugin.QuickNotes.Main",
                                  nameof(Init),
                                  66);
                }
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                if (Context?.API != null)
                {
                    Log.Exception("QuickNotes: Failed to initialize plugin", ex,
                                  typeof(Main),
                                  "Community.PowerToys.Run.Plugin.QuickNotes.Main",
                                  nameof(Init),
                                  80);
                }
            }
        }

        // Add this helper method to strip timestamp from note text
        private string StripTimestamp(string noteText)
        {
            // Timestamp format: [YYYY-MM-DD HH:MM:SS] 
            if (noteText.Length >= 22 && noteText[0] == '[' && noteText[20] == ']' && noteText[21] == ' ')
            {
                return noteText.Substring(22).Trim(); // Remove timestamp prefix
            }
            return noteText.Trim();
        }

        // Add this new helper method to strip both timestamp and hashtags
        private string StripTimestampAndTags(string noteText)
        {
            // First remove timestamp
            string textWithoutTimestamp = StripTimestamp(noteText);

            // Then remove hashtags using regex
            string textWithoutTags = Regex.Replace(textWithoutTimestamp, @"#\w+\s*", "");

            // Trim any extra spaces
            return textWithoutTags.Trim();
        }

        // Method for providing autocomplete suggestions
        public List<Result> GetQuerySuggestions(Query query, bool execute)
        {
            if (!_isInitialized)
                return new List<Result>();

            var searchText = CleanupQuery(query.Search?.Trim() ?? string.Empty);

            // If query is empty or too short
            if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 1)
            {
                return new List<Result>();
            }

            // Find matching commands
            var suggestions = new List<Result>();

            // Check if this could be a partial command
            string[] parts = searchText.Split(new[] { ' ' }, 2);
            string possibleCommand = parts[0].ToLowerInvariant();

            // If the text includes spaces, it could be an actual note with a command word
            // In that case, don't try to suggest commands
            if (parts.Length == 1)
            {
                // Get all commands that start with the possible command prefix
                var matchingCommands = _commands
                    .Where(cmd => cmd.StartsWith(possibleCommand, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(cmd => cmd.Length)  // Prioritize shorter matches first
                    .ToList();

                // If we have exact matches for commands, prioritize them
                bool hasExactMatch = matchingCommands.Contains(possibleCommand, StringComparer.OrdinalIgnoreCase);

                foreach (var command in matchingCommands)
                {
                    // Skip showing the exact match as a suggestion if we have other options
                    if (hasExactMatch && command.Equals(possibleCommand, StringComparison.OrdinalIgnoreCase) && matchingCommands.Count > 1)
                        continue;

                    suggestions.Add(new Result
                    {
                        Title = $"qq {command}",
                        SubTitle = _commandDescriptions.ContainsKey(command) 
                            ? _commandDescriptions[command] 
                            : $"Execute command '{command}'",
                        IcoPath = IconPath,
                        Score = 1000, // Very high score to ensure commands appear first
                        Action = _ =>
                        {
                            if (execute)
                            {
                                // Execute the command with space after
                                Context?.API.ChangeQuery($"qq {command} ", true);
                                return true;
                            }
                            // Just replace the input text with the command
                            Context?.API.ChangeQuery($"qq {command} ", false);
                            return false;
                        }
                    });
                }
            }

            // If we have suggestions for commands and this could be a command, don't show the "Add note" option
            if (suggestions.Count > 0)
                return suggestions;

            // If no command suggestions or this is clearly a note, add the "Add note" option
            if (!_commands.Contains(possibleCommand, StringComparer.OrdinalIgnoreCase) || parts.Length > 1)
            {
                suggestions.Add(new Result
                {
                    Title = $"Add note: {searchText}",
                    SubTitle = "Press Enter to save this note (with timestamp)",
                    IcoPath = IconPath,
                    Score = 10, // Lower score than commands
                    Action = _ =>
                    {
                        if (execute)
                        {
                            CreateNote(searchText);
                            Context?.API.ShowMsg("Note saved", $"Saved: {searchText}");
                            return true;
                        }
                        return false;
                    }
                });
            }

            return suggestions;
        }

        // Define priority for results
        public int GetPriority(Query query)
        {
            return 0; // Use 0 to not change priority
        }

        // Method to clean up query text and handle duplicate "qq" prefixes
        private string CleanupQuery(string query)
        {
            // Check for duplicate "qq" prefixes
            if (query.StartsWith("qq qq ", StringComparison.OrdinalIgnoreCase))
            {
                return query.Substring(3).Trim(); // Remove the first "qq "
            }
            return query.Trim();
        }

        public List<Result> Query(Query query, bool delayedExecution)
        {
            return Query(query);
        }

        public List<Result> Query(Query query)
        {
            if (!_isInitialized)
            {
                return ErrorResult("QuickNotes not initialized", "Plugin not initialized properly. Please restart PowerToys.");
            }

            // Get the text after "qq" and clean it up
            var originalSearch = query.Search?.Trim() ?? string.Empty;
            var searchText = CleanupQuery(originalSearch);

            // If empty search, show instructions and notes
            if (string.IsNullOrEmpty(searchText))
            {
                return GetInstructionsAndNotes(string.Empty);
            }

            // Parse the command and arguments
            string[] parts = searchText.Split(new[] { ' ' }, 2);
            string command = parts[0].ToLowerInvariant();
            string args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            // If we detected and cleaned up a duplicate "qq", show a hint result at the top
            if (originalSearch != searchText && originalSearch.StartsWith("qq qq", StringComparison.OrdinalIgnoreCase))
            {
                var results = new List<Result>
                {
                    new Result
                    {
                        Title = "Duplicate 'qq' detected",
                        SubTitle = "Using '" + searchText + "' instead. No need to type 'qq' twice.",
                        IcoPath = IconPath,
                        Score = 5000,
                        Action = _ => false
                    }
                };

                // Add regular results after the hint
                results.AddRange(GetCommandResults(command, args, searchText));
                return results;
            }

            // Check if this is a partial command (not a full command but starts with valid command prefixes)
            if (parts.Length == 1 && !_commands.Contains(command))
            {
                var matchingCommands = _commands
                    .Where(cmd => cmd.StartsWith(command, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(cmd => cmd.Length)
                    .ToList();

                if (matchingCommands.Any())
                {
                    var results = new List<Result>();

                    foreach (var matchCommand in matchingCommands)
                    {
                        results.Add(new Result
                        {
                            Title = $"qq {matchCommand}",
                            SubTitle = _commandDescriptions.ContainsKey(matchCommand) 
                                ? _commandDescriptions[matchCommand] 
                                : $"Execute command '{matchCommand}'",
                            IcoPath = IconPath,
                            Score = 1000, // Very high score
                            Action = _ =>
                            {
                                Context?.API.ChangeQuery($"qq {matchCommand} ", true);
                                return false;
                            }
                        });
                    }

                    // Add the "add note" option with lower priority
                    results.Add(new Result
                    {
                        Title = $"Add note: {searchText}",
                        SubTitle = "Press Enter to save this note (with timestamp)",
                        IcoPath = IconPath,
                        Score = 50, // Lower score than commands
                        Action = _ =>
                        {
                            CreateNote(searchText);
                            Context?.API.ShowMsg("Note saved", $"Saved: {searchText}");
                            return true;
                        }
                    });

                    return results;
                }
            }

            // Regular processing
            return GetCommandResults(command, args, searchText);
        }

        // Helper method to centralize command processing
        private List<Result> GetCommandResults(string command, string args, string searchText)
        {
            switch (command)
            {
                case "help":
                    return HelpCommand();
                case "backup":
                case "export":
                    return BackupNotes();
                case "edit":
                    return EditNote(args);
                case "view":
                    return ViewNote(args);
                case "delall":
                    return DeleteAllNotes();
                case "del":
                case "delete":
                    return DeleteNote(args);
                case "search":
                    return SearchNotes(args);
                case "searchtag":
                    return SearchTag(args);
                case "pin":
                    return PinNote(args, true);
                case "unpin":
                    return PinNote(args, false);
                case "undo":
                    return UndoDelete();
                case "sort":
                    return SortNotes(args);
                case "tagstyle":
                    return ToggleTagStyle(args);
                default:
                    return AddNoteCommand(searchText);
            }
        }

        // --- Command Implementations ---

        private List<Result> AddNoteCommand(string noteText)
        {
            if (string.IsNullOrWhiteSpace(noteText))
            {
                return GetInstructionsAndNotes(string.Empty);
            }
            return new List<Result>
            {
                new Result
                {
                    Title = $"Add note: {noteText}",
                    SubTitle = "Press Enter to save this note (with timestamp)",
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        CreateNote(noteText);
                        Context?.API.ShowMsg("Note saved", $"Saved: {noteText}");
                        return true;
                    }
                }
            };
        }

        // Text formatting for display
        private string FormatTextForDisplay(string text)
        {
            // Bold formatting: **text** or __text__
            text = Regex.Replace(text, @"\*\*(.*?)\*\*|__(.*?)__", m => 
                $"【{(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)}】");

            // Italics formatting: *text* or _text_
            text = Regex.Replace(text, @"(?<!\*)\*(?!\*)(.*?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(.*?)(?<!_)_(?!_)", m => 
                $"〈{(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)}〉");

            // Highlight: ==text==
            text = Regex.Replace(text, @"==(.*?)==", m => $"《{m.Groups[1].Value}》");

            // Make hashtags bold or italic
            if (_useItalicForTags)
                text = Regex.Replace(text, @"(#\w+)", m => $"〈{m.Groups[1].Value}〉");
            else
                text = Regex.Replace(text, @"(#\w+)", m => $"【{m.Groups[1].Value}】");

            return text;
        }

        // Tag style toggling
        private List<Result> ToggleTagStyle(string style)
        {
            if (style.Equals("bold", StringComparison.OrdinalIgnoreCase))
            {
                _useItalicForTags = false;
                return SingleInfoResult("Tag style set to bold", "Tags will now appear as 【#tag】", true);
            }
            else if (style.Equals("italic", StringComparison.OrdinalIgnoreCase))
            {
                _useItalicForTags = true;
                return SingleInfoResult("Tag style set to italic", "Tags will now appear as 〈#tag〉", true);
            }
            else
            {
                return SingleInfoResult("Invalid tag style", "Use 'qq tagstyle bold' or 'qq tagstyle italic'");
            }
        }

        private List<Result> SearchNotes(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return SingleInfoResult("Search QuickNotes", "Usage: qq search <term>");
            }

            var notes = ReadNotes();
            var matchedNotes = notes
                .Where(n => n.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchedNotes.Count == 0)
            {
                return SingleInfoResult("No matches found", $"No notes contain '{searchTerm}'.");
            }

            var results = new List<Result>();
            foreach (var match in matchedNotes)
            {
                string highlighted = HighlightMatch(match.Text, searchTerm);
                results.Add(CreateNoteResult(match, $"Press Enter to copy | Shift+Enter for content only | Ctrl+Click to Edit", highlighted));
            }
            return results;
        }

        private string HighlightMatch(string noteText, string searchTerm)
        {
            var pattern = Regex.Escape(searchTerm);
            var highlighted = Regex.Replace(noteText, pattern,
                m => $"[{m.Value}]",
                RegexOptions.IgnoreCase);
            return highlighted;
        }

        private List<Result> SearchTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return SingleInfoResult("Search by Tag", "Usage: qq searchtag <tag> (e.g., qq searchtag work)");
            }

            string tagSearch = tag.StartsWith('#') ? tag : "#" + tag;
            var notes = ReadNotes();
            var matchedNotes = notes
                .Where(n => n.Text.Contains(tagSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchedNotes.Count == 0)
            {
                return SingleInfoResult("No matches found", $"No notes found with tag '{tagSearch}'.");
            }

            var results = new List<Result>();
            foreach (var match in matchedNotes)
            {
                string highlighted = HighlightMatch(match.Text, tagSearch);
                results.Add(CreateNoteResult(match, $"Found note with tag '{tagSearch}'. Enter to copy.", highlighted));
            }
            return results;
        }

        private void CreateNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return;
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string entry = $"[{timestamp}] {note.Trim()}";
                File.AppendAllText(_notesPath, entry + Environment.NewLine);
                _lastDeletedNote = null;
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error saving note", ex.Message);
            }
        }

        private List<Result> GetInstructionsAndNotes(string? currentSearch)
        {
            var results = new List<Result>();
            if (string.IsNullOrEmpty(currentSearch))
            {
                results.Add(HelpResult());
            }

            var notes = ReadNotes();
            var filteredNotes = string.IsNullOrWhiteSpace(currentSearch)
                ? notes
                : notes.Where(n => n.Text.Contains(currentSearch, StringComparison.OrdinalIgnoreCase)).ToList();

            var pinnedNotes = filteredNotes.Where(n => n.IsPinned).OrderByDescending(n => n.Timestamp).ToList();
            var regularNotes = filteredNotes.Where(n => !n.IsPinned).OrderByDescending(n => n.Timestamp).ToList();

            if (pinnedNotes.Any())
            {
                results.Add(new Result { Title = "--- Pinned Notes ---", IcoPath = IconPath, Action = _ => false });
                foreach (var note in pinnedNotes)
                {
                    string highlighted = string.IsNullOrWhiteSpace(currentSearch) 
                        ? note.Text 
                        : HighlightMatch(note.Text, currentSearch);
                    results.Add(CreateNoteResult(note, $"Pinned | Enter to copy clean content (no timestamp/tags) | Ctrl+C for full note | qq unpin {note.OriginalIndex + 1}", highlighted));
                }
            }

            if (regularNotes.Any())
            {
                if (pinnedNotes.Any())
                {
                    results.Add(new Result { Title = "--- Notes ---", IcoPath = IconPath, Action = _ => false });
                }
                foreach (var note in regularNotes)
                {
                    string highlighted = string.IsNullOrWhiteSpace(currentSearch) 
                        ? note.Text 
                        : HighlightMatch(note.Text, currentSearch);
                    results.Add(CreateNoteResult(note, $"Press Enter to copy without timestamp | Ctrl+C for full note | Ctrl+Click to Edit", highlighted));
                }
            }

            if (!pinnedNotes.Any() && !regularNotes.Any())
            {
                if (string.IsNullOrEmpty(currentSearch))
                {
                    results.Add(SingleInfoResult("No notes found", "Type 'qq <your note>' to add one, or 'qq help' for commands.").First());
                }
                else
                {
                    results.Add(SingleInfoResult($"No notes match '{currentSearch}'", "Try a different search or add a new note.").First());
                }
            }

            if (!string.IsNullOrWhiteSpace(currentSearch))
            {
                var commands = new[] { "help", "backup", "export", "edit", "view", "delall", "del", "delete", "search", "searchtag", "pin", "unpin", "undo", "sort", "tagstyle" };
                if (!commands.Contains(currentSearch.Split(' ')[0].ToLowerInvariant()))
                {
                    results.Insert(0, AddNoteCommand(currentSearch).First());
                }
            }

            return results;
        }

        // Helper to create a standard result for a note
        private Result CreateNoteResult(NoteEntry note, string subTitle, string? displayText = null)
        {
            // Apply text formatting for display
            string noteText = displayText ?? note.Text;
            string formattedText = FormatTextForDisplay(noteText);

            string title = $"{(note.IsPinned ? "[P] " : "")}[{note.OriginalIndex + 1}] {formattedText}";
            return new Result
            {
                Title = title,
                SubTitle = subTitle,
                IcoPath = IconPath,
                ToolTipData = new ToolTipData("Note Details", 
                    $"Index: {note.OriginalIndex + 1}\nPinned: {note.IsPinned}\nCreated: {(note.Timestamp != DateTime.MinValue ? note.Timestamp.ToString("g") : "Unknown")}\nText: {note.Text}\n\nTip: Right-click for copy options or edit."),
                ContextData = note,
                Action = c =>
                {
                    try
                    {

                        string contentOnly = StripTimestampAndTags(note.Text);  
                        Clipboard.SetText(contentOnly);
                        Context?.API.ShowMsg("Clean content copied", 
                            $"Copied without timestamp and tags: {contentOnly.Substring(0, Math.Min(contentOnly.Length, 50))}{(contentOnly.Length > 50 ? "..." : "")}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Context?.API.ShowMsg("Error", "Failed to copy note to clipboard: " + ex.Message);
                        return false;
                    }
                }
            };
        }

        private List<Result> DeleteNote(string indexStr)
        {
            if (!TryParseNoteIndex(indexStr, out int index, out var errorResult))
                return errorResult;

            var notes = ReadNotes();
            if (index < 0 || index >= notes.Count)
            {
                return SingleInfoResult("Note not found", $"Note number {index + 1} is invalid. Max index is {notes.Count}.");
            }

            try
            {
                var noteToRemove = notes[index];
                _lastDeletedNote = (noteToRemove.ToFileLine(), noteToRemove.OriginalIndex, noteToRemove.IsPinned);
                var updatedNotes = notes.Where((note, i) => i != index).Select(n => n.ToFileLine()).ToList();
                WriteNotes(updatedNotes);
                return SingleInfoResult("Note deleted", $"Removed: [{index + 1}] {noteToRemove.Text}\nTip: Use 'qq undo' to restore.", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error deleting note", ex.Message);
            }
        }

        private List<Result> DeleteAllNotes()
        {
            try
            {
                var notes = ReadNotes();
                if (!notes.Any())
                {
                    return SingleInfoResult("No notes to delete", "Your notes file is already empty.");
                }

                _lastDeletedNote = null;
                WriteNotes(new List<string>());
                return SingleInfoResult("All notes deleted", $"Removed {notes.Count} notes. This action cannot be undone with 'qq undo'.", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error deleting all notes", ex.Message);
            }
        }

        private List<Result> UndoDelete()
        {
            if (_lastDeletedNote == null)
            {
                return SingleInfoResult("Nothing to undo", "No note has been deleted recently.");
            }

            try
            {
                var notes = ReadNotesRaw();
                var (text, index, _) = _lastDeletedNote.Value;

                if (index >= 0 && index <= notes.Count)
                {
                    notes.Insert(index, text);
                }
                else
                {
                    notes.Add(text);
                }

                WriteNotes(notes);
                _lastDeletedNote = null;
                return SingleInfoResult("Note restored", $"Restored note at index ~{index + 1}.", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error undoing delete", ex.Message);
            }
        }

        private List<Result> PinNote(string indexStr, bool pin)
        {
            if (!TryParseNoteIndex(indexStr, out int index, out var errorResult))
                return errorResult;

            var notes = ReadNotes();
            if (index < 0 || index >= notes.Count)
            {
                return SingleInfoResult("Note not found", $"Note number {index + 1} is invalid. Max index is {notes.Count}.");
            }

            try
            {
                var noteToUpdate = notes[index];
                if (noteToUpdate.IsPinned == pin)
                {
                    return SingleInfoResult($"Note {index + 1} already {(pin ? "pinned" : "unpinned")}", noteToUpdate.Text);
                }

                noteToUpdate.IsPinned = pin;
                WriteNotes(notes.Select(n => n.ToFileLine()).ToList());
                _lastDeletedNote = null;
                return SingleInfoResult($"Note {(pin ? "pinned" : "unpinned")}", $"[{index + 1}] {noteToUpdate.Text}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult($"Error {(pin ? "pinning" : "unpinning")} note", ex.Message);
            }
        }

        private List<Result> SortNotes(string args)
        {
            var notes = ReadNotes();
            if (!notes.Any()) return SingleInfoResult("No notes to sort", "");

            string sortType = args.ToLowerInvariant().Trim();
            bool descending = sortType.EndsWith(" desc");
            if (descending) sortType = sortType.Substring(0, sortType.Length - 5).Trim();
            bool ascending = sortType.EndsWith(" asc");
            if (ascending) sortType = sortType.Substring(0, sortType.Length - 4).Trim();

            IEnumerable<NoteEntry> sortedNotes;
            switch (sortType)
            {
                case "date":
                    sortedNotes = descending
                        ? notes.OrderByDescending(n => n.Timestamp == DateTime.MinValue ? DateTime.MaxValue : n.Timestamp)
                               .ThenBy(n => n.OriginalIndex)
                        : notes.OrderBy(n => n.Timestamp == DateTime.MinValue ? DateTime.MaxValue : n.Timestamp)
                               .ThenBy(n => n.OriginalIndex);
                    break;
                case "alpha":
                case "text":
                    sortedNotes = descending
                        ? notes.OrderByDescending(n => n.Text, StringComparer.OrdinalIgnoreCase)
                               .ThenBy(n => n.OriginalIndex)
                        : notes.OrderBy(n => n.Text, StringComparer.OrdinalIgnoreCase)
                               .ThenBy(n => n.OriginalIndex);
                    break;
                default:
                    return SingleInfoResult("Invalid sort type", "Use 'qq sort date [asc|desc]' or 'qq sort alpha [asc|desc]'");
            }

            try
            {
                WriteNotes(sortedNotes.Select(n => n.ToFileLine()).ToList());
                _lastDeletedNote = null;
                return SingleInfoResult("Notes sorted", $"Sorted by {sortType} {(descending ? "descending" : (ascending ? "ascending" : "(default asc)"))}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error sorting notes", ex.Message);
            }
        }

        private List<Result> EditNote(string noteNumberStr)
        {
            if (!TryParseNoteIndex(noteNumberStr, out int index, out var errorResult))
                return errorResult;

            var notes = ReadNotes();
            if (index < 0 || index >= notes.Count)
            {
                return SingleInfoResult("Note not found", $"Note number {index + 1} is invalid. Max index is {notes.Count}.");
            }

            var noteToEdit = notes[index];
            string oldNoteText = noteToEdit.Text;
            string newNoteText = Interaction.InputBox($"Edit note #{noteToEdit.OriginalIndex + 1}", "Edit QuickNote", oldNoteText);

            if (string.IsNullOrEmpty(newNoteText) || newNoteText == oldNoteText)
            {
                return SingleInfoResult("Edit cancelled", "Note was not changed.");
            }

            string timestampPrefix = "";
            if (noteToEdit.Timestamp != DateTime.MinValue && noteToEdit.Text.StartsWith("["))
            {
                timestampPrefix = noteToEdit.Text.Substring(0, 22);
            }
            noteToEdit.Text = timestampPrefix + newNoteText.Trim();

            try
            {
                WriteNotes(notes.Select(n => n.ToFileLine()).ToList());
                _lastDeletedNote = null;
                return SingleInfoResult("Note edited", $"Updated note #{index + 1}: {newNoteText}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error saving edited note", ex.Message);
            }
        }

        // Helper for inline editing (no modifier actions, since they are not supported)
        private void EditNoteInline(NoteEntry note)
        {
            string oldNoteText = note.Text;
            string newNoteText = Interaction.InputBox($"Edit note #{note.OriginalIndex + 1}", "Edit QuickNote", oldNoteText);

            if (string.IsNullOrEmpty(newNoteText) || newNoteText == oldNoteText)
            {
                Context?.API.ShowMsg("Edit cancelled", "Note was not changed.", IconPath);
                return;
            }

            string timestampPrefix = "";
            if (note.Timestamp != DateTime.MinValue && note.Text.StartsWith("["))
            {
                timestampPrefix = note.Text.Substring(0, 22);
            }
            note.Text = timestampPrefix + newNoteText.Trim();

            try
            {
                var allNotes = ReadNotes();
                var noteInCurrentList = allNotes.FirstOrDefault(n => n.OriginalIndex == note.OriginalIndex);
                if (noteInCurrentList != null)
                {
                    noteInCurrentList.Text = note.Text;
                    WriteNotes(allNotes.Select(n => n.ToFileLine()).ToList());
                    _lastDeletedNote = null;
                    Context?.API.ShowMsg("Note edited", $"Updated note #{note.OriginalIndex + 1}", IconPath);
                }
                else
                {
                    Context?.API.ShowMsg("Error", "Could not find the note to save the edit.", IconPath);
                }
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error saving note", ex.Message, IconPath);
            }
        }

        private List<Result> ViewNote(string noteNumberStr)
        {
            if (!TryParseNoteIndex(noteNumberStr, out int index, out var errorResult))
                return errorResult;

            var notes = ReadNotes();
            if (index < 0 || index >= notes.Count)
            {
                return SingleInfoResult("Note not found", $"Note number {index + 1} is invalid. Max index is {notes.Count}.");
            }

            var note = notes[index];
            return new List<Result> { CreateNoteResult(note, "Press Enter to copy without timestamp | Ctrl+C for full note | Right-click for more options") };
        }

        private List<Result> BackupNotes()
        {
            try
            {
                if (!File.Exists(_notesPath))
                {
                    return SingleInfoResult("No notes file to backup", "The notes file doesn't exist.");
                }

                string notesDir = Path.GetDirectoryName(_notesPath)!;
                string backupFileName = Path.Combine(notesDir, $"notes_backup_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.Copy(_notesPath, backupFileName, true);

                // Open Windows Explorer with the backup file selected
                Process.Start("explorer.exe", $"/select,\"{backupFileName}\"");

                // Open the backup file in the default application
                Process.Start(new ProcessStartInfo
                {
                    FileName = backupFileName,
                    UseShellExecute = true
                });

                return SingleInfoResult("Backup created", $"Backup saved to {Path.GetFileName(backupFileName)} in QuickNotes folder.", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error creating backup", ex.Message);
            }
        }

        private List<Result> HelpCommand()
        {
            return new List<Result> { HelpResult() };
        }

        // Centralized Help Result
        private Result HelpResult()
        {
            string helpText = 
                "qq <text>              Add note ✏️       | qq pin <N>             Pin note 📌\n" + 
                "qq search <term>       Search notes 🔍   | qq unpin <N>           Unpin note 📎\n" + 
                "qq searchtag <tag>     Search #tags 🏷️   | qq sort date|alpha     Sort notes 🔄\n" + 
                "qq view <N>            View note 👁️      | qq undo                Restore note ↩️\n" + 
                "qq edit <N>            Edit note 📝      | qq delall              Delete ALL 💣\n" + 
                "qq del <N>             Delete note 🗑️    | qq backup/export       Backup notes 💾\n" + 
                "qq help                Show help ℹ️      | qq tagstyle bold/italic Tag style ✨\n" +
                "\nFormatting: **bold** or __bold__, *italic* or _italic_, ==highlight==, #tag\n" +
                "TIP: Right-click on a note for copy options (with/without timestamp)";

            return new Result
            {
                Title = "QuickNotes Help",
                SubTitle = helpText,
                IcoPath = IconPath,
                Action = _ => false
            };
        }

        // --- File I/O Helpers ---
        private List<NoteEntry> ReadNotes()
        {
            try
            {
                if (!File.Exists(_notesPath)) return new List<NoteEntry>();
                return File.ReadAllLines(_notesPath)
                           .Select((line, index) => NoteEntry.Parse(line, index))
                           .Where(entry => !string.IsNullOrWhiteSpace(entry.Text))
                           .ToList();
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error reading notes", ex.Message);
                return new List<NoteEntry>();
            }
        }

        private List<string> ReadNotesRaw()
        {
            try
            {
                if (!File.Exists(_notesPath)) return new List<string>();
                return File.ReadAllLines(_notesPath).ToList();
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error reading notes", ex.Message);
                return new List<string>();
            }
        }

        private void WriteNotes(IEnumerable<string> lines)
        {
            try
            {
                File.WriteAllLines(_notesPath, lines);
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error saving notes", $"Failed to write to notes file: {ex.Message}");
                throw;
            }
        }

        // --- Utility Helpers ---
        private bool TryParseNoteIndex(string indexStr, out int index, out List<Result> errorResult)
        {
            if (!int.TryParse(indexStr, out int oneBasedIndex) || oneBasedIndex <= 0)
            {
                errorResult = SingleInfoResult("Invalid note number", "Please specify a valid positive number corresponding to the note.");
                index = -1;
                return false;
            }
            index = oneBasedIndex - 1;
            errorResult = new List<Result>();
            return true;
        }

        private List<Result> SingleInfoResult(string title, string subTitle, bool closeWindow = false)
        {
            return new List<Result>
            {
                new Result
                {
                    Title = title,
                    SubTitle = subTitle,
                    IcoPath = IconPath,
                    Action = _ => closeWindow
                }
            };
        }

        private List<Result> ErrorResult(string title, string subTitle)
        {
            return new List<Result>
            {
                new Result
                {
                    Title = title,
                    SubTitle = subTitle,
                    IcoPath = IconPath,
                    Action = _ => { Context?.API.ShowMsg(title, subTitle); return false; }
                }
            };
        }

        // --- Context Menu ---
                public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
                {
                    var contextMenuItems = new List<ContextMenuResult>();
                    if (selectedResult.ContextData is NoteEntry note)
                    {
                        // CHANGED: Make this the primary shortcut (Ctrl+C)
                        contextMenuItems.Add(new ContextMenuResult
                        {
                            PluginName = Name,
                            Title = "Copy Full Note (with timestamp)",
                            FontFamily = "Segoe MDL2 Assets",
                            Glyph = "\uE8C8", // Copy icon
                            AcceleratorKey = Key.C,
                            AcceleratorModifiers = ModifierKeys.Control,
                            Action = _ =>
                            {
                                try 
                                { 
                                    Clipboard.SetText(note.Text); 
                                    Context?.API.ShowMsg("Full note copied", 
                                        $"Copied with timestamp: {note.Text.Substring(0, Math.Min(note.Text.Length, 50))}{(note.Text.Length > 50 ? "..." : "")}");
                                    return true; 
                                }
                                catch (Exception ex) 
                                { 
                                    Context?.API.ShowMsg("Error", "Failed to copy: " + ex.Message); 
                                    return false; 
                                }
                            }
                        });

                        // Less prominent second option with Ctrl+Shift+C 
                        string contentOnly = StripTimestampAndTags(note.Text);
                        contextMenuItems.Add(new ContextMenuResult
                        {
                            PluginName = Name,
                            Title = "Copy Clean Content (no timestamp, no tags) (already default on Enter)",
                            FontFamily = "Segoe MDL2 Assets",
                            Glyph = "\uE8C9", // Document icon
                            AcceleratorKey = Key.C,
                            AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                            Action = _ =>
                            {
                                try 
                                { 
                                    Clipboard.SetText(contentOnly); 
                                    Context?.API.ShowMsg("Content copied", 
                                        $"Copied without timestamp: {contentOnly.Substring(0, Math.Min(contentOnly.Length, 50))}{(contentOnly.Length > 50 ? "..." : "")}");
                                    return true; 
                                }
                                catch (Exception ex) 
                                { 
                                    Context?.API.ShowMsg("Error", "Failed to copy: " + ex.Message);
                                    return false; 
                                }
                            }
                        });

                contextMenuItems.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Edit Note...",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\uE70F", // Edit icon
                    AcceleratorKey = Key.E,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        EditNoteInline(note);
                        return true;
                    }
                });

                contextMenuItems.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Delete Note",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\uE74D", // Delete icon
                    AcceleratorKey = Key.Delete,
                    Action = _ =>
                    {
                        DeleteNote((note.OriginalIndex + 1).ToString());
                        return true;
                    }
                });

                contextMenuItems.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = note.IsPinned ? "Unpin Note" : "Pin Note",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = note.IsPinned ? "\uE77A" : "\uE718", // Pin/Unpin icon
                    Action = _ =>
                    {
                        PinNote((note.OriginalIndex + 1).ToString(), !note.IsPinned);
                        return true;
                    }
                });

                // URL detection and opening
                Match urlMatch = UrlRegex.Match(note.Text);
                if (urlMatch.Success)
                {
                    string url = urlMatch.Value;
                    if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase) && !url.Contains("://"))
                    {
                        url = "http://" + url;
                    }

                    contextMenuItems.Add(new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = $"Open URL: {url.Substring(0, Math.Min(url.Length, 40))}{(url.Length > 40 ? "..." : "")}",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\uE774", // Globe icon
                        AcceleratorKey = Key.U,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            try
                            {
                                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Context?.API.ShowMsg("Error", $"Could not open URL: {ex.Message}");
                                return false;
                            }
                        }
                    });
                }
            }
            return contextMenuItems;
        }

        // --- IPluginI18n & IDisposable ---
        public string GetTranslatedPluginTitle() => Name;
        public string GetTranslatedPluginDescription() => Description;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }
            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }
            Disposed = true;
        }

        // --- Theme Handling ---
        private void UpdateIconPath(Theme theme) =>
            IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
                ? "Images/quicknotes.light.png"
                : "Images/quicknotes.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) =>
            UpdateIconPath(newTheme);
    }
}