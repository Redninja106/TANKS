using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal interface ICollider : IPositionable
{
    // static objects are not checked against other static objects, so they shouldn't move
    bool IsStatic { get; }

    Rectangle GetBounds();

    void OnCollision(ICollider other, Vector2 mtv);
}
