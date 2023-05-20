using Pixl.Demo.Systems;
using System.Diagnostics.CodeAnalysis;

namespace Pixl.Demo
{
    public class Entry : IGameEntry
    {
        public void OnStart(Scene scene)
        {
            Time.Precise.FixedTotal = Time.PreciseTicksPerSecond / 60;

            scene.AddSystem<RenderingSystem>();
            scene.AddSystem<CanvasSystem>();
            scene.AddSystem<VelocitySystem>();
        }
    }
}
