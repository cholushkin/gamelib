
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GameLib.Editor
{
    public class UPMUtilsWindow : EditorWindow
    {
        private const float StatusCellWidth = 128f;
        private const float StatusGridWidth = StatusCellWidth * 2f;
        private const float IconColumnWidth = 72f;
        private const float DetailIconSize = 72f;

        private Vector2 _scrollPosition;

        private readonly HashSet<string> _expandedModules =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Color LocalSourceColor =
            new Color(0.20f, 0.40f, 0.70f);

        private static readonly Color GitSourceColor =
            new Color(0.55f, 0.30f, 0.65f);

        private static readonly Color StatusInactiveColor =
            new Color(0.5f, 0.5f, 0.5f);

        private static readonly Color StatusGoodColor =
            new Color(0.25f, 0.70f, 0.30f);

        private static readonly Color StatusBadColor =
            new Color(0.80f, 0.25f, 0.25f);

        private static readonly Color TagColor =
            new Color(0.95f, 0.65f, 0.20f);

        private static GUIStyle _richMiniLabelStyle;
        private static GUIStyle _statusGreyStyle;
        private static GUIStyle _statusGoodStyle;
        private static GUIStyle _statusBadStyle;
        private static GUIStyle _detailTitleStyle;
        private static GUIStyle _richTagsStyle;

        [MenuItem("GameLib/UPM Utils/UPM Utils")]
        public static void Open()
        {
            GetWindow<UPMUtilsWindow>("UPM Utils");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += Repaint;
            PullLocalUPMPackages.StatusChanged += Repaint;

            // Automatically inspect packages when first opened.
            EditorApplication.delayCall += CheckOnOpen;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
            PullLocalUPMPackages.StatusChanged -= Repaint;
            EditorApplication.delayCall -= CheckOnOpen;
        }

        private void CheckOnOpen()
        {
            if (!this)
                return;

            if (!GameLibModuleScanner.HasScanned)
                GameLibModuleScanner.Scan();

            if (PullLocalUPMPackages.IsRunning ||
                PullLocalUPMPackages.HasCachedStatuses)
                return;

            CheckAll();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);

            DrawSamplesSection();

            EditorGUILayout.Space(12);

            DrawPackagesSection();
        }

        private void DrawSamplesSection()
        {
            EditorGUILayout.LabelField(
                "Samples", EditorStyles.boldLabel);

            bool hasSample =
                PushSampleBackToPackage.TryFindSelectedSample(out _);

            EditorGUILayout.HelpBox(
                "Select an imported sample under Assets/Samples " +
                "in the Project window.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(!hasSample))
            {
                if (GUILayout.Button(
                    "Push Selected Sample Back to Package",
                    GUILayout.Height(28)))
                {
                    PushSampleBackToPackage.PushSelected();
                }
            }
        }

        private void DrawPackagesSection()
        {
            EditorGUILayout.LabelField(
                "GameLib Modules (UPM)", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(
                PullLocalUPMPackages.IsRunning))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(
                        "Pull All from Git",
                        GUILayout.Height(30)))
                    {
                        PullAll();
                    }

                    if (GUILayout.Button(
                        "Status Check",
                        GUILayout.Height(30)))
                    {
                        CheckAll();
                    }
                }
            }

            if (PullLocalUPMPackages.IsRunning)
            {
                EditorGUILayout.HelpBox(
                    "Git operation is running. " +
                    "Check Unity Console for details.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(6);

            _scrollPosition = EditorGUILayout.BeginScrollView(
                _scrollPosition);

            int count = 0;

            foreach (var module in GameLibModuleScanner.Modules)
            {
                count++;
                DrawPackage(module);
            }

            if (count == 0)
            {
                EditorGUILayout.HelpBox(
                    GameLibModuleScanner.HasScanned
                        ? "No GameLib modules found among local or " +
                          "Git-installed UPM packages."
                        : "Scanning packages for GameLib module " +
                          "manifests...",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPackage(GameLibModuleScanner.ModuleInfo module)
        {
            var package = module.Package;

            bool isLocal =
                package.source == PackageSource.Local ||
                package.source == PackageSource.Embedded;

            bool isExpanded = _expandedModules.Contains(package.name);

            using (new EditorGUILayout.VerticalScope(
                EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        DrawPackageInfo(module, isLocal);
                    }

                    using (new EditorGUILayout.VerticalScope(
                        GUILayout.Width(StatusGridWidth)))
                    {
                        DrawStatusGrid(package, isLocal);
                    }

                    DrawManifestIconColumn(module, isExpanded);
                }

                if (isExpanded)
                    DrawModuleDetails(module);
            }
        }

        // Exactly four lines: name, version/source, path, buttons. Kept in
        // lockstep with the four-row status grid and the full-height icon
        // next to it so every column in the row lines up.
        private void DrawPackageInfo(
            GameLibModuleScanner.ModuleInfo module,
            bool isLocal)
        {
            var package = module.Package;

            EditorGUILayout.LabelField(
                package.name, EditorStyles.boldLabel);

            DrawVersionSourceLine(package, isLocal);

            EditorGUILayout.SelectableLabel(
                package.resolvedPath,
                EditorStyles.miniLabel,
                GUILayout.Height(
                    EditorGUIUtility.singleLineHeight));

            if (module.HasMultipleManifests)
            {
                EditorGUILayout.HelpBox(
                    $"Multiple module manifests found in this " +
                    $"package ({module.ManifestAssetPaths.Length}). " +
                    "Using the first one found.",
                    MessageType.Warning);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (isLocal)
                {
                    using (new EditorGUI.DisabledScope(
                        PullLocalUPMPackages.IsRunning))
                    {
                        if (GUILayout.Button(
                            "Pull", GUILayout.Width(65)))
                        {
                            PullPackage(package);
                        }

                        if (GUILayout.Button(
                            "Check", GUILayout.Width(65)))
                        {
                            CheckPackage(package);
                        }
                    }
                }

                if (GUILayout.Button(
                    "Open Folder", GUILayout.Width(100)))
                {
                    EditorUtility.RevealInFinder(
                        package.resolvedPath);
                }

                if (GUILayout.Button(
                    "Copy Path", GUILayout.Width(90)))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        package.resolvedPath;
                }
            }
        }

        private static void DrawVersionSourceLine(
            PackageInfo package, bool isLocal)
        {
            Color sourceColor = GetSourceColor(package.source);
            string hex = ColorUtility.ToHtmlStringRGB(sourceColor);

            string line =
                $"Version: {package.version}  |  Source: " +
                $"<color=#{hex}><b>{package.source}</b></color>";

            if (!isLocal &&
                package.git != null &&
                !string.IsNullOrEmpty(package.git.hash))
            {
                string shortHash = package.git.hash.Length > 7
                    ? package.git.hash.Substring(0, 7)
                    : package.git.hash;

                line += $"  |  Rev: {shortHash}";
            }

            EditorGUILayout.LabelField(line, RichMiniLabelStyle);
        }

        private static Color GetSourceColor(PackageSource source)
        {
            switch (source)
            {
                case PackageSource.Local:
                case PackageSource.Embedded:
                    return LocalSourceColor;

                case PackageSource.Git:
                    return GitSourceColor;

                default:
                    return StatusInactiveColor;
            }
        }

        // Always exactly 4 rows x 2 columns = 8 status cells, in the same
        // order every frame, so the control count never changes between a
        // Layout and a Repaint pass (a varying tag count there used to
        // corrupt the layout of the row next to it) and so the grid's
        // total height always matches the 4-line info column and the
        // full-height icon beside it.
        private static void DrawStatusGrid(PackageInfo package, bool isLocal)
        {
            GetStatusCells(
                package, isLocal, out var column1, out var column2);

            for (int i = 0; i < 4; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawStatusCell(column1[i]);
                    DrawStatusCell(column2[i]);
                }
            }
        }

        private static void GetStatusCells(
            PackageInfo package,
            bool isLocal,
            out StatusCell[] column1,
            out StatusCell[] column2)
        {
            if (!isLocal)
            {
                column1 = new[]
                {
                    new StatusCell("Managed by UPM", false, true),
                    StatusCell.Empty,
                    StatusCell.Empty,
                    StatusCell.Empty
                };

                column2 = new[]
                {
                    StatusCell.Empty,
                    StatusCell.Empty,
                    StatusCell.Empty,
                    StatusCell.Empty
                };

                return;
            }

            PullLocalUPMPackages.TryGetStatus(package, out var status);

            column1 = new[]
            {
                new StatusCell(
                    "No changes", status?.IsClean ?? false, true),
                new StatusCell(
                    "Not committed changes",
                    status?.HasUncommittedChanges ?? false, false),
                new StatusCell(
                    "Not pushed changes",
                    status?.HasUnpushedChanges ?? false, false),
                new StatusCell(
                    "Not pulled changes",
                    status?.HasUnpulledChanges ?? false, false)
            };

            column2 = new[]
            {
                new StatusCell(
                    "Diverged", status?.IsDiverged ?? false, false),
                new StatusCell(
                    "No upstream",
                    !(status?.HasUpstream ?? true), false),
                new StatusCell(
                    "Fetch failed",
                    !string.IsNullOrEmpty(status?.RemoteCheckError), false),
                new StatusCell(
                    "Git error",
                    !string.IsNullOrEmpty(status?.Error), false)
            };
        }

        private static void DrawStatusCell(StatusCell cell)
        {
            GUIStyle style;

            if (!cell.Active)
                style = StatusGreyStyle;
            else if (cell.IsGood)
                style = StatusGoodStyle;
            else
                style = StatusBadStyle;

            EditorGUILayout.LabelField(
                cell.Label, style, GUILayout.Width(StatusCellWidth));
        }

        private readonly struct StatusCell
        {
            public static readonly StatusCell Empty =
                new StatusCell("", false, true);

            public readonly string Label;
            public readonly bool Active;
            public readonly bool IsGood;

            public StatusCell(string label, bool active, bool isGood)
            {
                Label = label;
                Active = active;
                IsGood = isGood;
            }
        }

        private void DrawManifestIconColumn(
            GameLibModuleScanner.ModuleInfo module,
            bool isExpanded)
        {
            Rect rect = GUILayoutUtility.GetRect(
                IconColumnWidth,
                10f,
                GUILayout.Width(IconColumnWidth),
                GUILayout.ExpandHeight(true));

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);

            var icon = module.Manifest != null
                ? GameLibIconRecolor.GetGreenIcon(module.Manifest.Icon)
                : null;

            if (icon != null)
            {
                var padded = new Rect(
                    rect.x + 4, rect.y + 4,
                    rect.width - 8, rect.height - 8);

                GUI.DrawTexture(padded, icon, ScaleMode.ScaleToFit);
            }

            if (isExpanded)
            {
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.yMax - 16, rect.width, 16),
                    "▲", EditorStyles.centeredGreyMiniLabel);
            }

            if (clicked)
                ToggleExpanded(module.Package.name);
        }

        private void ToggleExpanded(string packageName)
        {
            if (!_expandedModules.Remove(packageName))
                _expandedModules.Add(packageName);
        }

        // Inlined module-details section shown below a row when its icon is
        // toggled open. Mirrors the layout the standalone module info popup
        // used to show, but wraps within the package list's own width and
        // scroll view instead of a fixed-size window.
        private static void DrawModuleDetails(
            GameLibModuleScanner.ModuleInfo module)
        {
            var manifest = module.Manifest;

            EditorGUILayout.Space(6);

            if (manifest == null)
            {
                EditorGUILayout.HelpBox(
                    "Module manifest is not available.", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawDetailIcon(manifest);

                GUILayout.Space(12);

                using (new EditorGUILayout.VerticalScope(
                    GUILayout.ExpandHeight(true)))
                {
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField(
                        string.IsNullOrEmpty(manifest.Name)
                            ? module.Package.name
                            : manifest.Name,
                        DetailTitleStyle);

                    EditorGUILayout.Space(4);

                    EditorGUILayout.LabelField(
                        $"Module version: {manifest.Version}",
                        EditorStyles.miniLabel);

                    EditorGUILayout.LabelField(
                        $"Package: {module.Package.name} " +
                        $"({module.Package.version})",
                        EditorStyles.miniLabel);

                    GUILayout.FlexibleSpace();
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Description", EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(manifest.Description)
                    ? "(no description)"
                    : manifest.Description,
                EditorStyles.wordWrappedLabel);

            DrawDetailTags(manifest);
        }

        private static void DrawDetailIcon(GameLibModuleManifest manifest)
        {
            Rect rect = GUILayoutUtility.GetRect(
                DetailIconSize,
                DetailIconSize,
                GUILayout.Width(DetailIconSize),
                GUILayout.Height(DetailIconSize));

            var texture = GameLibIconRecolor.GetGreenIcon(manifest.Icon);

            if (texture != null)
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
        }

        private static void DrawDetailTags(GameLibModuleManifest manifest)
        {
            if (manifest.Tags == null || manifest.Tags.Length == 0)
                return;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);

            string hex = ColorUtility.ToHtmlStringRGB(TagColor);
            var builder = new StringBuilder();

            for (int i = 0; i < manifest.Tags.Length; i++)
            {
                if (i > 0)
                    builder.Append("   ");

                builder.Append("<color=#").Append(hex).Append('>');
                builder.Append('#').Append(manifest.Tags[i]);
                builder.Append("</color>");
            }

            EditorGUILayout.LabelField(builder.ToString(), RichTagsStyle);
        }

        private static GUIStyle RichMiniLabelStyle
        {
            get
            {
                if (_richMiniLabelStyle == null)
                {
                    _richMiniLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        richText = true
                    };
                }

                return _richMiniLabelStyle;
            }
        }

        private static GUIStyle DetailTitleStyle
        {
            get
            {
                if (_detailTitleStyle == null)
                {
                    _detailTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 16
                    };
                }

                return _detailTitleStyle;
            }
        }

        private static GUIStyle RichTagsStyle
        {
            get
            {
                if (_richTagsStyle == null)
                {
                    _richTagsStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        richText = true,
                        wordWrap = true
                    };
                }

                return _richTagsStyle;
            }
        }

        private static GUIStyle StatusGreyStyle =>
            _statusGreyStyle ??= CreateStatusStyle(StatusInactiveColor);

        private static GUIStyle StatusGoodStyle =>
            _statusGoodStyle ??= CreateStatusStyle(StatusGoodColor);

        private static GUIStyle StatusBadStyle =>
            _statusBadStyle ??= CreateStatusStyle(StatusBadColor);

        private static GUIStyle CreateStatusStyle(Color color)
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

            style.normal.textColor = color;
            return style;
        }

        private async void PullAll()
        {
            try
            {
                await PullLocalUPMPackages.PullAllAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (this)
                Repaint();
        }

        private async void PullPackage(PackageInfo package)
        {
            try
            {
                await PullLocalUPMPackages.PullPackageAsync(package);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (this)
                Repaint();
        }

        private async void CheckAll()
        {
            GameLibModuleScanner.Scan();

            try
            {
                await PullLocalUPMPackages.CheckAllAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (this)
                Repaint();
        }

        private async void CheckPackage(PackageInfo package)
        {
            try
            {
                await PullLocalUPMPackages.CheckPackageAsync(package);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (this)
                Repaint();
        }
    }
}

#endif
