namespace Community.PowerToys.Run.Plugin.QuickNotes
{
    public class QuickNotesSettings
    {
        public bool EnableGitSync { get; set; } = false;
        public string NotesFolderPath { get; set; } = string.Empty;
        public string GitRepositoryUrl { get; set; } = string.Empty;
        public string GitBranch { get; set; } = "main";
        public string GitUsername { get; set; } = string.Empty;
        public string GitEmail { get; set; } = string.Empty;
    }
}
