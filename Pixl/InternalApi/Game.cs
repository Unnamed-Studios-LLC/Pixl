global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.IO;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Pixl.Demo")]
[assembly: InternalsVisibleTo("Pixl.Editor")]

[assembly: InternalsVisibleTo("Pixl.Android")]
[assembly: InternalsVisibleTo("Pixl.Android.Editor")]
[assembly: InternalsVisibleTo("Pixl.Android.Player")]

[assembly: InternalsVisibleTo("Pixl.iOS")]
[assembly: InternalsVisibleTo("Pixl.iOS.Editor")]
[assembly: InternalsVisibleTo("Pixl.iOS.Player")]

[assembly: InternalsVisibleTo("Pixl.Mac")]
[assembly: InternalsVisibleTo("Pixl.Mac.Editor")]
[assembly: InternalsVisibleTo("Pixl.Mac.Player")]

[assembly: InternalsVisibleTo("Pixl.Win")]
[assembly: InternalsVisibleTo("Pixl.Win.Editor")]
[assembly: InternalsVisibleTo("Pixl.Win.Player")]

namespace Pixl;

internal sealed class Game : App
{
    private static Game? s_shared;

    private long _nextFixedTime;

    internal Game(Logger logger, Window window, Graphics graphics, Resources resources, Values values, Files files) : base(window, graphics, resources, values, logger, files)
    {
        Scene = new Scene(this);
    }

    public static Game Shared => s_shared ?? throw new NullReferenceException("Shared Game is null");
    public InputState Input { get; } = new();
    public Scene Scene { get; }

    public RenderTexture? TargetRenderTexture { get; set; }

    public bool GetKey(KeyCode keyCode)
    {
        ref var record = ref Input.GetKeyRecord(keyCode);
        return record.State == KeyState.Pressed;
    }

    public bool GetKeyDown(KeyCode keyCode)
    {
        ref var record = ref Input.GetKeyRecord(keyCode);
        return record.State == KeyState.Pressed && record.Time == Time.Precise.Total;
    }

    public bool GetKeyUp(KeyCode keyCode)
    {
        ref var record = ref Input.GetKeyRecord(keyCode);
        return record.State == KeyState.Released && record.Time == Time.Precise.Total;
    }

    public override bool ProcessEvent(ref WindowEvent @event)
    {
        switch (@event.Type)
        {
            case WindowEventType.KeyDown:
                Input.OnKeyDown((KeyCode)@event.ValueA, Time.Precise.Total);
                break;
            case WindowEventType.KeyUp:
                Input.OnKeyUp((KeyCode)@event.ValueA, Time.Precise.Total);
                break;
            case WindowEventType.Unfocused:
                Input.Clear();
                break;
        }
        return base.ProcessEvent(ref @event);
    }

    public override void Start()
    {
        base.Start();
        s_shared = this;
        Scene.Start();
    }

    public override void Stop()
    {
        Scene.Stop();
        if (s_shared == this) s_shared = null;
        base.Stop();
    }

    public override void Render()
    {
        if (!Graphics.Setup) return;

        // sync size
        if (TargetRenderTexture == null)
        {
            Graphics.UpdateWindowSize(Window.Size);
        }
        else if (TargetRenderTexture.Framebuffer == null)
        {
            // render texture is not null and frame buffer is null, unknown error
            return;
        }
        else
        {
            TargetRenderTexture.Resize(Int2.Max(Int2.One, Window.Size));
        }

        var commands = Graphics.Commands;
        var frameBuffer = TargetRenderTexture?.Framebuffer ?? Graphics.SwapchainFramebuffer;
        Scene.Render(Graphics, commands, frameBuffer);
    }

    public override void Update()
    {
        FixedUpdates();
        Scene.Update();
    }

    private void FixedUpdates()
    {
        while (_nextFixedTime <= Time.Precise.Total)
        {
            var fixedDelta = Time.Precise.TargetFixedDelta;
            if (fixedDelta <= 0) return; // fixed update disabled
            Time.Precise.FixedTotal = _nextFixedTime;
            Time.Precise.FixedDelta = fixedDelta;
            Time.FixedTotal = _nextFixedTime / (float)PreciseVariables.TicksPerSecond;
            Time.FixedDelta = fixedDelta / (float)PreciseVariables.TicksPerSecond;
            Scene.FixedUpdate();
            _nextFixedTime += fixedDelta;
        }
    }
}
