using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Community.PowerToys.Run.Plugin.QuickNotes
{
    public partial class MultiLineInputDialog : Window, INotifyPropertyChanged
    {
        private string _inputText = string.Empty;
        private string _previewText = string.Empty;

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        public string PreviewText
        {
            get => _previewText;
            set
            {
                _previewText = value;
                OnPropertyChanged();
            }
        }

        public string ResultText { get; private set; } = string.Empty;
        public bool DialogResult { get; private set; } = false;

        public MultiLineInputDialog(string initialText = "")
        {
            InitializeComponent();
            DataContext = this;
            
            InputText = initialText;
            
            // Set up keyboard shortcuts
            KeyDown += OnKeyDown;
            InputTextBox.TextChanged += (s, e) => InputText = InputTextBox.Text;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveNote();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelDialog();
                e.Handled = true;
            }
        }

        private void UpdatePreview()
        {
            PreviewText = FormatMarkdownForPreview(_inputText);
        }

        private string FormatMarkdownForPreview(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return "Preview will appear here...";

            var text = markdown;

            // Headers
            text = Regex.Replace(text, @"^### (.+)$", "▶ $1", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^## (.+)$", "▶▶ $1", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^# (.+)$", "▶▶▶ $1", RegexOptions.Multiline);

            // Bold formatting: **text** or __text__
            text = Regex.Replace(text, @"\*\*(.*?)\*\*|__(.*?)__", m =>
                $"【{(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)}】");

            // Italics formatting: *text* or _text_
            text = Regex.Replace(text, @"(?<!\*)\*(?!\*)(.*?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(.*?)(?<!_)_(?!_)", m =>
                $"〈{(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)}〉");

            // Highlight: ==text==
            text = Regex.Replace(text, @"==(.*?)==", m => $"《{m.Groups[1].Value}》");

            // Inline code: `code`
            text = Regex.Replace(text, @"`([^`]+)`", "⟨$1⟩");

            // Code blocks: ```code```
            text = Regex.Replace(text, @"```(.*?)```", m => $"┌─ CODE ─┐\n{m.Groups[1].Value}\n└─────────┘", RegexOptions.Singleline);

            // Lists
            text = Regex.Replace(text, @"^- (.+)$", "• $1", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^\d+\. (.+)$", "1. $1", RegexOptions.Multiline);

            // Tags
            text = Regex.Replace(text, @"(#\w+)", "【$1】");

            return text;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveNote();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        private void SaveNote()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                MessageBox.Show("Please enter some text for your note.", "Empty Note", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ResultText = InputText.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelDialog()
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
