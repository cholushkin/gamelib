using System.Collections.Generic;
using UnityEngine;

// todo: add convex polygon fast-path triangulation (fan method)
// todo: add center-based triangulation mode for radial effects and animations
// todo: cache triangulation results and only recompute when topology changes
// todo: detect and enforce consistent winding order (CW/CCW)
// todo: implement vertex deduplication with epsilon threshold
// todo: handle collinear and near-degenerate edges robustly
// todo: support polygons with holes (outer + inner contours)
// todo: support multiple disconnected polygon islands
// todo: abstract triangulation behind strategy interface (ear clipping, fan, etc.)
// todo: add runtime switch for triangulation strategy
// todo: add predefined shape generator (triangle, quad, hex, circle approximation)
// todo: support parametric shapes (radius, sides, rotation, aspect ratio)
// todo: add editor tooling for vertex manipulation (scene handles)
// todo: add ability to insert/remove vertices in editor
// todo: add polygon validation with warnings (self-intersection, invalid input)
// todo: add debug visualization (edges, normals, triangulation)
// todo: support UV mapping modes (stretch, fit, tile, radial)
// todo: support sprite atlas and sliced sprite compatibility
// todo: integrate material presets (unlit, sprite, custom shader)
// todo: add outline rendering (geometry-based and shader-based options)
// todo: add glow/emission support via shader integration
// todo: support per-vertex color and gradient fills
// todo: add vertex animation system (wobble, pulse, noise, radial expansion)
// todo: separate base vertices and runtime animated vertices
// todo: ensure zero-GC runtime updates for animation
// todo: add GPU-driven vertex animation option (shader-based deformation)
// todo: support batching-friendly mesh updates
// todo: add collider generation (PolygonCollider2D sync)
// todo: support runtime morphing between shapes
// todo: implement LOD or simplification for complex polygons
// todo: support Bezier curves and spline-based polygon generation
// todo: add SVG import pipeline (convert paths to polygons)
// todo: add export/debug tools (save mesh, visualize indices)
// todo: add unit tests for triangulation correctness and edge cases

public static class PolygonTriangulator
{
    // Reusable index buffer (avoids allocations)
    private static readonly List<int> _V = new();

    public static void Triangulate(List<Vector2> points, List<int> result)
    {
        result.Clear();

        int n = points.Count;
        if (n < 3)
            return;

        _V.Clear();
        for (int i = 0; i < n; i++)
            _V.Add(i);

        // Ensure correct winding (CCW)
        if (Area(points) < 0f)
            _V.Reverse();

        int nv = n;
        int count = 2 * nv;

        int v = nv - 1;

        while (nv > 2)
        {
            if ((count--) <= 0)
            {
                // Failed → likely bad polygon
                Debug.LogWarning("Triangulation failed: possibly non-simple polygon");
                return;
            }

            int u = v;
            if (u >= nv) u = 0;

            v = u + 1;
            if (v >= nv) v = 0;

            int w = v + 1;
            if (w >= nv) w = 0;

            if (Snip(points, u, v, w, nv))
            {
                int a = _V[u];
                int b = _V[v];
                int c = _V[w];

                result.Add(a);
                result.Add(b);
                result.Add(c);

                // Remove v from polygon
                _V.RemoveAt(v);

                nv--;
                count = 2 * nv;
            }
        }
    }

    static float Area(List<Vector2> points)
    {
        float A = 0f;

        for (int p = points.Count - 1, q = 0; q < points.Count; p = q++)
        {
            Vector2 p0 = points[p];
            Vector2 p1 = points[q];
            A += (p0.x * p1.y - p1.x * p0.y);
        }

        return A * 0.5f;
    }

    static bool Snip(List<Vector2> points, int u, int v, int w, int n)
    {
        Vector2 A = points[_V[u]];
        Vector2 B = points[_V[v]];
        Vector2 C = points[_V[w]];

        // Check if triangle is degenerate or flipped
        if (Cross(B - A, C - A) <= Mathf.Epsilon)
            return false;

        // Check if any point is inside triangle
        for (int p = 0; p < n; p++)
        {
            if (p == u || p == v || p == w)
                continue;

            Vector2 P = points[_V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }

        return true;
    }

    static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ab = Cross(B - A, P - A);
        float bc = Cross(C - B, P - B);
        float ca = Cross(A - C, P - C);

        // Allow small epsilon tolerance
        return ab >= -Mathf.Epsilon &&
               bc >= -Mathf.Epsilon &&
               ca >= -Mathf.Epsilon;
    }
}