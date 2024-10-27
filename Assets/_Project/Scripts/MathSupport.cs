using System;
using System.Drawing;
using UnityEngine;

public static class MathSupport
{
    private enum Orientation
    {
        Collinear,
        Clockwise,
        CounterClockwise
    }

    private static Orientation CheckOrientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float value = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

        if (Mathf.Abs(value) < float.Epsilon) return Orientation.Collinear;
        return (value > 0) ? Orientation.Clockwise : Orientation.CounterClockwise;
    }

    private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        return q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
               q.y <= Math.Max(p.y, r.y) && q.x >= Math.Min(p.y, r.y);
    }

    public static bool IsIntersect(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
    {
        Orientation o1 = CheckOrientation(A, B, C);
        Orientation o2 = CheckOrientation(A, B, D);
        Orientation o3 = CheckOrientation(C, D, A);
        Orientation o4 = CheckOrientation(C, D, B);

        // General case: segments intersect if orientations differ
        if (o1 != o2 && o3 != o4)
            return true;

        // Special cases: check if any points are collinear and on segment
        if (o1 == 0 && OnSegment(A, C, B)) return true;
        if (o2 == 0 && OnSegment(A, D, B)) return true;
        if (o3 == 0 && OnSegment(C, A, D)) return true;
        if (o4 == 0 && OnSegment(C, B, D)) return true;

        return false;
    }

    public static Vector2 GetIntersectionPoint(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
    {
        float denominator = (A.x - B.x) * (C.y - D.y) - (A.y - B.y) * (C.x - D.x);

        float det1 = (A.x * B.y - A.y * B.x);
        float det2 = (C.x * D.y - C.y * D.x);

        float intersectX = (det1 * (C.x - D.x) - (A.x - B.x) * det2) / denominator;
        float intersectY = (det1 * (C.y - D.y) - (A.y - B.y) * det2) / denominator;

        return new Vector2(intersectX, intersectY);
    }

    public static Vector2 AngleToVector(float angle, float rotationOffset)
    {
        angle += rotationOffset;

        float x = (float)Math.Round(Mathf.Cos(angle * Mathf.Deg2Rad), 3);
        float y = (float)Math.Round(Mathf.Sin(-angle * Mathf.Deg2Rad), 3);

        return new Vector2(x, y);
    }

    public static float VectorToAngle(Vector2 vector, float rotationOffset)
    {
        float radians = (float)Mathf.Atan2(vector.y, vector.x);

        return -(radians * Mathf.Rad2Deg) - rotationOffset;
    }
}
