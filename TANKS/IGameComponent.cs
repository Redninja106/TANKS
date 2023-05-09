using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal interface IGameComponent
{
    RenderLayer RenderLayer { get; }

    void Render(ICanvas canvas);
    void Update();
}