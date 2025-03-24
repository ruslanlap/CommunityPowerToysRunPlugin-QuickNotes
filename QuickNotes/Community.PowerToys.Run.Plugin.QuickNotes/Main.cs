using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics; // For Process.Start
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // For highlighting
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;
using Wox.Plugin.Logger;
using Microsoft.VisualBasic; // For InputBox

namespace Community.PowerToys.Run.Plugin.QuickNotes
{
    public class Main : IPlugin, IContextMenu, IPluginI18n, IDisposable
    {
        // Must match the ID in plugin.json
        public static string PluginID => "2083308C581F4D36B0C02E69A2FD91D7";

        public string Name => "QuickNotes";
        public string Description => "Save, view, manage, and search quick notes";

        private PluginInitContext? Context { get; set; }
        private string? IconPath { get; set; }
        private bool Disposed { get; set; }

        private string _notesPath = string.Empty;
        private bool _isInitialized = false;

        public void Init(PluginInitContext context)
        {
            try
            {
                Context = context ?? throw new ArgumentNullException(nameof(context));

                // Handle theme changes
                UpdateIconPath(Context.API.GetCurrentTheme());
                Context.API.ThemeChanged += OnThemeChanged;

                // Create the QuickNotes folder in LocalAppData
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var powerToysPath = Path.Combine(appDataPath, "Microsoft", "PowerToys", "QuickNotes");
                if (!Directory.Exists(powerToysPath))
                {
                    Directory.CreateDirectory(powerToysPath);
                }

                _notesPath = Path.Combine(powerToysPath, "notes.txt");
                if (!File.Exists(_notesPath))
                {
                    File.WriteAllText(_notesPath, string.Empty);
                }

                // Test file access
                try
                {
                    File.AppendAllText(_notesPath, string.Empty);
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

        public List<Result> Query(Query query)
        {
            if (!_isInitialized)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "QuickNotes not initialized",
                        SubTitle = "Plugin not initialized properly. Please restart PowerToys.",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("QuickNotes Error", "Plugin not initialized properly. Please restart PowerToys.");
                            return true;
                        }
                    }
                };
            }

            // Get the text after "qq"
            var searchText = query.Search?.Trim() ?? string.Empty;
            string lower = searchText.ToLower();

            // 1. Help command
            if (lower.Equals("help"))
            {
                return HelpCommand();
            }
            // 2. Backup/Export command
            else if (lower.Equals("backup") || lower.Equals("export"))
            {
                return BackupNotes();
            }
            // 3. Edit command (edit note)
            else if (lower.StartsWith("edit "))
            {
                var noteNumberStr = searchText.Substring("edit ".Length).Trim();
                return EditNote(noteNumberStr);
            }
            // 4. View command (view note details)
            else if (lower.StartsWith("view "))
            {
                var noteNumberStr = searchText.Substring("view ".Length).Trim();
                return ViewNote(noteNumberStr);
            }
            // 5. Delete All command
            else if (lower.Equals("delall"))
            {
                return DeleteAllNotes();
            }
            // 6. Delete individual note command
            else if (lower.StartsWith("del "))
            {
                var noteNumberStr = searchText.Substring("del ".Length).Trim();
                return DeleteNote(noteNumberStr);
            }
            // 7. Search command
            else if (lower.StartsWith("search "))
            {
                var term = searchText.Substring("search ".Length).Trim();
                if (string.IsNullOrEmpty(term))
                {
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "Search QuickNotes",
                            SubTitle = "Type qq search <word> to find notes containing that word",
                            IcoPath = IconPath,
                            Action = _ => false
                        }
                    };
                }
                return SearchNotes(term);
            }
            // 8. Default – add new note
            else
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = $"Add note: {searchText}",
                        SubTitle = "Press Enter to create your note, or type 'qq help' for command assistance ",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            CreateNote(searchText);
                            Context?.API.ShowMsg("Note saved", $"Your note has been saved: {searchText}");
                            return true;
                        }
                    }
                };
            }
        }

        // ---------------------------
        // Search command logic
        // ---------------------------
        private List<Result> SearchNotes(string searchTerm)
        {
            var results = new List<Result>();

            if (!File.Exists(_notesPath))
            {
                results.Add(new Result
                {
                    Title = "No notes found",
                    SubTitle = "Your notes file does not exist or is empty.",
                    IcoPath = IconPath,
                    Action = _ => false
                });
                return results;
            }

            // Read all notes
            var notes = File.ReadAllLines(_notesPath)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();

            // Filter notes that contain the search term (case-insensitive)
            var matchedNotes = notes
                .Select((noteText, idx) => new { Text = noteText, Index = idx })
                .Where(x => x.Text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (matchedNotes.Count == 0)
            {
                results.Add(new Result
                {
                    Title = "No matches found",
                    SubTitle = $"No notes contain the word \"{searchTerm}\".",
                    IcoPath = IconPath,
                    Action = _ => false
                });
                return results;
            }

            // Build results for each matched note with highlighting
            foreach (var match in matchedNotes)
            {
                string highlighted = HighlightMatch(match.Text, searchTerm);
                var result = new Result
                {
                    Title = $"[{match.Index + 1}] {highlighted}",
                    SubTitle = "Press Enter to copy to clipboard | 'qq del " + (match.Index + 1) + "' to delete",
                    IcoPath = IconPath,
                    ToolTipData = new ToolTipData("Note", match.Text),
                    Action = _ =>
                    {
                        try
                        {
                            Clipboard.SetText(match.Text);
                            Context?.API.ShowMsg("Note copied", "Copied to clipboard");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Context?.API.ShowMsg("Error", "Failed to copy to clipboard: " + ex.Message);
                            return false;
                        }
                    },
                    ContextData = match.Text
                };
                results.Add(result);
            }
            return results;
        }

        /// <summary>
        /// Highlights the matched text by adding square brackets.
        /// For example, if searchTerm = "milk", "Buy milk today" becomes "Buy [milk] today".
        /// </summary>
        private string HighlightMatch(string noteText, string searchTerm)
        {
            var pattern = Regex.Escape(searchTerm);
            var highlighted = Regex.Replace(noteText, pattern,
                m => $"[{m.Value}]",
                RegexOptions.IgnoreCase);
            return highlighted;
        }

        // ---------------------------
        // Commands for adding, editing, viewing, and deleting notes
        // ---------------------------
        private void CreateNote(string note)
        {
            if (string.IsNullOrEmpty(note))
            {
                return;
            }
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string entry = $"[{timestamp}] {note}";
                File.AppendAllText(_notesPath, entry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Context?.API.ShowMsg("Error saving note", ex.Message);
            }
        }

        private List<Result> GetInstructionsAndNotes()
        {
            var results = new List<Result>
            {
                new Result
                {
                    Title = "QuickNotes Commands",
                    SubTitle = "Commands:\n" +
                               "qq <text>             - Add note\n" +
                               "qq del <N>            - Delete note\n" +
                               "qq delall             - Delete all notes\n" +
                               "qq search <word>      - Search notes (matched words highlighted)\n" +
                               "qq edit <N>           - Edit note\n" +
                               "qq view <N>           - View note details\n" +
                               "qq backup/export      - Backup notes\n" +
                               "qq help               - Show help",
                    IcoPath = IconPath,
                    Action = _ => false
                }
            };

            try
            {
                if (File.Exists(_notesPath))
                {
                    var notes = File.ReadAllLines(_notesPath)
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .ToList();
                    for (int i = 0; i < notes.Count; i++)
                    {
                        int index = i;
                        results.Add(new Result
                        {
                            Title = $"[{i + 1}] {notes[i]}",
                            SubTitle = $"Press Enter to copy | 'qq del {i + 1}' to delete",
                            IcoPath = IconPath,
                            ToolTipData = new ToolTipData("Note", notes[i]),
                            Action = _ =>
                            {
                                try
                                {
                                    Clipboard.SetText(notes[index]);
                                    Context?.API.ShowMsg("Note copied", "Copied to clipboard");
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    Context?.API.ShowMsg("Error", "Failed to copy: " + ex.Message);
                                    return false;
                                }
                            },
                            ContextData = notes[i]
                        });
                    }

                    if (notes.Count == 0)
                    {
                        results.Add(new Result
                        {
                            Title = "No notes found",
                            SubTitle = "Type qq <note> to create your first note",
                            IcoPath = IconPath,
                            Action = _ => false
                        });
                    }
                }
                else
                {
                    File.WriteAllText(_notesPath, string.Empty);
                    results.Add(new Result
                    {
                        Title = "No notes found",
                        SubTitle = "Type qq <note> to create your first note",
                        IcoPath = IconPath,
                        Action = _ => false
                    });
                }
            }
            catch (Exception ex)
            {
                results.Add(new Result
                {
                    Title = "Error reading notes",
                    SubTitle = ex.Message,
                    IcoPath = IconPath,
                    Action = _ => false
                });
            }
            return results;
        }

        private List<Result> DeleteNote(string indexStr)
        {
            if (!File.Exists(_notesPath))
            {
                return GetInstructionsAndNotes();
            }
            if (!int.TryParse(indexStr, out int index) || index <= 0)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Invalid note number",
                        SubTitle = "Please specify a valid positive integer",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", "Invalid note number. Use a positive integer.");
                            return true;
                        }
                    }
                };
            }
            try
            {
                var notes = File.ReadAllLines(_notesPath)
                                .Where(line => !string.IsNullOrWhiteSpace(line))
                                .ToList();
                if (index > notes.Count)
                {
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "Note not found",
                            SubTitle = $"The last note is #{notes.Count}",
                            IcoPath = IconPath,
                            Action = _ =>
                            {
                                Context?.API.ShowMsg("Error", $"Note #{index} not found. The last note is #{notes.Count}.");
                                return true;
                            }
                        }
                    };
                }
                string deletedNote = notes[index - 1];
                notes.RemoveAt(index - 1);
                File.WriteAllLines(_notesPath, notes);
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Note deleted",
                        SubTitle = deletedNote,
                        IcoPath = IconPath,
                        ToolTipData = new ToolTipData("Note deleted", deletedNote),
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Note deleted", "The note has been removed.");
                            return true;
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Error deleting note",
                        SubTitle = ex.Message,
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", "Failed to delete note: " + ex.Message);
                            return false;
                        }
                    }
                };
            }
        }

        private List<Result> DeleteAllNotes()
        {
            if (!File.Exists(_notesPath))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "No notes file found",
                        SubTitle = "It may already be empty or missing.",
                        IcoPath = IconPath,
                        Action = _ => false
                    }
                };
            }
            try
            {
                File.WriteAllText(_notesPath, string.Empty);
                return new List<Result>
                {
                    new Result
                    {
                        Title = "All notes deleted",
                        SubTitle = "Your QuickNotes file is now empty.",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("All notes deleted", "Your QuickNotes file is now empty.");
                            return true;
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Error deleting all notes",
                        SubTitle = ex.Message,
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", "Failed to delete all notes: " + ex.Message);
                            return false;
                        }
                    }
                };
            }
        }

        // ---------------------------
        // Edit note command: qq edit [number]
        // ---------------------------
        private List<Result> EditNote(string noteNumberStr)
        {
            if (!int.TryParse(noteNumberStr, out int noteIndex) || noteIndex <= 0)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Invalid note number",
                        SubTitle = "Please provide a valid positive integer",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", "Invalid note number.");
                            return true;
                        }
                    }
                };
            }
            var notes = File.ReadAllLines(_notesPath)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();
            if (noteIndex > notes.Count)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Note not found",
                        SubTitle = $"Last note is #{notes.Count}",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", $"Note #{noteIndex} not found. The last note is #{notes.Count}.");
                            return true;
                        }
                    }
                };
            }
            string oldNote = notes[noteIndex - 1];
            // Use InputBox for editing
            string newNote = Interaction.InputBox("Edit note", "Edit Note", oldNote);
            if (string.IsNullOrEmpty(newNote) || newNote == oldNote)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Note unchanged",
                        SubTitle = "No modifications made.",
                        IcoPath = IconPath,
                        Action = _ => false
                    }
                };
            }
            notes[noteIndex - 1] = newNote;
            File.WriteAllLines(_notesPath, notes);
            return new List<Result>
            {
                new Result
                {
                    Title = "Note edited",
                    SubTitle = newNote,
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        Context?.API.ShowMsg("Note edited", "The note has been updated.");
                        return true;
                    }
                }
            };
        }

        // ---------------------------
        // View note command: qq view [number]
        // ---------------------------
        private List<Result> ViewNote(string noteNumberStr)
        {
            if (!int.TryParse(noteNumberStr, out int noteIndex) || noteIndex <= 0)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Invalid note number",
                        SubTitle = "Please provide a valid positive integer",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", "Invalid note number.");
                            return true;
                        }
                    }
                };
            }
            var notes = File.ReadAllLines(_notesPath)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToList();
            if (noteIndex > notes.Count)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Note not found",
                        SubTitle = $"Last note is #{notes.Count}",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", $"Note #{noteIndex} not found. The last note is #{notes.Count}.");
                            return true;
                        }
                    }
                };
            }
            string note = notes[noteIndex - 1];
            return new List<Result>
            {
                new Result
                {
                    Title = $"Note #{noteIndex}",
                    SubTitle = note,
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        try
                        {
                            Clipboard.SetText(note);
                            Context?.API.ShowMsg("Note copied", "The note has been copied to clipboard.");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Context?.API.ShowMsg("Error", "Failed to copy note: " + ex.Message);
                            return false;
                        }
                    }
                }
            };
        }

        // ---------------------------
        // Backup/Export command: qq backup or qq export
        // ---------------------------
        private List<Result> BackupNotes()
        {
            try
            {
                if (!File.Exists(_notesPath))
                {
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "No notes file",
                            SubTitle = "Notes file does not exist.",
                            IcoPath = IconPath,
                            Action = _ => false
                        }
                    };
                }
                string backupFileName = Path.Combine(Path.GetDirectoryName(_notesPath)!, "notes_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                File.Copy(_notesPath, backupFileName, true);

                // Open Windows Explorer with the backup file selected
                Process.Start("explorer.exe", $"/select,\"{backupFileName}\"");

                // Open the backup file in the default application
                Process.Start(new ProcessStartInfo
                {
                    FileName = backupFileName,
                    UseShellExecute = true
                });

                return new List<Result>
                {
                    new Result
                    {
                        Title = "Backup created",
                        SubTitle = $"Backup file: {Path.GetFileName(backupFileName)}",
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Backup", $"Backup created: {backupFileName}");
                            return true;
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Error creating backup",
                        SubTitle = ex.Message,
                        IcoPath = IconPath,
                        Action = _ =>
                        {
                            Context?.API.ShowMsg("Error", "Failed to create backup: " + ex.Message);
                            return false;
                        }
                    }
                };
            }
        }

        // ---------------------------
        // Help command: qq help
        // ---------------------------
        private List<Result> HelpCommand()
        {
            string helpText = "Available commands:\n" +
                              "qq <text>             - ✅ Add note \n" +
                              "qq del <number>       - ❌ Delete note \n" +
                              "qq delall             - ⛔ Delete all notes \n" +
                              "qq search <word>      - 🔎 Search notes  (matched words highlighted)\n" +
                              "qq edit <number>      - ✏️ Edit note \n" +
                              "qq view <number>      - 👀 View note details \n" +
                              "qq backup/export      - 💾 Backup notes  (opens folder and file)\n" +
                              "qq help               - Show help ";
            return new List<Result>
            {
                new Result
                {
                    Title = "QuickNotes Help",
                    SubTitle = helpText,
                    IcoPath = IconPath,
                    Action = _ => false
                }
            };
        }

        // ---------------------------
        // Context menu, localization, and Dispose
        // ---------------------------
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string noteText)
            {
                return new List<ContextMenuResult>
                {
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy to clipboard",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\uE8C8", // Copy icon
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            try
                            {
                                Clipboard.SetText(noteText);
                                Context?.API.ShowMsg("Note copied", "Copied to clipboard");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Context?.API.ShowMsg("Error", "Failed to copy to clipboard: " + ex.Message);
                                return false;
                            }
                        }
                    }
                };
            }
            return new List<ContextMenuResult>();
        }

        public string GetTranslatedPluginTitle() => "QuickNotes";
        public string GetTranslatedPluginDescription() => "Save, view, manage, and search quick notes";

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

        private void UpdateIconPath(Theme theme) =>
            IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
                ? "Images/quicknotes.light.png"
                : "Images/quicknotes.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) =>
            UpdateIconPath(newTheme);
    }
}
