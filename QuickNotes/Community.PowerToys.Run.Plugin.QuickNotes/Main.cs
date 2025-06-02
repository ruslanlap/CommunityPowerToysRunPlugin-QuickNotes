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
    // Note structure now includes a persistent GUID-based ID
    internal class NoteEntry
    {
        // Унікальний незмінний ідентифікатор (GUID)
        public string Id { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        public int DisplayIndex { get; set; } // Індекс для відображення користувачу
        public DateTime Timestamp { get; set; } = DateTime.MinValue; // Parsed timestamp

        // Формує унікальний ContentId (за потреби), але основний ідентифікатор — GUID
        public string ContentId => $"{Id}_{(IsPinned ? "PINNED" : "")}_{Text.Trim()}";

        /// <summary>
        /// Парсить рядок у форматі:
        /// [id:<GUID>] [PINNED] [YYYY-MM-DD HH:MM:SS] текст нотатки
        /// </summary>
        internal static NoteEntry Parse(string line)
        {
            var entry = new NoteEntry();
            string remaining = line.Trim();

            // 1. Витягаємо GUID (id)
            var idPattern = new Regex(@"^\[id:(.+?)\]\s*", RegexOptions.Compiled);
            var idMatch = idPattern.Match(remaining);
            if (idMatch.Success)
            {
                entry.Id = idMatch.Groups[1].Value;
                remaining = remaining.Substring(idMatch.Length);
            }

            // 2. Перевіряємо на закріплену нотатку
            const string pinnedMarker = "[PINNED] ";
            if (remaining.StartsWith(pinnedMarker, StringComparison.OrdinalIgnoreCase))
            {
                entry.IsPinned = true;
                remaining = remaining.Substring(pinnedMarker.Length);
            }

            // 3. Перевіряємо на наявність timestamp (формат "[YYYY-MM-DD HH:MM:SS] ")
            if (remaining.Length > 22 && remaining[0] == '[' && remaining[20] == ']' && remaining[21] == ' ')
            {
                if (DateTime.TryParse(remaining.Substring(1, 19), out DateTime ts))
                {
                    entry.Timestamp = ts;
                    entry.Text = remaining; // зберігаємо весь рядок (з timestamp та текстом)
                }
                else
                {
                    entry.Text = remaining; // якщо не вдалось розпарсити timestamp
                }
            }
            else
            {
                entry.Text = remaining; // якщо timestamp відсутній
            }

            return entry;
        }

        /// <summary>
        /// Формує рядок для збереження у файл:
        /// [id:<GUID>] [PINNED] [YYYY-MM-DD HH:MM:SS] текст нотатки
        /// </summary>
        internal string ToFileLine()
        {
            var sb = new StringBuilder();
            sb.Append($"[id:{Id}] ");
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
        private static readonly Regex UrlRegex = new Regex(@"\b(https?://|www\.)\S+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const string NotesFileName = "notes.txt"; // Центральний файл

        // --- Properties ---
        public string Name => "QuickNotes";
        public string Description => "Save, view, manage, search, tag, and pin quick notes";

        private PluginInitContext? Context { get; set; }
        private string? IconPath { get; set; }
        private bool Disposed { get; set; }

        private string _notesPath = string.Empty;
        private bool _isInitialized = false;

        // --- Стан для Undo ---
        private (string Text, int RawIndex, bool WasPinned)? _lastDeletedNoteRaw;

        // Форматування тегів
        private bool _useItalicForTags = false; // Використовується у FormatTextForDisplay

        // Команди автодоповнення
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

        // Опис команд
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

        // --- Ініціалізація плагіна ---
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
                    Directory.CreateDirectory(powerToysPath);

                _notesPath = Path.Combine(powerToysPath, NotesFileName);
                if (!File.Exists(_notesPath))
                    File.WriteAllText(_notesPath, string.Empty);

                try
                {
                    File.AppendAllText(_notesPath, string.Empty); // Перевірка доступу на запис
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

        // Допоміжний метод: відкинути timestamp
        private string StripTimestamp(string noteText)
        {
            // Формат timestamp: [YYYY-MM-DD HH:MM:SS] 
            if (noteText.Length >= 22 && noteText[0] == '[' && noteText[20] == ']' && noteText[21] == ' ')
                return noteText.Substring(22).Trim();
            return noteText.Trim();
        }

        // Допоміжний метод: відкинути timestamp і #теги
        private string StripTimestampAndTags(string noteText)
        {
            var withoutTs = StripTimestamp(noteText);
            var withoutTags = Regex.Replace(withoutTs, @"#\w+\s*", "");
            return withoutTags.Trim();
        }

        // Method for providing autocomplete suggestions
        public List<Result> GetQuerySuggestions(Query query, bool execute)
        {
            if (!_isInitialized)
                return new List<Result>();

            var originalSearch = query.Search?.Trim() ?? string.Empty;
            var searchText = CleanupQuery(originalSearch);
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<Result>();

            var suggestions = new List<Result>();
            var parts = searchText.Split(new[] { ' ' }, 2);
            var possibleCommand = parts[0].ToLowerInvariant();

            // If only one word, suggest commands
            if (parts.Length == 1)
            {
                var matchingCommands = _commands
                    .Where(cmd => cmd.StartsWith(possibleCommand, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(cmd => cmd.Length)
                    .ToList();

                bool hasExact = matchingCommands.Contains(possibleCommand, StringComparer.OrdinalIgnoreCase);
                foreach (var cmd in matchingCommands)
                {
                    if (hasExact && cmd.Equals(possibleCommand, StringComparison.OrdinalIgnoreCase) && matchingCommands.Count > 1)
                        continue;

                    suggestions.Add(new Result
                    {
                        Title = cmd,
                        SubTitle = _commandDescriptions.ContainsKey(cmd) ? _commandDescriptions[cmd] : $"Execute command '{cmd}'",
                        IcoPath = IconPath,
                        Score = 1000,
                        Action = _ =>
                        {
                            if (execute)
                            {
                                Context?.API.ChangeQuery($"qq {cmd} ", true);
                                return true;
                            }
                            Context?.API.ChangeQuery($"qq {cmd} ", false);
                            return false;
                        }
                    });
                }
            }

            if (suggestions.Count > 0)
                return suggestions;

            // If no command suggestions, offer to add a note
            if (!_commands.Contains(possibleCommand, StringComparer.OrdinalIgnoreCase) || parts.Length > 1)
            {
                suggestions.Add(new Result
                {
                    Title = $"Add note: {searchText}",
                    SubTitle = "Press Enter to save this note (with timestamp)",
                    IcoPath = IconPath,
                    Score = 10,
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

        public int GetPriority(Query query) => 0;

        // Method to clean up duplicate "qq" prefixes
        private string CleanupQuery(string query)
        {
            if (query.StartsWith("qq ", StringComparison.OrdinalIgnoreCase)
                && query.Length > 3
                && query.Substring(3).TrimStart().StartsWith("qq", StringComparison.OrdinalIgnoreCase))
            {
                int pos = query.IndexOf("qq", 3, StringComparison.OrdinalIgnoreCase);
                if (pos >= 0)
                    return query.Substring(3).Trim();
            }
            return query.Trim();
        }

        public List<Result> Query(Query query, bool delayedExecution) => Query(query);

        public List<Result> Query(Query query)
        {
            if (!_isInitialized)
                return ErrorResult("QuickNotes not initialized", "Plugin not initialized properly. Please restart PowerToys.");

            var originalSearch = query.Search?.Trim() ?? string.Empty;
            var searchText = CleanupQuery(originalSearch);

            if (string.IsNullOrEmpty(searchText))
                return GetInstructionsAndNotes(string.Empty);

            var parts = searchText.Split(new[] { ' ' }, 2);
            var command = parts[0].ToLowerInvariant();
            var args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            if (originalSearch != searchText && originalSearch.StartsWith("qq qq", StringComparison.OrdinalIgnoreCase))
            {
                var resultsHint = new List<Result>
                {
                    new Result
                    {
                        Title = "Duplicate 'qq' detected",
                        SubTitle = $"Using '{searchText}' instead. No need to type 'qq' twice.",
                        IcoPath = IconPath,
                        Score = 5000,
                        Action = _ => false
                    }
                };
                resultsHint.AddRange(GetCommandResults(command, args, searchText));
                return resultsHint;
            }

            if (parts.Length == 1 && !_commands.Contains(command))
            {
                var matching = _commands
                    .Where(cmd => cmd.StartsWith(command, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(cmd => cmd.Length)
                    .ToList();
                if (matching.Any())
                {
                    var results = new List<Result>();
                    foreach (var cmd in matching)
                    {
                        results.Add(new Result
                        {
                            Title = cmd,
                            SubTitle = _commandDescriptions.ContainsKey(cmd) ? _commandDescriptions[cmd] : $"Execute command '{cmd}'",
                            IcoPath = IconPath,
                            Score = 1000,
                            Action = _ =>
                            {
                                Context?.API.ChangeQuery($"qq {cmd} ", true);
                                return false;
                            }
                        });
                    }
                    results.Add(new Result
                    {
                        Title = $"Add note: {searchText}",
                        SubTitle = "Press Enter to save this note (with timestamp)",
                        IcoPath = IconPath,
                        Score = 50,
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

            return GetCommandResults(command, args, searchText);
        }

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
                    if (args.EndsWith(" --confirm"))
                    {
                        var cleanArgs = args.Replace(" --confirm", "").Trim();
                        return DeleteNote(cleanArgs, true);
                    }
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

        // --- Додавання нотатки з GUID ---
        private void CreateNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return;

            try
            {
                // Зараз усі Id починаються з 'Q'
                var newId = "Q" + Guid.NewGuid().ToString("N"); // Унікальний GUID без дефісів + префікс Q
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var entryLine = $"[id:{newId}] [{timestamp}] {note.Trim()}";

                using (var fs = new FileStream(_notesPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fs))
                {
                    writer.WriteLine(entryLine);
                    writer.Flush();
                }

                _lastDeletedNoteRaw = null; // Скидаємо Undo-буфер
            }
            catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32)
            {
                Context?.API.ShowMsg("File in use", "Notes file is used by another process. Please try again shortly.");
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error creating note", ex.Message);
            }
        }

        private List<Result> AddNoteCommand(string noteText)
        {
            if (string.IsNullOrWhiteSpace(noteText))
                return GetInstructionsAndNotes(string.Empty);

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

        // --- Пошук нотаток ---
        private List<Result> SearchNotes(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return SingleInfoResult("Search QuickNotes", "Usage: qq search <term>");

            var notes = ReadNotes();
            var matches = notes
                .Where(n => n.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matches.Any())
                return SingleInfoResult("No matches found", $"No notes contain '{searchTerm}'.");

            var results = new List<Result>();
            foreach (var match in matches)
            {
                var highlighted = HighlightMatch(match.Text, searchTerm);
                results.Add(CreateNoteResult(match,
                    $"Press Enter to copy | Shift+Enter for content only | Ctrl+Click to Edit",
                    highlighted));
            }
            return results;
        }

        private string HighlightMatch(string noteText, string searchTerm)
        {
            var pattern = Regex.Escape(searchTerm);
            return Regex.Replace(noteText, pattern, m => $"[{m.Value}]", RegexOptions.IgnoreCase);
        }

        private List<Result> SearchTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return SingleInfoResult("Search by Tag", "Usage: qq searchtag <tag> (e.g., qq searchtag work)");

            var tagSearch = tag.StartsWith('#') ? tag : "#" + tag;
            var notes = ReadNotes();
            var matches = notes
                .Where(n => n.Text.Contains(tagSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matches.Any())
                return SingleInfoResult("No matches found", $"No notes found with tag '{tagSearch}'.");

            var results = new List<Result>();
            foreach (var match in matches)
            {
                var highlighted = HighlightMatch(match.Text, tagSearch);
                results.Add(CreateNoteResult(match, $"Found note with tag '{tagSearch}'. Enter to copy.", highlighted));
            }
            return results;
        }

        // --- Форматування тексту для відображення ---
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

            // Make hashtags bold or italic based on _useItalicForTags
            if (_useItalicForTags)
                text = Regex.Replace(text, @"(#\w+)", m => $"〈{m.Groups[1].Value}〉");
            else
                text = Regex.Replace(text, @"(#\w+)", m => $"【{m.Groups[1].Value}】");

            return text;
        }

        // --- Перемикання стилю відображення тегів ---
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

        // --- Відображення інструкцій та нотаток ---
        private List<Result> GetInstructionsAndNotes(string? currentSearch)
        {
            var results = new List<Result>();
            if (string.IsNullOrEmpty(currentSearch))
                results.Add(HelpResult());

            var notes = ReadNotes();
            var filtered = string.IsNullOrWhiteSpace(currentSearch)
                ? notes
                : notes.Where(n => n.Text.Contains(currentSearch, StringComparison.OrdinalIgnoreCase)).ToList();

            var pinned = filtered.Where(n => n.IsPinned).OrderByDescending(n => n.Timestamp).ToList();
            var regular = filtered.Where(n => !n.IsPinned).OrderByDescending(n => n.Timestamp).ToList();

            if (pinned.Any())
            {
                results.Add(new Result { Title = "--- Pinned Notes ---", IcoPath = IconPath, Action = _ => false });
                foreach (var note in pinned)
                {
                    var highlighted = string.IsNullOrWhiteSpace(currentSearch)
                        ? note.Text
                        : HighlightMatch(note.Text, currentSearch);

                    results.Add(CreateNoteResult(note,
                        $"Pinned | Enter to copy clean content (no timestamp/tags) | Ctrl+Click to Edit | qq unpin {note.DisplayIndex}",
                        highlighted));
                }
            }

            if (regular.Any())
            {
                if (pinned.Any())
                    results.Add(new Result { Title = "--- Notes ---", IcoPath = IconPath, Action = _ => false });

                foreach (var note in regular)
                {
                    var highlighted = string.IsNullOrWhiteSpace(currentSearch)
                        ? note.Text
                        : HighlightMatch(note.Text, currentSearch);

                    results.Add(CreateNoteResult(note,
                        $"Press Enter to copy without timestamp | Ctrl+C for full note | Ctrl+Click to Edit",
                        highlighted));
                }
            }

            if (!pinned.Any() && !regular.Any())
            {
                if (string.IsNullOrEmpty(currentSearch))
                    results.Add(SingleInfoResult("No notes found", "Type 'qq <your note>' to add one, or 'qq help' for commands.").First());
                else
                    results.Add(SingleInfoResult($"No notes match '{currentSearch}'", "Try a different search or add a new note.").First());
            }

            if (!string.IsNullOrWhiteSpace(currentSearch))
            {
                var cmds = new[]
                {
                    "help", "backup", "export", "edit", "view", "delall", "del", "delete", "search", "searchtag", "pin", "unpin", "undo", "sort", "tagstyle"
                };
                if (!cmds.Contains(currentSearch.Split(' ')[0].ToLowerInvariant()))
                    results.Insert(0, AddNoteCommand(currentSearch).First());
            }

            return results;
        }

        // Формує Result для однієї нотатки
        private Result CreateNoteResult(NoteEntry note, string subTitle, string? displayText = null, Func<ActionContext, bool>? customAction = null)
        {
            var noteText = displayText ?? note.Text;
            var formatted = FormatTextForDisplay(noteText);
            var title = $"{(note.IsPinned ? "[P] " : "")}[{note.DisplayIndex}] {formatted}";

            return new Result
            {
                Title = title,
                SubTitle = subTitle,
                IcoPath = IconPath,
                ToolTipData = new ToolTipData(
                    "Note Details",
                    $"ID: {note.Id}\nDisplay Index: {note.DisplayIndex}\nPinned: {note.IsPinned}\nCreated: {(note.Timestamp != DateTime.MinValue ? note.Timestamp.ToString("g") : "Unknown")}\nText: {note.Text}\n\nTip: Right-click for copy options or edit."
                ),
                ContextData = note,
                Action = customAction ?? (c =>
                {
                    try
                    {
                        var contentOnly = StripTimestampAndTags(note.Text);
                        Clipboard.SetText(contentOnly);
                        Context?.API.ShowMsg("Clean content copied",
                            $"Copied without timestamp and tags: {contentOnly.Substring(0, Math.Min(contentOnly.Length, 50))}{(contentOnly.Length > 50 ? "..." : "")}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Context?.API.ShowMsg("Error", $"Failed to copy note: {ex.Message}");
                        return false;
                    }
                })
            };
        }

        // --- Оновлена логіка видалення нотатки за DisplayIndex, без зміщення індексів ---
        private List<Result> DeleteNote(string indexOrText, bool confirmed = false)
        {
            // Якщо аргумент конвертується в число → спочатку знаходимо по DisplayIndex
            if (int.TryParse(indexOrText, out int displayIndex) && displayIndex > 0)
            {
                // Зчитуємо сирі рядки
                var rawLines = ReadNotesRaw();
                // Будуємо мапу DisplayIndex → (NoteEntry, rawIndex)
                var indexedNotes = new List<(NoteEntry entry, int rawIndex)>();
                int di = 1;
                for (int i = 0; i < rawLines.Count; i++)
                {
                    var line = rawLines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var entry = NoteEntry.Parse(line);
                    entry.DisplayIndex = di++;
                    indexedNotes.Add((entry, i));
                }

                // Шукаємо пару за DisplayIndex
                var pair = indexedNotes.FirstOrDefault(p => p.entry.DisplayIndex == displayIndex);
                if (pair.entry == null || pair.entry.DisplayIndex != displayIndex)
                {
                    var available = indexedNotes.Select(p => p.entry.DisplayIndex).OrderBy(x => x).ToList();
                    return SingleInfoResult("Note not found",
                        $"Note number {displayIndex} does not exist.\nAvailable: {string.Join(", ", available)}");
                }

                var noteToRemove = pair.entry;
                var rawIndexToRemove = pair.rawIndex;

                if (!confirmed)
                {
                    // Показуємо підтвердження
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "Confirm Deletion",
                            SubTitle = $"Are you sure you want to delete this note?\n\nNote: {Truncate(noteToRemove.Text, 100)}\nPress Enter to confirm or Esc to cancel.",
                            IcoPath = IconPath,
                            Score = 1000,
                            Action = _ =>
                            {
                                // Викликаємо вдруге з --confirm
                                Context?.API.ChangeQuery($"qq del {displayIndex} --confirm", true);
                                return false;
                            }
                        }
                    };
                }

                // Після підтвердження – видаляємо рядок за rawIndex
                return DeleteSpecificLine(rawLines, noteToRemove, rawIndexToRemove);
            }
            else
            {
                // Якщо аргумент не число, шукаємо за текстом (exact / partial)
                var notes = ReadNotes();
                var exactMatches = notes.Where(n => StripTimestampAndTags(n.Text)
                                                    .Equals(indexOrText, StringComparison.OrdinalIgnoreCase))
                                        .ToList();

                if (exactMatches.Count == 1)
                {
                    var rawLines = ReadNotesRaw();
                    var indexedNotes = new List<(NoteEntry entry, int rawIndex)>();
                    int di = 1;
                    for (int i = 0; i < rawLines.Count; i++)
                    {
                        var line = rawLines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var entry = NoteEntry.Parse(line);
                        entry.DisplayIndex = di++;
                        indexedNotes.Add((entry, i));
                    }

                    var matchingPair = indexedNotes.FirstOrDefault(p =>
                        string.Equals(StripTimestampAndTags(p.entry.Text), StripTimestampAndTags(exactMatches[0].Text), StringComparison.OrdinalIgnoreCase)
                        && p.entry.IsPinned == exactMatches[0].IsPinned);

                    if (matchingPair.entry != null)
                        return DeleteSpecificLine(rawLines, matchingPair.entry, matchingPair.rawIndex);
                    else
                        return ErrorResult("Error deleting note",
                            "Could not find the exact note in the file. File may have been modified. Type 'qq' to refresh.");
                }
                else if (exactMatches.Count > 1)
                {
                    var results = new List<Result>
                    {
                        new Result
                        {
                            Title = "Multiple exact matching notes found",
                            SubTitle = "Please select a specific note to delete:",
                            IcoPath = IconPath
                        }
                    };

                    foreach (var match in exactMatches)
                    {
                        results.Add(CreateNoteResult(match, "Press Enter to delete", null, (ctx) =>
                        {
                            DeleteNote(match.DisplayIndex.ToString());
                            return true;
                        }));
                    }
                    return results;
                }
                else
                {
                    var partialMatches = notes.Where(n => StripTimestampAndTags(n.Text)
                                                          .Contains(indexOrText, StringComparison.OrdinalIgnoreCase))
                                              .ToList();

                    if (partialMatches.Count == 1)
                    {
                        var rawLines = ReadNotesRaw();
                        var indexedNotes = new List<(NoteEntry entry, int rawIndex)>();
                        int di = 1;
                        for (int i = 0; i < rawLines.Count; i++)
                        {
                            var line = rawLines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var entry = NoteEntry.Parse(line);
                            entry.DisplayIndex = di++;
                            indexedNotes.Add((entry, i));
                        }

                        var matchingPair = indexedNotes.FirstOrDefault(p =>
                            string.Equals(StripTimestampAndTags(p.entry.Text), StripTimestampAndTags(partialMatches[0].Text), StringComparison.OrdinalIgnoreCase)
                            && p.entry.IsPinned == partialMatches[0].IsPinned);

                        if (matchingPair.entry != null)
                            return DeleteSpecificLine(rawLines, matchingPair.entry, matchingPair.rawIndex);
                        else
                            return ErrorResult("Error deleting note",
                                "Could not find the exact note in the file. File may have been modified. Type 'qq' to refresh.");
                    }
                    else if (partialMatches.Count > 1)
                    {
                        var results = new List<Result>
                        {
                            new Result
                            {
                                Title = "Multiple partial matching notes found",
                                SubTitle = "Please select a specific note to delete:",
                                IcoPath = IconPath
                            }
                        };

                        foreach (var match in partialMatches)
                        {
                            results.Add(CreateNoteResult(match, "Press Enter to delete", null, (ctx) =>
                            {
                                DeleteNote(match.DisplayIndex.ToString());
                                return true;
                            }));
                        }
                        return results;
                    }
                    else
                    {
                        return SingleInfoResult("Note not found", $"No note with content '{indexOrText}' was found. Try using the note number instead.");
                    }
                }
            }
        }

        // Новий метод: безпосереднє видалення за rawIndex
        private List<Result> DeleteSpecificLine(List<string> rawLines, NoteEntry noteToRemove, int rawIndex)
        {
            try
            {
                var idPrefix = $"[id:{noteToRemove.Id}]";
                var lineToRemove = rawLines.FirstOrDefault(line => line.StartsWith(idPrefix));
                if (lineToRemove == null)
                {
                    return ErrorResult("Note not found", "The note was not found in the file. It may have been deleted already.");
                }

                // Зберігаємо для Undo (без rawIndex, вставка буде в кінець)
                _lastDeletedNoteRaw = (lineToRemove, -1, noteToRemove.IsPinned);

                // Видаляємо рядок за вмістом
                rawLines.Remove(lineToRemove);
                WriteNotes(rawLines);

                return SingleInfoResult("Note deleted",
                    $"Removed: [{noteToRemove.DisplayIndex}] {Truncate(noteToRemove.Text, 50)}\nTip: Use 'qq undo' to restore.", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error deleting note", ex.Message);
            }
        }

        // --- Реалізація qq delall: видалити всі нотатки з підтвердженням ---
        private List<Result> DeleteAllNotes()
        {
            try
            {
                var notes = ReadNotes();
                if (!notes.Any())
                    return SingleInfoResult("No notes to delete", "Your notes file is already empty.");

                // Створюємо варіант підтвердження
                var result = new List<Result>
                {
                    new Result
                    {
                        Title = "Confirm deletion of ALL notes",
                        SubTitle = $"This will permanently delete all {notes.Count} notes. Are you sure?",
                        IcoPath = IconPath,
                        Score = 1000,
                        Action = _ =>
                        {
                            // Backup перед видаленням
                            BackupNotesInternal();

                            // Видаляємо всі рядки
                            WriteNotes(new List<string>());
                            _lastDeletedNoteRaw = null; // reset undo buffer—невідновлювано
                            Context?.API.ShowMsg("All notes deleted",
                                "All notes have been permanently deleted. You can find a backup in the same folder.", IconPath);
                            return true;
                        }
                    },
                    new Result
                    {
                        Title = "Cancel",
                        SubTitle = "Keep your notes (no changes made)",
                        IcoPath = IconPath,
                        Action = _ => true
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                return ErrorResult("Error deleting all notes", ex.Message);
            }
        }

        // Виклик безпосереднього бекапу (для DeleteAllNotes)
        private void BackupNotesInternal()
        {
            try
            {
                if (!File.Exists(_notesPath))
                    return;

                var notesDir = Path.GetDirectoryName(_notesPath)!;
                var backupFile = Path.Combine(notesDir, $"notes_backup_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.Copy(_notesPath, backupFile, true);
            }
            catch
            {
                // Ігноруємо помилки при створенні бекапу
            }
        }

        private List<Result> UndoDelete()
        {
            if (!_lastDeletedNoteRaw.HasValue)
                return SingleInfoResult("Nothing to undo", "No recently deleted note. Delete one first.");

            try
            {
                var (deletedLine, _, wasPinned) = _lastDeletedNoteRaw.Value;
                var rawLines = ReadNotesRaw();
                rawLines.Add(deletedLine); // Додаємо в кінець
                WriteNotes(rawLines);

                var updatedNotes = ReadNotes();
                var restored = updatedNotes.FirstOrDefault(n => n.Text.Trim() == deletedLine.Trim() && n.IsPinned == wasPinned);

                var displayInfo = restored != null ? $" (now note #{restored.DisplayIndex})" : "";
                _lastDeletedNoteRaw = null;

                return SingleInfoResult("Note restored",
                    $"Restored note{displayInfo}.\n{Truncate(deletedLine, 50)}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error restoring note", ex.Message);
            }
        }

        // --- Pin/Unpin також через rawIndex/ID ---
        private List<Result> PinNote(string indexStr, bool pin)
        {
            if (!TryParseNoteIndex(indexStr, out int displayIndex, out var err))
                return err;

            var notes = ReadNotes();
            var noteToUpdate = notes.FirstOrDefault(n => n.DisplayIndex == displayIndex);
            if (noteToUpdate == null)
            {
                var available = notes.Select(n => n.DisplayIndex).OrderBy(x => x).ToList();
                return SingleInfoResult("Note not found", $"Note #{displayIndex} does not exist. Available: {string.Join(", ", available)}");
            }

            try
            {
                var rawLines = ReadNotesRaw();
                var idPrefix = $"[id:{noteToUpdate.Id}]";
                var index = rawLines.FindIndex(line => line.StartsWith(idPrefix));
                if (index < 0)
                    return ErrorResult("Note not found", "Could not find the note in the file.");

                var entryToChange = NoteEntry.Parse(rawLines[index]);
                entryToChange.IsPinned = pin;
                rawLines[index] = entryToChange.ToFileLine();
                WriteNotes(rawLines);

                _lastDeletedNoteRaw = null;
                return SingleInfoResult($"Note {(pin ? "pinned" : "unpinned")}", $"[{displayIndex}] {entryToChange.Text}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult($"Error {(pin ? "pinning" : "unpinning")} note", ex.Message);
            }
        }

        private List<Result> SortNotes(string args)
        {
            var notes = ReadNotes();
            if (!notes.Any())
                return SingleInfoResult("No notes to sort", "");

            var sortType = args.ToLowerInvariant().Trim();
            var descending = sortType.EndsWith(" desc");
            if (descending) sortType = sortType.Substring(0, sortType.Length - 5).Trim();
            var ascending = sortType.EndsWith(" asc");
            if (ascending) sortType = sortType.Substring(0, sortType.Length - 4).Trim();

            IEnumerable<NoteEntry> sorted;
            switch (sortType)
            {
                case "date":
                    sorted = descending
                        ? notes.OrderByDescending(n => n.Timestamp == DateTime.MinValue ? DateTime.MaxValue : n.Timestamp)
                               .ThenBy(n => n.DisplayIndex)
                        : notes.OrderBy(n => n.Timestamp == DateTime.MinValue ? DateTime.MaxValue : n.Timestamp)
                               .ThenBy(n => n.DisplayIndex);
                    break;
                case "alpha":
                case "text":
                    sorted = descending
                        ? notes.OrderByDescending(n => n.Text, StringComparer.OrdinalIgnoreCase)
                               .ThenBy(n => n.DisplayIndex)
                        : notes.OrderBy(n => n.Text, StringComparer.OrdinalIgnoreCase)
                               .ThenBy(n => n.DisplayIndex);
                    break;
                default:
                    return SingleInfoResult("Invalid sort type", "Use 'qq sort date [asc|desc]' or 'qq sort alpha [asc|desc]'");
            }

            try
            {
                var linesToWrite = sorted.Select(n => n.ToFileLine()).ToList();
                WriteNotes(linesToWrite);
                _lastDeletedNoteRaw = null;
                return SingleInfoResult("Notes sorted", $"Sorted by {sortType} {(descending ? "descending" : (ascending ? "ascending" : "(default asc)"))}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error sorting notes", ex.Message);
            }
        }

        private List<Result> EditNote(string noteNumberStr)
        {
            if (!TryParseNoteIndex(noteNumberStr, out int displayIndex, out var err))
                return err;

            var notes = ReadNotes();
            var noteToEdit = notes.FirstOrDefault(n => n.DisplayIndex == displayIndex);
            if (noteToEdit == null)
            {
                var available = notes.Select(n => n.DisplayIndex).OrderBy(x => x).ToList();
                return SingleInfoResult("Note not found", $"Note #{displayIndex} does not exist. Available: {string.Join(", ", available)}");
            }

            var oldText = noteToEdit.Text;
            var newText = Interaction.InputBox($"Edit note #{displayIndex}", "Edit QuickNote", oldText);
            if (string.IsNullOrEmpty(newText) || newText == oldText)
                return SingleInfoResult("Edit cancelled", "Note was not changed.");

            try
            {
                // Зберігаємо timestamp prefix, якщо є
                var tsPrefix = "";
                if (noteToEdit.Timestamp != DateTime.MinValue && noteToEdit.Text.StartsWith("["))
                    tsPrefix = noteToEdit.Text.Substring(0, 22);

                noteToEdit.Text = tsPrefix + newText.Trim();

                // Зчитуємо rawLines і знаходимо, де лежить ця нотатка
                var rawLines = ReadNotesRaw();
                var idPrefix = $"[id:{noteToEdit.Id}]";
                var index = rawLines.FindIndex(line => line.StartsWith(idPrefix));
                if (index < 0)
                    return ErrorResult("Note not found", "Could not find the note in the file.");

                rawLines[index] = noteToEdit.ToFileLine();
                WriteNotes(rawLines);
                _lastDeletedNoteRaw = null;
                return SingleInfoResult("Note edited", $"Updated note #{displayIndex}: {Truncate(newText, 50)}", true);
            }
            catch (Exception ex)
            {
                return ErrorResult("Error saving edited note", ex.Message);
            }
        }

        private void EditNoteInline(NoteEntry note)
        {
            var oldText = note.Text;
            var newText = Interaction.InputBox($"Edit note #{note.DisplayIndex}", "Edit QuickNote", oldText);
            if (string.IsNullOrEmpty(newText) || newText == oldText)
            {
                Context?.API.ShowMsg("Edit cancelled", "Note was not changed.", IconPath);
                return;
            }

            try
            {
                var tsPrefix = "";
                if (note.Timestamp != DateTime.MinValue && note.Text.StartsWith("["))
                    tsPrefix = note.Text.Substring(0, 22);

                note.Text = tsPrefix + newText.Trim();

                var rawLines = ReadNotesRaw();
                var idPrefix = $"[id:{note.Id}]";
                var index = rawLines.FindIndex(line => line.StartsWith(idPrefix));
                if (index < 0)
                {
                    Context?.API.ShowMsg("Error", "Could not find note to save edit. Refresh and retry.", IconPath);
                    return;
                }

                rawLines[index] = note.ToFileLine();
                WriteNotes(rawLines);
                _lastDeletedNoteRaw = null;
                Context?.API.ShowMsg("Note edited", $"Updated note #{note.DisplayIndex}", IconPath);
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error saving note", ex.Message, IconPath);
            }
        }

        private List<Result> ViewNote(string noteNumberStr)
        {
            if (!TryParseNoteIndex(noteNumberStr, out int displayIndex, out var err))
                return err;

            var notes = ReadNotes();
            var note = notes.FirstOrDefault(n => n.DisplayIndex == displayIndex);
            if (note == null)
            {
                var available = notes.Select(n => n.DisplayIndex).OrderBy(x => x).ToList();
                return SingleInfoResult("Note not found", $"Note #{displayIndex} does not exist. Available: {string.Join(", ", available)}");
            }

            return new List<Result> { CreateNoteResult(note, "Press Enter to copy without timestamp | Ctrl+C for full note | Right-click for options") };
        }

        private List<Result> BackupNotes()
        {
            try
            {
                if (!File.Exists(_notesPath))
                    return SingleInfoResult("No notes file to backup", "The notes file doesn't exist.");

                var notesDir = Path.GetDirectoryName(_notesPath)!;
                var backupFile = Path.Combine(notesDir, $"notes_backup_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.Copy(_notesPath, backupFile, true);

                try
                {
                    Process.Start(new ProcessStartInfo(notesDir) { UseShellExecute = true, Verb = "open" });
                }
                catch (Exception ex)
                {
                    Log.Exception("Failed to open backup folder", ex, GetType());
                }

                return SingleInfoResult("Backup created",
                    $"Backup saved to: {backupFile}\nBackup folder opened.", true);
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

        private Result HelpResult()
        {
            var helpText =
                "qq <text>              Add note ✏️       | qq pin <N>             Pin note 📌\n" +
                "qq search <term>       Search notes 🔍   | qq unpin <N>           Unpin note 📎\n" +
                "qq searchtag <tag>     Search #tags 🏷️   | qq sort date|alpha     Sort notes 🔄\n" +
                "qq view <N>            View note 👁️      | qq undo                Restore note ↩️\n" +
                "qq edit <N>            Edit note 📝      | qq delall              Delete ALL 💣\n" +
                "qq del <N>             Delete note 🗑️    | qq backup/export       Backup notes 💾\n" +
                "qq help                Show help ℹ️      | qq tagstyle bold/italic Tag style ✨\n\n" +
                "Formatting: **bold** or __bold__, *italic* or _italic_, ==highlight==, #tag\n" +
                "TIP: Right-click on a note for copy/edit options";

            return new Result
            {
                Title = "QuickNotes Help",
                SubTitle = helpText,
                IcoPath = IconPath,
                Action = _ => false
            };
        }

        // --- Зчитування нотаток ---
        private List<NoteEntry> ReadNotes()
        {
            try
            {
                if (!File.Exists(_notesPath))
                    return new List<NoteEntry>();

                var allLines = ReadNotesRaw();
                var entries = new List<NoteEntry>();
                int di = 1;

                foreach (var line in allLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var entry = NoteEntry.Parse(line);
                    if (!string.IsNullOrWhiteSpace(entry.Text))
                    {
                        entry.DisplayIndex = di++;
                        entries.Add(entry);
                    }
                }
                return entries;
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error reading notes", ex.Message);
                return new List<NoteEntry>();
            }
        }

        // Зчитує сирі рядки з файлу
        private List<string> ReadNotesRaw()
        {
            try
            {
                if (!File.Exists(_notesPath))
                    return new List<string>();

                using (var fs = new FileStream(_notesPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    var lines = new List<string>();
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                        lines.Add(line);
                    return lines;
                }
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error reading notes", ex.Message);
                return new List<string>();
            }
        }

        // Записує оновлений список рядків у файл
        private void WriteNotes(List<string> lines)
        {
            try
            {
                var backupPath = _notesPath + ".bak";
                if (File.Exists(_notesPath))
                {
                    try { File.Copy(_notesPath, backupPath, true); }
                    catch { /* Ігноруємо помилки при створенні бекапу */ }
                }

                using (var fs = new FileStream(_notesPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fs))
                {
                    foreach (var l in lines)
                        writer.WriteLine(l);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error writing notes", ex.Message);
            }
        }

        // --- Утиліти ---
        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        private bool TryParseNoteIndex(string indexStr, out int displayIndex, out List<Result> errorResult)
        {
            if (!int.TryParse(indexStr, out displayIndex) || displayIndex <= 0)
            {
                errorResult = SingleInfoResult("Invalid note number", "Specify a valid positive number. E.g. 'qq del 3'");
                return false;
            }

            var notes = ReadNotes();
            if (!notes.Any())
            {
                errorResult = SingleInfoResult("No notes found", "No notes to operate on. Add a note first.");
                return false;
            }

            var maxIdx = notes.Max(n => n.DisplayIndex);
            if (displayIndex > maxIdx)
            {
                errorResult = SingleInfoResult("Invalid note number",
                    $"Note #{displayIndex} does not exist. Highest is {maxIdx}.\nAvailable: {string.Join(", ", notes.Select(n => n.DisplayIndex).OrderBy(x => x))}");
                return false;
            }

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
                    Action = _ =>
                    {
                        Context?.API.ShowMsg(title, subTitle);
                        return false;
                    }
                }
            };
        }

        // --- Контекстне меню ---
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var items = new List<ContextMenuResult>();
            if (selectedResult.ContextData is NoteEntry note)
            {
                // Copy full note
                items.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy Full Note (with timestamp)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\uE8C8",
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
                            Context?.API.ShowMsg("Error", $"Failed to copy: {ex.Message}");
                            return false;
                        }
                    }
                });

                // Copy clean content
                var contentOnly = StripTimestampAndTags(note.Text);
                items.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy Clean Content (no timestamp, no tags)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\uE8C9",
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
                            Context?.API.ShowMsg("Error", $"Failed to copy: {ex.Message}");
                            return false;
                        }
                    }
                });

                // Edit
                items.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Edit Note...",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\uE70F",
                    AcceleratorKey = Key.E,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        EditNoteInline(note);
                        return true;
                    }
                });

                // Delete
                items.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Delete Note",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\uE74D",
                    AcceleratorKey = Key.Delete,
                    Action = _ =>
                    {
                        DeleteNote(note.DisplayIndex.ToString());
                        return true;
                    }
                });

                // Pin/Unpin
                items.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = note.IsPinned ? "Unpin Note" : "Pin Note",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = note.IsPinned ? "\uE77A" : "\uE718",
                    Action = _ =>
                    {
                        PinNote(note.DisplayIndex.ToString(), !note.IsPinned);
                        return true;
                    }
                });

                // Open URL (якщо є)
                var urlMatch = UrlRegex.Match(note.Text);
                if (urlMatch.Success)
                {
                    var url = urlMatch.Value;
                    if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase) && !url.Contains("://"))
                        url = "http://"+url;

                    items.Add(new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = $"Open URL: {url.Substring(0, Math.Min(url.Length, 40))}{(url.Length > 40 ? "..." : "")}",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\uE774",
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
            return items;
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
            if (Disposed || !disposing) return;
            if (Context?.API != null)
                Context.API.ThemeChanged -= OnThemeChanged;
            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) =>
            IconPath = (theme == Theme.Light || theme == Theme.HighContrastWhite)
                ? "Images/quicknotes.light.png"
                : "Images/quicknotes.dark.png";

        private void OnThemeChanged(Theme current, Theme next) =>
            UpdateIconPath(next);
    }
}
