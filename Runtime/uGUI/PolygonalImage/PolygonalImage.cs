using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
TODO – Polygon UI Image Roadmap
===============================

1) Stable triangulation
-----------------------
- Improve robustness against:
  - collinear vertices
  - very small angles
  - duplicate / nearly-duplicate points
- Enforce consistent winding order (CW / CCW) before triangulation
- Cache triangulation results and only recompute when topology changes
- Add validation step with clear editor warnings for invalid polygons

2) Multiple triangulation strategies
------------------------------------
- Abstract triangulation behind an interface (strategy pattern)
- Allow switching strategy per component (Inspector dropdown)

Planned strategies:
- Ear clipping (current, general-purpose)
- Fan triangulation (convex only, very fast)
- Center-vertex triangulation:
  - All triangles share a central vertex (pivot)
  - Useful for radial effects, breathing, scaling, and fills
- (Future) Monotone polygon triangulation
- (Future) Constrained Delaunay triangulation (high-quality meshes)

3) Holes and islands support
----------------------------
- Support polygons with:
  - holes (donut / cut-out shapes)
  - multiple disconnected islands
- Define data model:
  - outer contour
  - inner contours (holes)
- Ensure correct winding per contour type
- Extend triangulation to respect hole constraints
- Editor visualization for holes and islands

4) Predefined shapes dropdown
-----------------------------
- Inspector dropdown for common shapes:
  - Triangle
  - Rectangle / Quad
  - Pentagon
  - Hexagon
  - Octagon
  - Pyramid / Chevron / Arrow
- Shapes generated in normalized (-1..1) space
- Optional parameters:
  - corner count
  - aspect ratio
  - rotation
- Allow "Reset to Shape" without breaking animations

5) Vertex animation support
----------------------------
- Separate:
  - base (authoring) vertices
  - animated (runtime) vertices
- Cache triangulation and reuse it during animation
- Provide built-in animation helpers:
  - breathing / pulsing
  - wobble / noise
  - radial expansion
- Ensure animations:
  - do not change topology
  - do not allocate memory per frame
- Optional shader-based vertex animation for large batches

*/

public class PolygonalImage : Image
{
    [SerializeField]
    public List<Vector2> normalizedVertices = new List<Vector2>()
    {
        new Vector2(-1, -1),
        new Vector2( 1, -1),
        new Vector2( 1,  1),
        new Vector2(-1,  1),
    };

    // Cached buffers (avoid allocations)
    private readonly List<Vector2> _localVerts = new();
    private readonly List<int> _tris = new();

    // ------------------------------------------------------

    Vector2 NormalizedToLocal(Vector2 n)
    {
        Rect r = rectTransform.rect;
        return new Vector2(
            n.x * r.width  * 0.5f,
            n.y * r.height * 0.5f
        );
    }

    Vector2 LocalToNormalized(Vector2 l)
    {
        Rect r = rectTransform.rect;
        return new Vector2(
            l.x / (r.width  * 0.5f),
            l.y / (r.height * 0.5f)
        );
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif

    // ------------------------------------------------------

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (normalizedVertices == null || normalizedVertices.Count < 3)
            return;

        Rect rect = GetPixelAdjustedRect();
        Sprite s = sprite;

        Vector4 uv = s != null
            ? UnityEngine.Sprites.DataUtility.GetOuterUV(s)
            : new Vector4(0, 0, 1, 1);

        // Convert to local space (NO GC)
        _localVerts.Clear();
        for (int i = 0; i < normalizedVertices.Count; i++)
            _localVerts.Add(NormalizedToLocal(normalizedVertices[i]));

        // Triangulate (NEW API)
        PolygonTriangulator.Triangulate(_localVerts, _tris);

        // Build vertices
        for (int i = 0; i < _localVerts.Count; i++)
        {
            Vector2 p = _localVerts[i];

            float u = Mathf.InverseLerp(rect.xMin, rect.xMax, p.x);
            float v = Mathf.InverseLerp(rect.yMin, rect.yMax, p.y);

            Vector2 uv0 = new Vector2(
                Mathf.Lerp(uv.x, uv.z, u),
                Mathf.Lerp(uv.y, uv.w, v)
            );

            vh.AddVert(p, color, uv0);
        }

        // Build triangles
        for (int i = 0; i < _tris.Count; i += 3)
        {
            vh.AddTriangle(_tris[i], _tris[i + 1], _tris[i + 2]);
        }
    }
}