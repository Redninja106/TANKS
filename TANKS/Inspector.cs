using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal class Inspector
{
    object? selected;
    bool open;

    public void Layout()
    {
        if (Keyboard.IsKeyPressed(Key.F1))
            open = !open;

        if (open && ImGui.Begin("Inspector", ref open))
        {
            if (selected is IInspectable inspectable)
            {
                inspectable.Layout();
            }
        }
    }

    public void Select(object obj)
    {
        selected = obj;
    }
}
