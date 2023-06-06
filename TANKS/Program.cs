using Silk.NET.Input.Glfw;
using Silk.NET.Windowing.Glfw;
using SimulationFramework;
using SimulationFramework.Desktop;
using SimulationFramework.Drawing;
using System.Diagnostics;
using System.Numerics;
using TANKS;
using TANKS.Particles;

new Program().RunDesktop();

partial class Program : Simulation
{
    public static World World { get; private set; }

    private static World? nextWorld = null;

    public static Camera Camera { get; private set; }
    public static Vector2 MousePosition { get; private set; }
    public static Inspector Inspector { get; private set; }

    public override void OnInitialize(AppConfig config)
    {
        Camera = new(15);
        Camera.Transform.Rotation = MathF.PI / 2f;
        Inspector = new();

        ReloadLevel("./Levels/final.lvl");
        
        config.Title = "TANKS!";
        Time.MaxDeltaTime = 1 / 30f;
    }

    public override void OnRender(ICanvas canvas)
    {
        if (nextWorld is not null)
            World = nextWorld;

        canvas.Clear(Color.FromHSV(0,0,.1f));

        Camera.Update(canvas.Width, canvas.Height);

        MousePosition = Vector2.Transform(Mouse.Position, Camera.LocalToScreen.InverseMatrix * Camera.WorldToLocal.InverseMatrix);
        
        Inspector.Layout();
        World.Update();

        Camera.Update(canvas.Width, canvas.Height);

        canvas.Antialias(true);

        canvas.PushState();
        Camera.ApplyTo(canvas);
        World.Render(canvas);
        canvas.PopState();

        canvas.DrawText(Performance.Framerate.ToString("f0"), new(Camera.DisplayWidth, 0), Alignment.TopRight);
    }

    public static void ReloadLevel(string path)
    {
        nextWorld = new();
        var reader = new LevelReader();
        foreach (var component in reader.GetComponents(path))
        {
            nextWorld.Add(component);
        }
    }
}