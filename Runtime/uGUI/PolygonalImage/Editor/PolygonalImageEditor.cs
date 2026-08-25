#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PolygonalImage))]
public class PolygonalImageEditor : Editor
{
    // Cached buffers (avoid GC in Scene view)
    private readonly List<Vector2> _localVerts = new();
    private readonly List<int> _tris = new();

    void OnSceneGUI()
    {
        PolygonalImage img = (PolygonalImage)target;
        RectTransform rt = img.rectTransform;

        Rect r = rt.rect;
        if (r.width <= 0 || r.height <= 0)
            return;

        // Convert normalized → local (NO GC)
        _localVerts.Clear();
        foreach (var n in img.normalizedVertices)
        {
            _localVerts.Add(new Vector2(
                n.x * r.width  * 0.5f,
                n.y * r.height * 0.5f
            ));
        }

        // Triangulate (NEW API)
        PolygonTriangulator.Triangulate(_localVerts, _tris);

        // Draw triangulation
        Handles.color = new Color(0, 1, 1, 0.6f);

        for (int i = 0; i < _tris.Count; i += 3)
        {
            Vector3 a = rt.TransformPoint(_localVerts[_tris[i]]);
            Vector3 b = rt.TransformPoint(_localVerts[_tris[i + 1]]);
            Vector3 c = rt.TransformPoint(_localVerts[_tris[i + 2]]);

            Handles.DrawLine(a, b);
            Handles.DrawLine(b, c);
            Handles.DrawLine(c, a);
        }

        // Move vertices
        Handles.color = Color.white;

        for (int i = 0; i < _localVerts.Count; i++)
        {
            Vector3 worldPos = rt.TransformPoint(_localVerts[i]);

            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.FreeMoveHandle(
                worldPos,
                HandleUtility.GetHandleSize(worldPos) * 0.08f,
                Vector3.zero,
                Handles.DotHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(img, "Move Polygon Vertex");

                Vector2 newLocal = rt.InverseTransformPoint(newWorldPos);

                img.normalizedVertices[i] = new Vector2(
                    newLocal.x / (r.width  * 0.5f),
                    newLocal.y / (r.height * 0.5f)
                );

                img.SetVerticesDirty();
                EditorUtility.SetDirty(img);
            }
        }
    }
}
#endif