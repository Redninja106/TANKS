using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal class CollisionManager
{
    World world;
    public bool DrawColliders { get; set; }
    private ICollider[] colliders;

    public CollisionManager(World world)
    {
        this.world = world;
    }

    public void Update()
    {
        colliders = world.Components.OfType<ICollider>().ToArray();

        Span<Vector2> colliderOBB = stackalloc Vector2[4];
        Span<Vector2> otherOBB = stackalloc Vector2[4];

        foreach (var collider in colliders)
        {
            foreach (var other in colliders)
            {
                if (collider == other)
                    continue;

                if (collider.IsStatic && other.IsStatic)
                    continue;

                var colliderRect = collider.GetBounds();
                var colliderTransform = collider.Transform;
                GetOBB(colliderRect, colliderTransform, colliderOBB);

                var otherRect = other.GetBounds();
                var otherTransform = other.Transform;
                GetOBB(otherRect, otherTransform, otherOBB);

                if (SATCollision(colliderOBB, otherOBB, out Vector2 mtv))
                {
                    if (Vector2.Dot(otherTransform.Position - colliderTransform.Position, mtv) > 0)
                    {
                        mtv = -mtv;
                    }

                    collider.OnCollision(other, mtv);
                    other.OnCollision(collider, -mtv);
                }
            }
        }
    }

    public void Render(ICanvas canvas)
    {
        if (DrawColliders)
        {
            Span<Vector2> colliderOBB = stackalloc Vector2[4];
            var colliders = world.Components.OfType<ICollider>();
            
            foreach (var collider in colliders)
            {
                var colliderRect = collider.GetBounds();
                var colliderTransform = collider.Transform;
                GetOBB(colliderRect, colliderTransform, colliderOBB);
                DrawShape(canvas, colliderOBB);
            }
        }
    }

    void DrawShape(ICanvas canvas, Span<Vector2> shape)
    {
        canvas.PushState();
        canvas.Transform(Program.Camera.LocalToScreen.Matrix);
        canvas.Transform(Program.Camera.WorldToLocal.Matrix);

        canvas.Stroke(Color.Red);
        canvas.DrawPolygon(shape);
        canvas.DrawLine(shape[0], shape[^1]);
        canvas.PopState();
    }

    private bool SATCollision(Span<Vector2> shapeA, Span<Vector2> shapeB, out Vector2 mtv)
    {
        Span<Vector2> axes = stackalloc Vector2[shapeA.Length + shapeB.Length - 2];
        GetAxes(shapeA, axes);
        GetAxes(shapeB, axes[(shapeA.Length - 1)..]);

        float leastOverlap = float.PositiveInfinity;
        Vector2 leastOverlapAxis = default;

        for (int i = 0; i < axes.Length; i++)
        {
            Project(shapeA, axes[i], out float aMin, out float aMax);
            Project(shapeB, axes[i], out float bMin, out float bMax);

            if ((aMin < bMax && aMin > bMin) || (bMin < aMax && bMin > aMin))
            {
                float overlap = MathF.Min(aMax, bMax) - MathF.Max(aMin, bMin);

                if (overlap < leastOverlap)
                {
                    leastOverlap = overlap;
                    leastOverlapAxis = axes[i];
                }

                continue;
            }

            mtv = default;
            return false;
        }

        mtv = leastOverlapAxis * leastOverlap;
        return true;
    }

    private void GetAxes(Span<Vector2> shape, Span<Vector2> axes)
    {
        for (int i = 0; i < shape.Length - 1; i++)
        {
            Vector2 side = shape[i] - shape[i + 1];
            axes[i] = new(side.Y, -side.X);
            axes[i] = axes[i].Normalized();
        }
    }

    private void Project(Span<Vector2> shape, Vector2 axis, out float min, out float max)
    {
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;

        for (int i = 0; i < shape.Length; i++)
        {
            Vector2 vertex = shape[i];

            float dot = Vector2.Dot(vertex, axis);

            min = Math.Min(min, dot);
            max = Math.Max(max, dot);
        }
    }

    private void GetOBB(Rectangle rectangle, Transform transform, Span<Vector2> result)
    {
        result[0] = transform.Position + rectangle.GetAlignedPoint(Alignment.TopLeft).Rotated(transform.Rotation);
        result[1] = transform.Position + rectangle.GetAlignedPoint(Alignment.TopRight).Rotated(transform.Rotation);
        result[2] = transform.Position + rectangle.GetAlignedPoint(Alignment.BottomRight).Rotated(transform.Rotation);
        result[3] = transform.Position + rectangle.GetAlignedPoint(Alignment.BottomLeft).Rotated(transform.Rotation);
    }

    public bool RayCast(Vector2 position, Vector2 direction, float maxDistance, out RayCastHit hit)
    {
        return RayCast(position, direction, maxDistance, null, out hit);
    }

    public bool RayCast(Vector2 position, Vector2 direction, float maxDistance, Predicate<ICollider>? filter, out RayCastHit hit)
    {
        Span<Vector2> obb = stackalloc Vector2[4];
        float closest = maxDistance;
        ICollider? closestCollider = null;
        Vector2 closestNormal = default;

        foreach (var collider in colliders)
        {
            if (filter is not null && !filter(collider))
                continue;

            GetOBB(collider.GetBounds(), collider.Transform, obb);

            for (int i = 0; i < obb.Length; i++)
            {
                Vector2 from = obb[i];
                Vector2 to = obb[i + 1 >= obb.Length ? 0 : (i + 1)];

                float? dist = RayLineIntersect(position, direction, from, to);
                if (dist is not null)
                {
                    if (dist < closest)
                    {
                        closest = dist.Value;
                        closestCollider = collider;
                        Vector2 dir = (to - from);
                        closestNormal = new Vector2(dir.Y, -dir.X).Normalized();
                    }
                }
            }
        }

        hit.position = position;
        hit.direction = direction;
        hit.distance = closest;
        hit.collider = closestCollider;
        hit.normal = closestNormal;

        return hit.collider is not null;
    }

    public float? RayLineIntersect(Vector2 position, Vector2 direction, Vector2 from, Vector2 to)
    {
        var v1 = position - from;
        var v2 = to - from;
        var v3 = new Vector2(-direction.Y, direction.X);


        var dot = Vector2.Dot(v2, v3);
        if (Math.Abs(dot) < 0.0001f)
            return null;

        var t1 = Cross(v2, v1) / dot;
        var t2 = Vector2.Dot(v1, v3) / dot;

        if (t1 >= 0.0f && (t2 >= 0.0f && t2 <= 1.0f))
            return t1;

        return null;
    }

    private float Cross(Vector2 a, Vector2 b)
    {
        return Vector3.Cross(new(a, 0), new(b, 0)).Z;
    }

    public bool TestPoint(Vector2 point, Predicate<ICollider>? filter = null)
    {
        return TestPoint(point, filter, out _);
    }

    public bool TestPoint(Vector2 point, Predicate<ICollider>? filter, [NotNullWhen(true)] out ICollider? hit)
    {
        foreach (var collider in colliders)
        {
            if (filter is not null && !filter(collider))
                continue;

            var localPoint = (point - collider.Transform.Position).Rotated(-collider.Transform.Rotation);

            if (collider.GetBounds().ContainsPoint(localPoint))
            {
                hit = collider;
                return true;
            }
        }

        hit = null;
        return false;
    }
}

struct RayCastHit
{
    public Vector2 position;
    public Vector2 direction;
    public float distance;
    public ICollider? collider;
    public Vector2 normal;

    public Vector2 Point => position + direction * distance;
}