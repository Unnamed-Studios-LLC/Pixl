global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.IO;

using Pixl.Demo.Systems;

namespace Pixl.Demo
{
    public class Entry : IGameEntry
    {
        public void OnStart(Scene scene)
        {
            Time.Precise.TargetUpdateDelta = 0;
            Time.Precise.FixedTotal = Time.PreciseTicksPerSecond / 60;

            scene.AddSystem<RenderingSystem>();
            scene.AddSystem<CanvasSystem>();
            scene.AddSystem<VelocitySystem>();
            scene.AddSystem<InputSystem>();
        }
    }
}
