using SimulationFramework;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TANKS.Extensions;

namespace TANKS;
internal class Camera
{
    public readonly MatrixBuilder WorldToLocal = new();
    public readonly MatrixBuilder LocalToScreen = new();

    public Transform Transform;

    public float AspectRatio { get; private set; }

    public float VerticalSize { get; set; }
    public float HorizontalSize { get => VerticalSize * AspectRatio; set => VerticalSize = value / AspectRatio; }

    public int DisplayWidth { get; private set; }
    public int DisplayHeight { get; private set; }

    public Camera(float verticalSize)
    {
        VerticalSize = verticalSize;
    }

    public void Update(int displayWidth, int displayHeight)
    {
        this.DisplayWidth = displayWidth;
        this.DisplayHeight = displayHeight;

        AspectRatio = displayWidth / (float)displayHeight;

        LocalToScreen
            .Reset()
            .Translate(displayWidth / 2f, displayHeight / 2f)
            .Scale(displayHeight / VerticalSize);

        WorldToLocal
            .Reset()
            .Rotate(-Transform.Rotation)
            .Translate(-Transform.Position);
    }

    public void ApplyTo(ICanvas canvas)
    {
        canvas.Transform(LocalToScreen.Matrix);
        canvas.Transform(WorldToLocal.Matrix);
    }
}
