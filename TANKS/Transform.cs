using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal struct Transform
{
    public Vector2 Position;
    public float Rotation;

    public Transform(float x, float y, float rotation) : this(new(x, y), rotation)
    {
    }

    public Transform(Vector2 position, float rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public Vector2 Forward
    { 
        get => Vector2.UnitX.Rotated(Rotation); 
        set => Rotation = MathF.Atan2(value.Y, value.X); 
    }
    public Vector2 Backward
    {
        get => (-Vector2.UnitX).Rotated(Rotation);
    }
    public Vector2 Up
    {
        get => Vector2.UnitY.Rotated(Rotation);
    }
    public Vector2 Down
    {
        get => (-Vector2.UnitY).Rotated(Rotation);
    }

    public Transform Translated(Vector2 vector, bool local = true)
    {
        if (local)
            vector = vector.Rotated(this.Rotation);

        return this with { Position = this.Position + vector };
    }

    public Transform Rotated(float rotation)
    {
        return this with { Rotation = this.Rotation + rotation };
    }
}
