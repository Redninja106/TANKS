using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
// "gameplay"
internal class GameplayController : IGameComponent
{
    public RenderLayer RenderLayer => RenderLayer.UI;

    int boxCount = 0;

    public void Render(ICanvas canvas)
    {
        canvas.PushState();
        canvas.ResetState();

        canvas.DrawText($"Boxes left: {boxCount}", new(Program.Camera.DisplayWidth / 2f, 0), Alignment.TopCenter);
        
        canvas.PopState();
    }

    public void Update()
    {
        boxCount = Program.World.Components.Count(c => c is Box);
    }
}
