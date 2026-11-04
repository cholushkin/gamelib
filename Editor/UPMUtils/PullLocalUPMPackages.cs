
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GameLib.Editor
{
    public static class PullLocalUPMPackages
    {
        public enum PullResult
        {
            Success,
            Skipped,
            Failed
        }

        public sealed class PackageGitStatus
        {
            public string PackageName { get; internal set; }
            public bool IsStandaloneRepository { get; internal set; }
            public bool HasUncommittedChanges { get; internal set; }
            public bool HasUpstream { get; internal set; }
            public int UnpushedCommitCount { get; internal set; }
            public string Upstream { get; internal set; }
            public string ChangedFiles { get; internal set; }
            public string Note { get; internal set; }
            public string Error { get; internal set; }

            public bool HasUnpushedChanges =>
                UnpushedCommitCount > 0;

            public bool IsClean =>
                IsStandaloneRepository &&
                HasUpstream &&
                !HasUncommittedChanges &&
                !HasUnpushedChanges &&
                string.IsNullOrEmpty(Error);
        }

        private sealed class GitResult
        {
            public int ExitCode;
            public string StdOut;
            public string StdErr;

            public string Output =>
                (StdOut + "\n" + StdErr).Trim();
        }

        private sealed class PullOutcome
        {
            public PullResult Result;
            public bool HasNewChanges;
        }

        private static readonly Dictionary<string, PackageGitStatus>
            StatusCache = new Dictionary<string, PackageGitStatus>(
                StringComparer.OrdinalIgnoreCase);

        public static bool IsRunning { get; private set; }

        public static bool HasCachedStatuses => StatusCache.Count > 0;

        public static event Action StatusChanged;

        [MenuItem("GameLib/UPM Utils/Pull Local Packages")]
        private static async void PullMenu()
        {
            await PullAllAsync();
        }

        [MenuItem("GameLib/UPM Utils/Pull Local Packages", true)]
        private static bool ValidatePullMenu()
        {
            return !IsRunning;
        }

        [MenuItem("GameLib/UPM Utils/Check Local Changes")]
        private static async void CheckMenu()
        {
            await CheckAllAsync();
        }

        [MenuItem("GameLib/UPM Utils/Check Local Changes", true)]
        private static bool ValidateCheckMenu()
        {
            return !IsRunning;
        }

        public static bool TryGetStatus(
            PackageInfo package,
            out PackageGitStatus status)
        {
            status = null;

            if (package == null ||
                string.IsNullOrEmpty(package.resolvedPath))
                return false;

            string path = Path.GetFullPath(package.resolvedPath);
            return StatusCache.TryGetValue(path, out status);
        }

        public static async Task PullAllAsync()
        {
            if (!TryBegin())
                return;

            int ok = 0;
            int updated = 0;
            int failed = 0;
            int skipped = 0;

            try
            {
                Debug.Log("[UPM Pull] Starting...");

                foreach (var package in GetLocalPackages())
                {
                    var outcome = await PullInternalAsync(package);

                    switch (outcome.Result)
                    {
                        case PullResult.Success:
                            ok++;
                            if (outcome.HasNewChanges)
                                updated++;
                            break;

                        case PullResult.Skipped:
                            skipped++;
                            break;

                        case PullResult.Failed:
                            failed++;
                            break;
                    }

                    // Keep the window's status tags up to date.
                    await UpdateStatusAsync(package, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EndOperation();

                AssetDatabase.Refresh();

                Debug.Log(
                    $"[UPM Pull] Finished.\n" +
                    $"OK: {ok}\n" +
                    $"With new changes: {updated}\n" +
                    $"Skipped: {skipped}\n" +
                    $"Failed: {failed}");
            }
        }

        public static async Task<PullResult> PullPackageAsync(
            PackageInfo package)
        {
            if (!TryBegin())
                return PullResult.Skipped;

            try
            {
                var outcome = await PullInternalAsync(package);
                await UpdateStatusAsync(package, false);

                return outcome.Result;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return PullResult.Failed;
            }
            finally
            {
                EndOperation();
                AssetDatabase.Refresh();
            }
        }

        public static async Task CheckAllAsync()
        {
            if (!TryBegin())
                return;

            int clean = 0;
            int uncommitted = 0;
            int unpushed = 0;
            int other = 0;

            try
            {
                StatusCache.Clear();
                NotifyStatusChanged();

                Debug.Log("[UPM Status] Checking local packages...");

                foreach (var package in GetLocalPackages())
                {
                    var status = await UpdateStatusAsync(package, true);

                    if (status.IsClean)
                        clean++;

                    if (status.HasUncommittedChanges)
                        uncommitted++;

                    if (status.HasUnpushedChanges)
                        unpushed++;

                    if (!status.IsClean &&
                        !status.HasUncommittedChanges &&
                        !status.HasUnpushedChanges)
                        other++;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EndOperation();

                Debug.Log(
                    $"[UPM Status] Finished.\n" +
                    $"Clean: {clean}\n" +
                    $"With uncommitted changes: {uncommitted}\n" +
                    $"With unpushed commits: {unpushed}\n" +
                    $"Other / unknown: {other}");
            }
        }

        public static async Task<PackageGitStatus> CheckPackageAsync(
            PackageInfo package)
        {
            if (!TryBegin())
                return null;

            try
            {
                return await UpdateStatusAsync(package, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            finally
            {
                EndOperation();
            }
        }

        private static bool TryBegin()
        {
            if (IsRunning)
            {
                Debug.LogWarning(
                    "[UPM] Another Git operation is already running.");
                return false;
            }

            IsRunning = true;
            NotifyStatusChanged();
            return true;
        }

        private static void EndOperation()
        {
            IsRunning = false;
            NotifyStatusChanged();
        }

        private static void NotifyStatusChanged()
        {
            StatusChanged?.Invoke();
        }

        private static bool IsLocal(PackageInfo package)
        {
            return package != null &&
                   (package.source == PackageSource.Local ||
                    package.source == PackageSource.Embedded);
        }

        private static IEnumerable<PackageInfo> GetLocalPackages()
        {
            var seen = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                if (!IsLocal(package) ||
                    string.IsNullOrEmpty(package.resolvedPath))
                    continue;

                string path = Path.GetFullPath(package.resolvedPath);

                if (seen.Add(path))
                    yield return package;
            }
        }

        private static async Task<PullOutcome> PullInternalAsync(
            PackageInfo package)
        {
            var outcome = new PullOutcome
            {
                Result = PullResult.Skipped
            };

            if (!IsLocal(package) ||
                string.IsNullOrEmpty(package.resolvedPath))
            {
                Debug.LogWarning(
                    "[UPM Pull] Only Local and Embedded packages " +
                    "are supported.");
                return outcome;
            }

            string name = package.name;

            try
            {
                string path = Path.GetFullPath(package.resolvedPath);

                if (!Directory.Exists(path))
                {
                    Debug.LogWarning(
                        $"[UPM Pull] SKIP {name}: directory not found.");
                    return outcome;
                }

                var root = await RunGitAsync(
                    path, "rev-parse --show-toplevel");

                if (root.ExitCode != 0 ||
                    !SamePath(root.StdOut.Trim(), path))
                {
                    Debug.Log(
                        $"[UPM Pull] SKIP {name}: " +
                        "not a standalone Git repository.");
                    return outcome;
                }

                var before = await RunGitAsync(path, "rev-parse HEAD");

                if (before.ExitCode != 0)
                {
                    Debug.LogError(
                        $"[UPM Pull] FAILED {name}\n{before.Output}");

                    outcome.Result = PullResult.Failed;
                    return outcome;
                }

                Debug.Log($"[UPM Pull] Pulling {name}\n{path}");

                var pull = await RunGitAsync(path, "pull --ff-only");

                if (pull.ExitCode != 0)
                {
                    Debug.LogError(
                        $"[UPM Pull] FAILED {name}\n" +
                        $"Exit code: {pull.ExitCode}\n{pull.Output}");

                    outcome.Result = PullResult.Failed;
                    return outcome;
                }

                var after = await RunGitAsync(path, "rev-parse HEAD");

                bool hasNewChanges = false;
                string changedFiles = "";

                if (after.ExitCode == 0 &&
                    !string.Equals(
                        before.StdOut.Trim(),
                        after.StdOut.Trim(),
                        StringComparison.Ordinal))
                {
                    // Compare actual tracked files, not Git output text.
                    string args =
                        $"diff --name-only {before.StdOut.Trim()} " +
                        $"{after.StdOut.Trim()} --";

                    var diff = await RunGitAsync(path, args);

                    if (diff.ExitCode == 0)
                    {
                        changedFiles = diff.StdOut.Trim();
                        hasNewChanges =
                            !string.IsNullOrEmpty(changedFiles);
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[UPM Pull] Unable to compare " +
                            $"commits for {name}:\n{diff.Output}");
                    }
                }

                string label = hasNewChanges
                    ? "OK (new changes!)"
                    : "OK";

                Debug.Log(
                    $"[UPM Pull] {label} {name}\n" +
                    $"{pull.Output}" +
                    (hasNewChanges
                        ? $"\nChanged files:\n{changedFiles}"
                        : ""));

                outcome.Result = PullResult.Success;
                outcome.HasNewChanges = hasNewChanges;
                return outcome;
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[UPM Pull] FAILED {name}\n{e}");

                outcome.Result = PullResult.Failed;
                return outcome;
            }
        }

        private static async Task<PackageGitStatus> UpdateStatusAsync(
            PackageInfo package,
            bool printToConsole)
        {
            var status = await ReadStatusAsync(package);

            if (package != null &&
                !string.IsNullOrEmpty(package.resolvedPath))
            {
                string path = Path.GetFullPath(package.resolvedPath);
                StatusCache[path] = status;
                NotifyStatusChanged();
            }

            if (printToConsole)
                LogStatus(status);

            return status;
        }

        private static async Task<PackageGitStatus> ReadStatusAsync(
            PackageInfo package)
        {
            var status = new PackageGitStatus
            {
                PackageName = package != null
                    ? package.name
                    : "(unknown)"
            };

            try
            {
                if (!IsLocal(package) ||
                    string.IsNullOrEmpty(package.resolvedPath))
                {
                    status.Note = "Not a local or embedded package.";
                    return status;
                }

                string path = Path.GetFullPath(package.resolvedPath);

                if (!Directory.Exists(path))
                {
                    status.Error = "Package directory not found.";
                    return status;
                }

                var root = await RunGitAsync(
                    path, "rev-parse --show-toplevel");

                if (root.ExitCode != 0)
                {
                    status.Note = "Not a Git repository.";
                    return status;
                }

                if (!SamePath(root.StdOut.Trim(), path))
                {
                    status.Note =
                        "Git root differs from the package directory.";
                    return status;
                }

                status.IsStandaloneRepository = true;

                // Includes staged, unstaged and untracked files.
                var workingTree = await RunGitAsync(
                    path,
                    "status --porcelain=v1 --untracked-files=all");

                if (workingTree.ExitCode != 0)
                {
                    status.Error = workingTree.Output;
                    return status;
                }

                status.ChangedFiles = workingTree.StdOut.Trim();
                status.HasUncommittedChanges =
                    !string.IsNullOrEmpty(status.ChangedFiles);

                var upstream = await RunGitAsync(
                    path,
                    "rev-parse --abbrev-ref --symbolic-full-name @{upstream}");

                if (upstream.ExitCode != 0)
                {
                    status.Note =
                        "No upstream configured; " +
                        "unpushed commits cannot be determined.";
                    return status;
                }

                status.HasUpstream = true;
                status.Upstream = upstream.StdOut.Trim();

                // Commits in HEAD that are not in the upstream ref.
                var ahead = await RunGitAsync(
                    path, "rev-list --count @{upstream}..HEAD");

                if (ahead.ExitCode != 0 ||
                    !int.TryParse(
                        ahead.StdOut.Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int count))
                {
                    status.Error =
                        "Could not determine unpushed commits.\n" +
                        ahead.Output;
                    return status;
                }

                status.UnpushedCommitCount = count;
            }
            catch (Exception e)
            {
                status.Error = e.ToString();
            }

            return status;
        }

        private static void LogStatus(PackageGitStatus status)
        {
            var message = new StringBuilder();

            message.AppendLine(
                $"[UPM Status] {status.PackageName}");

            if (!string.IsNullOrEmpty(status.Error))
            {
                message.AppendLine(status.Error);
                Debug.LogError(message.ToString());
                return;
            }

            if (!status.IsStandaloneRepository)
            {
                message.AppendLine(
                    $"SKIP: {status.Note}");
                Debug.Log(message.ToString());
                return;
            }

            if (status.HasUncommittedChanges)
            {
                message.AppendLine("NOT COMMITTED:");
                message.AppendLine(status.ChangedFiles);
            }

            if (status.HasUnpushedChanges)
            {
                message.AppendLine(
                    $"NOT PUSHED: {status.UnpushedCommitCount} " +
                    $"commit(s) ahead of {status.Upstream}");
            }

            if (!status.HasUpstream)
            {
                message.AppendLine(status.Note);
            }

            if (status.IsClean)
            {
                message.AppendLine("No changes.");
                Debug.Log(message.ToString());
            }
            else
            {
                Debug.LogWarning(message.ToString());
            }
        }

        private static bool SamePath(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) ||
                string.IsNullOrWhiteSpace(b))
                return false;

            var comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return string.Equals(
                Path.GetFullPath(a).TrimEnd('/', '\\'),
                Path.GetFullPath(b).TrimEnd('/', '\\'),
                comparison);
        }

        private static async Task<GitResult> RunGitAsync(
            string directory,
            string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process.Start();

                var stdoutTask =
                    process.StandardOutput.ReadToEndAsync();

                var stderrTask =
                    process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit());

                return new GitResult
                {
                    ExitCode = process.ExitCode,
                    StdOut = await stdoutTask,
                    StdErr = await stderrTask
                };
            }
        }
    }
}

#endif
