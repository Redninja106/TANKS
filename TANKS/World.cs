using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANKS;
internal class World
{
    private readonly List<IGameComponent> components = new();
    private readonly Queue<IGameComponent> addQueue = new();
    private readonly Queue<IGameComponent> removeQueue = new();

    public CollisionManager Collision;

    public IEnumerable<IGameComponent> Components => components;

    public World()
    {
        Collision = new(this);
    }

    public void Update()
    {
        Collision.Update();

        foreach (var e in components)
        {
            e.Update();
        }

        while (addQueue.Any())
        {
            components.Add(addQueue.Dequeue());
        }

        while (removeQueue.Any())
        {
            components.Remove(removeQueue.Dequeue());
        }
    }

    public void Add(IGameComponent entity)
    {
        addQueue.Enqueue(entity);
    }

    public void Remove(IGameComponent entity)
    {
        removeQueue.Enqueue(entity);
    }

    public void Render(ICanvas canvas)
    {
        foreach (var e in components)
        {
            canvas.PushState();
            e.Render(canvas);
            canvas.PopState();
        }

        components.Sort((a, b) => Comparer<RenderLayer>.Default.Compare(a.RenderLayer, b.RenderLayer));

        Stack<int> ints;
    }
}
