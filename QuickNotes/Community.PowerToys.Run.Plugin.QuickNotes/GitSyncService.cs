using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Community.PowerToys.Run.Plugin.QuickNotes
{
    public class GitSyncService
    {
        private readonly string _notesDirectory;
        private readonly string _notesFileName = "notes.txt";
        private readonly QuickNotesSettings _settings;

        public event Action<string>? ProgressChanged;

        public GitSyncService(string notesDirectory, QuickNotesSettings settings)
        {
            _notesDirectory = notesDirectory;
            _settings = settings;
        }

        private string NotesFilePath => Path.Combine(_notesDirectory, _notesFileName);

        private string EffectiveBranch => string.IsNullOrWhiteSpace(_settings.GitBranch) ? "main" : _settings.GitBranch;

        public (bool Success, string Message) SyncToGit()
        {
            ReportProgress("🔍 Checking repository...");
            var ensure = EnsureRepository();
            if (!ensure.Success)
            {
                return ensure;
            }

            var addResult = RunGit(_notesDirectory, "add", _notesFileName);
            ReportCommandOutput(addResult);
            if (!addResult.Success)
            {
                return (false, addResult.Message);
            }

            var statusResult = RunGit(_notesDirectory, "status", "--porcelain");
            ReportCommandOutput(statusResult);
            if (!statusResult.Success)
            {
                return (false, statusResult.Message);
            }

            if (string.IsNullOrWhiteSpace(statusResult.StdOut))
            {
                ReportProgress("✓ No changes to sync");
                return (true, "✓ Everything is up to date! No changes to sync.");
            }

            var username = string.IsNullOrWhiteSpace(_settings.GitUsername)
                ? GetGitConfigValue("user.name") ?? "QuickNotes User"
                : _settings.GitUsername;

            var email = string.IsNullOrWhiteSpace(_settings.GitEmail)
                ? GetGitConfigValue("user.email") ?? "quicknotes@local"
                : _settings.GitEmail;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var commitArgs = new List<string>();
            if (!string.IsNullOrWhiteSpace(username))
            {
                commitArgs.Add("-c");
                commitArgs.Add($"user.name={username}");
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                commitArgs.Add("-c");
                commitArgs.Add($"user.email={email}");
            }
            commitArgs.Add("commit");
            commitArgs.Add("-m");
            commitArgs.Add(timestamp);

            ReportProgress($"💾 Creating commit: {timestamp}...");
            var commitResult = RunGit(_notesDirectory, commitArgs.ToArray());
            ReportCommandOutput(commitResult);
            if (!commitResult.Success)
            {
                return (false, commitResult.Message);
            }

            ReportProgress("☁️ Pushing to remote repository...");
            var pushResult = RunGit(_notesDirectory, "push", "origin", EffectiveBranch);
            ReportCommandOutput(pushResult);
            if (!pushResult.Success)
            {
                return (false, pushResult.Message);
            }

            ReportProgress("✓ Push completed");
            return (true, $"✅ Notes synced successfully at {DateTime.Now:HH:mm:ss}");
        }

        public (bool Success, string Message) RestoreFromGit()
        {
            ReportProgress("🔍 Checking repository...");
            var ensure = EnsureRepository();
            if (!ensure.Success)
            {
                return ensure;
            }

            try
            {
                if (File.Exists(NotesFilePath))
                {
                    var backupPath = Path.Combine(_notesDirectory, $"notes_backup_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    ReportProgress("💾 Creating local backup...");
                    File.Copy(NotesFilePath, backupPath, true);
                    ReportProgress($"✓ Backup saved: {Path.GetFileName(backupPath)}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"❌ Failed to create backup: {ex.Message}");
            }

            ReportProgress("☁️ Fetching from remote repository...");
            var fetchResult = RunGit(_notesDirectory, "fetch", "origin", EffectiveBranch);
            ReportCommandOutput(fetchResult);
            if (!fetchResult.Success)
            {
                return (false, fetchResult.Message);
            }

            ReportProgress("🔄 Restoring notes from remote...");
            var resetResult = RunGit(_notesDirectory, "reset", "--hard", $"origin/{EffectiveBranch}");
            ReportCommandOutput(resetResult);
            if (!resetResult.Success)
            {
                return (false, resetResult.Message);
            }

            ReportProgress("✓ Restore completed");
            return (true, $"✅ Notes restored successfully at {DateTime.Now:HH:mm:ss}. Local backup created.");
        }

        private (bool Success, string Message) EnsureRepository()
        {
            if (string.IsNullOrWhiteSpace(_settings.GitRepositoryUrl))
            {
                return (false, "⚠️ Git Repository URL is not configured in settings");
            }

            Directory.CreateDirectory(_notesDirectory);

            var gitFolder = Path.Combine(_notesDirectory, ".git");
            if (Directory.Exists(gitFolder))
            {
                return (true, string.Empty);
            }

            List<string>? localNotes = null;
            if (File.Exists(NotesFilePath))
            {
                ReportProgress("💾 Detected existing notes. Backing up before linking repository...");
                localNotes = File.ReadAllLines(NotesFilePath).ToList();
            }

            var tempDir = Path.Combine(Path.GetTempPath(), "QuickNotes_" + Guid.NewGuid().ToString("N"));
            var cloneArgs = new List<string> { "clone", "--single-branch", "--branch", EffectiveBranch, _settings.GitRepositoryUrl, tempDir };
            var cloneResult = RunGit(Path.GetTempPath(), cloneArgs.ToArray());
            ReportCommandOutput(cloneResult);
            if (!cloneResult.Success)
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }

                ReportProgress("⚠️ Requested branch not found or clone failed. Trying repository default branch...");
                var fallbackResult = RunGit(Path.GetTempPath(), "clone", _settings.GitRepositoryUrl, tempDir);
                ReportCommandOutput(fallbackResult);
                if (!fallbackResult.Success)
                {
                    return (false, cloneResult.Message);
                }

                var branchResult = RunGit(tempDir, "rev-parse", "--abbrev-ref", "HEAD");
                ReportCommandOutput(branchResult);
                if (branchResult.Success)
                {
                    var branchName = branchResult.StdOut.Trim();
                    if (!string.IsNullOrWhiteSpace(branchName) && !branchName.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
                    {
                        _settings.GitBranch = branchName;
                        ReportProgress($"ℹ️ Using repository branch '{branchName}'. Update settings if you prefer a different branch.");
                    }
                }
            }

            try
            {
                var tempGitDir = Path.Combine(tempDir, ".git");
                MoveDirectory(tempGitDir, gitFolder);
                ReportProgress("📁 Repository initialized in notes directory");

                var remoteNotes = new List<string>();
                var remoteNotesPath = Path.Combine(tempDir, _notesFileName);
                if (File.Exists(remoteNotesPath))
                {
                    remoteNotes = File.ReadAllLines(remoteNotesPath).ToList();
                }

                CopyRepositoryFiles(tempDir, _notesDirectory, excludeFile: _notesFileName);

                if (localNotes != null && localNotes.Count > 0)
                {
                    ReportProgress("🔄 Reconciling local and remote notes...");
                    var merged = MergeNotes(remoteNotes, localNotes);
                    File.WriteAllLines(NotesFilePath, merged);
                    ReportProgress("✓ Notes merged successfully");
                }
                else if (remoteNotes.Count > 0)
                {
                    File.WriteAllLines(NotesFilePath, remoteNotes);
                    ReportProgress("✓ Remote notes downloaded");
                }
                else
                {
                    // Neither local nor remote notes exist; ensure file is present.
                    File.WriteAllText(NotesFilePath, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, $"❌ Failed to setup repository: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch
                {
                    // ignore cleanup failures
                }
            }

            return (true, string.Empty);
        }

        private void ReportProgress(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            ProgressChanged?.Invoke(message);
        }

        private void ReportCommandOutput(GitCommandResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.StdOut))
            {
                foreach (var line in result.StdOut.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ReportProgress(line);
                }
            }

            if (!string.IsNullOrWhiteSpace(result.StdErr))
            {
                foreach (var line in result.StdErr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ReportProgress($"⚠️ {line}");
                }
            }
        }

        private string? GetGitConfigValue(string key)
        {
            var result = RunGit(_notesDirectory, "config", key);
            if (!result.Success)
            {
                return null;
            }

            return result.StdOut.Trim();
        }

        private GitCommandResult RunGit(string workingDirectory, params string[] args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            foreach (var arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            try
            {
                using var process = Process.Start(psi);
                if (process == null)
                {
                    return new GitCommandResult(false, string.Empty, string.Empty, "Failed to start git process.", -1);
                }

                var stdOut = process.StandardOutput.ReadToEnd();
                var stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var command = string.Join(' ', args);
                    var message = string.IsNullOrWhiteSpace(stdErr)
                        ? $"git {command} failed with exit code {process.ExitCode}"
                        : stdErr.Trim();
                    return new GitCommandResult(false, stdOut, stdErr, $"❌ {message}", process.ExitCode);
                }

                return new GitCommandResult(true, stdOut, stdErr, string.Empty, process.ExitCode);
            }
            catch (Win32Exception)
            {
                return new GitCommandResult(false, string.Empty, string.Empty, "❌ Git CLI not found. Install Git for Windows or add it to PATH.", -1);
            }
            catch (Exception ex)
            {
                return new GitCommandResult(false, string.Empty, string.Empty, $"❌ Git command failed: {ex.Message}", -1);
            }
        }

        private static void MoveDirectory(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                return;
            }

            if (Directory.Exists(destinationDir))
            {
                Directory.Delete(destinationDir, true);
            }

            try
            {
                Directory.Move(sourceDir, destinationDir);
            }
            catch
            {
                CopyDirectory(sourceDir, destinationDir);
                Directory.Delete(sourceDir, true);
            }
        }

        private static void CopyRepositoryFiles(string sourceDir, string destinationDir, string excludeFile)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var name = Path.GetFileName(file);
                if (name.Equals(".git", StringComparison.OrdinalIgnoreCase) || name.Equals(excludeFile, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var targetFile = Path.Combine(destinationDir, name);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                File.Copy(file, targetFile, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var name = Path.GetFileName(directory);
                if (name.Equals(".git", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var targetDir = Path.Combine(destinationDir, name);
                CopyDirectory(directory, targetDir);
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var targetFile = Path.Combine(destinationDir, Path.GetFileName(file));
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                File.Copy(file, targetFile, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var targetSubDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, targetSubDir);
            }
        }

        private static List<string> MergeNotes(List<string> baseline, List<string> incoming)
        {
            var result = new List<string>(baseline);
            var indexByKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < result.Count; i++)
            {
                var key = GetNoteKey(result[i], out _);
                if (!string.IsNullOrEmpty(key))
                {
                    indexByKey[key] = i;
                }
            }

            foreach (var line in incoming)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var key = GetNoteKey(line, out var incomingTimestamp);
                if (!string.IsNullOrEmpty(key) && indexByKey.TryGetValue(key, out var existingIndex))
                {
                    var existingLine = result[existingIndex];
                    GetNoteKey(existingLine, out var existingTimestamp);

                    if (incomingTimestamp == DateTime.MinValue || incomingTimestamp >= existingTimestamp)
                    {
                        result[existingIndex] = line;
                    }
                }
                else
                {
                    result.Add(line);
                    if (!string.IsNullOrEmpty(key))
                    {
                        indexByKey[key] = result.Count - 1;
                    }
                }
            }

            return result;
        }

        private static string GetNoteKey(string line, out DateTime timestamp)
        {
            timestamp = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(line))
            {
                return string.Empty;
            }

            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith("[id:", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            var idEnd = trimmed.IndexOf(']');
            if (idEnd <= 4)
            {
                return trimmed;
            }

            var idKey = trimmed.Substring(0, idEnd + 1);
            var remaining = trimmed.Substring(idEnd + 1).TrimStart();

            const string pinnedMarker = "[PINNED]";
            if (remaining.StartsWith(pinnedMarker, StringComparison.OrdinalIgnoreCase))
            {
                remaining = remaining.Substring(pinnedMarker.Length).TrimStart();
            }

            if (remaining.Length >= 22 && remaining[0] == '[' && remaining[20] == ']' && remaining[21] == ' ')
            {
                var timestampText = remaining.Substring(1, 19);
                if (DateTime.TryParseExact(timestampText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    timestamp = parsed;
                }
            }

            return idKey;
        }

        private readonly struct GitCommandResult
        {
            public GitCommandResult(bool success, string stdOut, string stdErr, string message, int exitCode)
            {
                Success = success;
                StdOut = stdOut;
                StdErr = stdErr;
                Message = message;
                ExitCode = exitCode;
            }

            public bool Success { get; }
            public string StdOut { get; }
            public string StdErr { get; }
            public string Message { get; }
            public int ExitCode { get; }
        }
    }
}
