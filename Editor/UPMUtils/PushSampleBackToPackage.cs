
#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GameLib.Editor
{
    public static class PushSampleBackToPackage
    {
        private const string MenuPath =
            "Assets/Push Sample Back to Package";

        [MenuItem(MenuPath, false, 2000)]
        private static void ExecuteMenu()
        {
            PushSelected();
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateMenu()
        {
            return TryFindSelectedSample(out _);
        }

        public static bool PushSelected()
        {
            if (!TryFindSelectedSample(out var sample))
            {
                Debug.LogWarning(
                    "[UPM] Select an imported sample " +
                    "under Assets/Samples.");
                return false;
            }

            return Push(sample);
        }

        public static bool Push(Sample sample)
        {
            try
            {
                if (!sample.isImported ||
                    string.IsNullOrEmpty(sample.importPath) ||
                    string.IsNullOrEmpty(sample.resolvedPath))
                {
                    Debug.LogWarning("[UPM] Invalid imported sample.");
                    return false;
                }

                string source = Path.GetFullPath(sample.importPath);
                string destination = Path.GetFullPath(sample.resolvedPath);

                if (!Directory.Exists(source) ||
                    !Directory.Exists(destination))
                {
                    Debug.LogError(
                        "[UPM] Sample source or destination does not exist.");
                    return false;
                }

                if (!IsWritablePackagePath(destination))
                {
                    Debug.LogError(
                        "[UPM] Destination is not inside a " +
                        "local or embedded package.");
                    return false;
                }

                int count = CopyDirectory(source, destination);

                AssetDatabase.Refresh();

                Debug.Log(
                    $"[UPM] Push Sample Back to Package: " +
                    $"copied {count} files.\n" +
                    $"From: {source}\n" +
                    $"To: {destination}");

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }

        public static bool TryFindSelectedSample(out Sample result)
        {
            result = default;

            string assetPath = AssetDatabase.GetAssetPath(
                Selection.activeObject);

            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.StartsWith(
                    "Assets/Samples/",
                    StringComparison.OrdinalIgnoreCase))
                return false;

            string projectRoot = Path.GetDirectoryName(
                Application.dataPath);

            string selectedPath = Path.GetFullPath(
                Path.Combine(projectRoot, assetPath));

            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                if (!IsWritable(package))
                    continue;

                var samples = Sample.FindByPackage(
                    package.name, package.version);

                foreach (var sample in samples)
                {
                    if (!sample.isImported ||
                        string.IsNullOrEmpty(sample.importPath) ||
                        string.IsNullOrEmpty(sample.resolvedPath))
                        continue;

                    string importPath = Path.GetFullPath(
                        sample.importPath);

                    string resolvedPath = Path.GetFullPath(
                        sample.resolvedPath);

                    if (!IsInside(selectedPath, importPath))
                        continue;

                    if (!Directory.Exists(importPath) ||
                        !Directory.Exists(resolvedPath))
                        continue;

                    if (!IsInside(resolvedPath, package.resolvedPath))
                        continue;

                    result = sample;
                    return true;
                }
            }

            return false;
        }

        private static bool IsWritablePackagePath(string path)
        {
            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                if (IsWritable(package) &&
                    !string.IsNullOrEmpty(package.resolvedPath) &&
                    IsInside(path, package.resolvedPath))
                    return true;
            }

            return false;
        }

        private static bool IsWritable(PackageInfo package)
        {
            return package.source == PackageSource.Local ||
                   package.source == PackageSource.Embedded;
        }

        private static bool IsInside(string path, string directory)
        {
            path = Path.GetFullPath(path).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            directory = Path.GetFullPath(directory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            var comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return string.Equals(path, directory, comparison) ||
                   path.StartsWith(
                       directory + Path.DirectorySeparatorChar,
                       comparison);
        }

        private static int CopyDirectory(
            string source,
            string destination)
        {
            int count = 0;

            foreach (string file in Directory.GetFiles(
                         source, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(source.Length)
                    .TrimStart(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar);

                string target = Path.Combine(destination, relativePath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(target));

                File.Copy(file, target, overwrite: true);
                count++;
            }

            return count;
        }
    }
}

#endif
