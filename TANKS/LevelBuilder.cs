using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TANKS;
internal class LevelBuilder : IGameComponent
{
    public RenderLayer RenderLayer => RenderLayer.UI;

    bool active;

    public void Render(ICanvas canvas)
    {
        if (!active)
            return;

        canvas.PushState();
        canvas.ResetState();

        string desc = 
@"Level Editor Active:
1 - wall
2 - box
delete/backspace - remove objects";

        canvas.FontStyle(20, FontStyle.Normal);
        foreach (var line in desc.Split(Environment.NewLine))
        {
            canvas.DrawText(line, Vector2.Zero);
            canvas.Translate(0, 24);
        }

        canvas.PopState();

        Vector2 coords = new(MathF.Round(Program.MousePosition.X * 2) / 2f, MathF.Round(Program.MousePosition.Y * 2) / 2f);
        if (wallStartPos is not null)
        {
            Vector2 diff = coords - wallStartPos.Value;
            var transform = new Transform(wallStartPos.Value + diff * .5f, Angle.FromVector(diff));
            canvas.ApplyTransform(transform);
            canvas.Fill(Color.FromHSV(0, 0, .9f, .5f));
            canvas.DrawRect(0, 0, diff.Length() + .5f, .5f, Alignment.Center);
        }
    }

    Vector2? wallStartPos;

    public void Update()
    {
        if (Keyboard.IsKeyPressed(Key.F2))
            active = !active;

        if (Keyboard.IsKeyPressed(Key.F3))
            saveWindow = !saveWindow;

        LayoutSaveWindow();

        if (!active)
        {
            wallStartPos = null;
            return;
        }

        Vector2 coords = new(MathF.Round(Program.MousePosition.X * 2) / 2f, MathF.Round(Program.MousePosition.Y * 2) / 2f);

        bool hitTest = Program.World.Collision.TestPoint(Program.MousePosition, null, out var hitCollider);

        if (Keyboard.IsKeyPressed(Key.Delete) || Keyboard.IsKeyPressed(Key.Backspace))
        {
            if (hitTest && hitCollider is IGameComponent g)
            {
                Program.World.Remove(g);
            }
        }

        if (Keyboard.IsKeyPressed(Key.Key2) && !hitTest)
        {
            Program.World.Add(new Box(new(coords, 0), .5f, .5f));
        }

        if (Keyboard.IsKeyPressed(Key.Key1))
        {
            if (wallStartPos is null)
            {
                wallStartPos = coords;
            }
            else
            {
                Vector2 diff = coords - wallStartPos.Value;
                var transform = new Transform(wallStartPos.Value + diff * .5f, Angle.FromVector(diff));
                Program.World.Add(new Wall(transform, diff.Length() + .5f, .5f));
                wallStartPos = null;
            }
        }

        if (Keyboard.IsKeyPressed(Key.Esc) && wallStartPos is not null)
        {
            wallStartPos = null;
        }
    }

    bool saveWindow = false;
    string currentLevelName = "";
    string[]? levels;

    private void LayoutSaveWindow()
    {
        if (saveWindow && ImGui.Begin("Save/Load Levels", ref saveWindow))
        {
            ImGui.InputText("Level File", ref currentLevelName, 128);

            if (!ImGui.IsItemFocused() && string.IsNullOrEmpty(currentLevelName))
            {
                currentLevelName = GetBestLevelFileName();
            }

            if (ImGui.Button("Save Current Level"))
            {
                if (!currentLevelName.EndsWith(".lvl"))
                    currentLevelName += ".lvl";

                SaveLevel($"Levels/{currentLevelName}");
            }

            if (ImGui.CollapsingHeader("Load Levels"))
            {
                if (levels is null || ImGui.Button("Refresh"))
                {
                    levels = Directory.GetFiles("Levels/", "*.lvl", SearchOption.AllDirectories);
                }

                foreach (var level in levels)
                {
                    if (ImGui.Selectable(level))
                    {
                        Program.ReloadLevel(level);
                    }
                }
            }
        }
        ImGui.End();
    }

    void SaveLevel(string fileName)
    {
        StringBuilder result = new();

        foreach (var component in Program.World.Components.Reverse())
        {
            if (component is Projectile)
                continue;

            result.Append(component.GetType().Name);

            if (component is Box box)
            {
                result.Append($" {box.Transform.Position.X:f3} {box.Transform.Position.Y:f3}");
            }

            if (component is Wall wall)
            {
                var direction = Angle.ToVector(wall.Transform.Rotation) * (wall.width - .5f);
                var from = wall.Transform.Position - direction * .5f;
                var to = wall.Transform.Position + direction * .5f;

                result.Append($" {from.X:f3} {from.Y:f3} {to.X:f3} {to.Y:f3}");
            }

            result.AppendLine();
        }

        File.WriteAllText(fileName , result.ToString());
    }

    private string GetBestLevelFileName()
    {
        int levelNumber = 0;
        while (File.Exists($"Levels/test{levelNumber}.lvl"))
            levelNumber++;

        return $"test{levelNumber}.lvl";
    }
}
