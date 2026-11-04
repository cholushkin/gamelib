
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GameLib.Editor
{
    public class UPMUtilsWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        private static readonly Color CleanColor =
            new Color(0.15f, 0.55f, 0.25f);

        private static readonly Color ChangedColor =
            new Color(0.75f, 0.20f, 0.20f);

        private static readonly Color NeutralColor =
            new Color(0.40f, 0.40f, 0.40f);

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
            if (!this ||
                PullLocalUPMPackages.IsRunning ||
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
                "Local UPM Packages", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(
                    PullLocalUPMPackages.IsRunning))
                {
                    if (GUILayout.Button(
                        "Pull All from Git",
                        GUILayout.Height(30)))
                    {
                        PullAll();
                    }

                    if (GUILayout.Button(
                        "Check Local Changes",
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

            var packages = PackageInfo.GetAllRegisteredPackages();

            _scrollPosition = EditorGUILayout.BeginScrollView(
                _scrollPosition);

            int count = 0;

            foreach (var package in packages)
            {
                if (package.source != PackageSource.Local &&
                    package.source != PackageSource.Embedded)
                    continue;

                count++;
                DrawPackage(package);
            }

            if (count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No local or embedded UPM packages found.",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPackage(PackageInfo package)
        {
            using (new EditorGUILayout.VerticalScope(
                EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        package.name,
                        EditorStyles.boldLabel,
                        GUILayout.ExpandWidth(false));

                    GUILayout.FlexibleSpace();

                    DrawPackageStatus(package);
                }

                EditorGUILayout.LabelField(
                    $"Version: {package.version}  |  " +
                    $"Source: {package.source}",
                    EditorStyles.miniLabel);

                EditorGUILayout.SelectableLabel(
                    package.resolvedPath,
                    EditorStyles.miniLabel,
                    GUILayout.Height(
                        EditorGUIUtility.singleLineHeight));

                using (new EditorGUILayout.HorizontalScope())
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
        }

        private static void DrawPackageStatus(PackageInfo package)
        {
            if (!PullLocalUPMPackages.TryGetStatus(
                    package, out var status))
            {
                DrawTag(
                    PullLocalUPMPackages.IsRunning
                        ? "Checking..."
                        : "Not checked",
                    NeutralColor);
                return;
            }

            if (!string.IsNullOrEmpty(status.Error))
            {
                DrawTag("Git error", ChangedColor);
                return;
            }

            if (!status.IsStandaloneRepository)
            {
                DrawTag("Not a package Git repo", NeutralColor);
                return;
            }

            bool hasChanges = false;

            if (status.HasUncommittedChanges)
            {
                DrawTag("Not committed changes", ChangedColor);
                hasChanges = true;
            }

            if (status.HasUnpushedChanges)
            {
                DrawTag("Not pushed changes", ChangedColor);
                hasChanges = true;
            }

            if (!status.HasUpstream)
            {
                DrawTag("No upstream", NeutralColor);
            }
            else if (!hasChanges)
            {
                DrawTag("No changes", CleanColor);
            }
        }

        private static void DrawTag(string label, Color color)
        {
            var style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(7, 7, 2, 2)
            };

            style.normal.textColor = Color.white;

            var content = new GUIContent(label);
            float width = style.CalcSize(content).x + 4;

            Rect rect = GUILayoutUtility.GetRect(
                width,
                20f,
                GUILayout.ExpandWidth(false));

            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, content, style);
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
