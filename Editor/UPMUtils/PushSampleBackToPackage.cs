
using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace GameLib.Editor
{
    public static class PushSampleBackToPackage
    {
        private const string MenuPath = "Assets/Push Sample Back to Package";

        [MenuItem(MenuPath, false, 2000)]
        private static void Execute()
        {
            if (!TryFindSelectedSample(out var sample))
                return;

            try
            {
                string source = Path.GetFullPath(sample.importPath);
                string destination = Path.GetFullPath(sample.resolvedPath);

                int count = CopyDirectory(source, destination);

                AssetDatabase.Refresh();

                Debug.Log(
                    $"[Push Sample Back to Package] " +
                    $"Copied {count} files.\n" +
                    $"From: {source}\n" +
                    $"To: {destination}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate()
        {
            return TryFindSelectedSample(out _);
        }

        private static bool TryFindSelectedSample(out Sample result)
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

            foreach (var package in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                // Only writable development packages.
                if (package.source != PackageSource.Local &&
                    package.source != PackageSource.Embedded)
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

                    // Ensure destination belongs to this package.
                    if (!IsInside(
                            resolvedPath,
                            Path.GetFullPath(package.resolvedPath)))
                        continue;

                    result = sample;
                    return true;
                }
            }

            return false;
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

                string target = Path.Combine(
                    destination, relativePath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(target));

                File.Copy(file, target, overwrite: true);
                count++;
            }

            return count;
        }
    }
}
