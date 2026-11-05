
#if UNITY_EDITOR

using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameLib.Editor
{
    public class GameLibModuleInfoWindow : EditorWindow
    {
        private const float HeaderIconSize = 150f;

        private static readonly Color TagColor =
            new Color(0.95f, 0.65f, 0.20f);

        private static GUIStyle _titleStyle;
        private static GUIStyle _richTagsStyle;

        private GameLibModuleManifest _manifest;
        private string _packageName;
        private string _packageVersion;

        public static void Open(
            GameLibModuleManifest manifest,
            string packageName,
            string packageVersion)
        {
            var window = CreateInstance<GameLibModuleInfoWindow>();

            window._manifest = manifest;
            window._packageName = packageName;
            window._packageVersion = packageVersion;

            window.titleContent = new GUIContent(
                manifest != null && !string.IsNullOrEmpty(manifest.Name)
                    ? manifest.Name
                    : "Module Info");

            window.minSize = new Vector2(460, 320);
            window.maxSize = new Vector2(460, 620);

            window.ShowUtility();
        }

        private void OnGUI()
        {
            if (_manifest == null)
            {
                EditorGUILayout.HelpBox(
                    "Module manifest is not available.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(6);

            // The icon drives the row's height; the text column stretches
            // to match it and centers itself vertically alongside it.
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawIcon();

                GUILayout.Space(12);

                using (new EditorGUILayout.VerticalScope(
                    GUILayout.ExpandHeight(true)))
                {
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField(
                        string.IsNullOrEmpty(_manifest.Name)
                            ? _packageName
                            : _manifest.Name,
                        TitleStyle);

                    EditorGUILayout.Space(4);

                    EditorGUILayout.LabelField(
                        $"Module version: {_manifest.Version}",
                        EditorStyles.miniLabel);

                    EditorGUILayout.LabelField(
                        $"Package: {_packageName} ({_packageVersion})",
                        EditorStyles.miniLabel);

                    GUILayout.FlexibleSpace();
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(_manifest.Description)
                    ? "(no description)"
                    : _manifest.Description,
                EditorStyles.wordWrappedLabel);

            DrawTags();
        }

        private void DrawTags()
        {
            if (_manifest.Tags == null || _manifest.Tags.Length == 0)
                return;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);

            string hex = ColorUtility.ToHtmlStringRGB(TagColor);
            var builder = new StringBuilder();

            for (int i = 0; i < _manifest.Tags.Length; i++)
            {
                if (i > 0)
                    builder.Append("   ");

                builder.Append("<color=#").Append(hex).Append('>');
                builder.Append('#').Append(_manifest.Tags[i]);
                builder.Append("</color>");
            }

            EditorGUILayout.LabelField(builder.ToString(), RichTagsStyle);
        }

        private void DrawIcon()
        {
            Rect rect = GUILayoutUtility.GetRect(
                HeaderIconSize,
                HeaderIconSize,
                GUILayout.Width(HeaderIconSize),
                GUILayout.Height(HeaderIconSize));

            var texture = GameLibIconRecolor.GetGreenIcon(_manifest.Icon);

            if (texture != null)
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
        }

        private static GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 16
                    };
                }

                return _titleStyle;
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
    }
}

#endif
