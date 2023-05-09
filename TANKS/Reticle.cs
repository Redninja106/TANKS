using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal class Reticle : IGameComponent
{
    Transform transform;

    public RenderLayer RenderLayer => RenderLayer.UI;

    public void Render(ICanvas canvas)
    {
        canvas.StrokeWidth(.05f);
        canvas.Stroke(Color.Red);
        canvas.DrawCircle(Program.MousePosition, .25f);
        canvas.DrawCircle(Program.MousePosition, .01f);

        for (int i = 0; i < 4; i++)
        {
            float r = i * (MathF.PI / 2);
            Vector2 v = Vector2.One.Normalized().Rotated(r + Program.Camera.Transform.Rotation);
            canvas.DrawLine(Program.MousePosition + v * .15f, Program.MousePosition + v * .35f);
        }
    }

    public void Update()
    {
    }
}
