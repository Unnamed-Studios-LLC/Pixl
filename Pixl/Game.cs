using System.Diagnostics;
using System.Runtime.CompilerServices;
using Veldrid;

[assembly: InternalsVisibleTo("Pixl.Demo")]
[assembly: InternalsVisibleTo("Pixl.Editor")]

[assembly: InternalsVisibleTo("Pixl.Win")]
[assembly: InternalsVisibleTo("Pixl.Win.Editor")]
[assembly: InternalsVisibleTo("Pixl.Win.Player")]

namespace Pixl
{
    internal sealed class Game : Api
    {
        private static readonly List<Game> s_games = new();

        private long _startTime;
        private long _nextFixedTime;
        private readonly ManualResetEvent _waitEvent = new(false);

        public Game(Resources resources, Graphics graphics, IPlayer player, IGameEntry entry) : base(player)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        internal static Game Current => s_games.First(); // TODO thread matching

        public string InternalAssetsPath => Player.InternalAssetsPath;

        public IGameEntry Entry { get; }
        public Graphics Graphics { get; }
        public Resources Resources { get; }
        public Scene Scene { get; } = new();
        public RenderTexture? TargetRenderTexture { get; set; }

        public bool Run()
        {
            UpdateTime();
            if (!ProcessEvents()) return false;
            FixedUpdates();
            Update();
            Render();
            return true;
        }

        public void Start()
        {
            Add(this);

            _startTime = Stopwatch.GetTimestamp();
            Resources.Start();
            Scene.Start(this);
            Entry.OnStart(Scene);
        }

        public void Stop()
        {
            Remove(this);
        }

        public void SwitchGraphicsApi(GraphicsApi graphicsApi)
        {
            Graphics.Stop(Resources);
            Graphics.Start(Resources, Player.Window, graphicsApi);
        }

        public void WaitForNextUpdate()
        {
            var ticks = GetTicksUntilNextUpdate();
            if (ticks <= 0) return;
            var spinWaitMaxTicks = 2 * Stopwatch.Frequency / 1000; // 2 ms
            var startTime = Stopwatch.GetTimestamp();

            if (ticks > spinWaitMaxTicks)
            {
                // wait most of the duration
                _waitEvent.WaitOne((int)((ticks - spinWaitMaxTicks) / (Stopwatch.Frequency / 1000)));
            }

            while (Stopwatch.GetTimestamp() - startTime < ticks) { } // spin last bits
        }

        internal static void Add(Game game)
        {
            lock (s_games)
            {
                s_games.Add(game);
            }
        }

        internal static void Remove(Game game)
        {
            lock (s_games)
            {
                s_games.Remove(game);
            }
        }

        internal string GetInternalAssetPath(string localPath) => Path.Combine(Player.InternalAssetsPath, localPath);

        private long GetTicksUntilNextUpdate()
        {
            var timestamp = Stopwatch.GetTimestamp();
            var currentTime = timestamp - _startTime;
            var targetDelta = Precise.TargetUpdateDelta;
            var nextUpdateTime = Time + targetDelta;
            return nextUpdateTime - currentTime;
        }

        private void FixedUpdates()
        {
            while (_nextFixedTime <= Time)
            {
                var fixedDelta = Precise.FixedUpdateDelta;
                if (fixedDelta <= 0) return; // fixed update disabled
                Precise.FixedTotal = _nextFixedTime;
                FixedTimeTotal = _nextFixedTime / (float)PreciseTime.TicksPerSecond;
                Scene.FixedUpdate();
                _nextFixedTime += fixedDelta;
            }
        }

        private bool ProcessEvent(ref WindowEvent @event)
        {
            switch (@event.Type)
            {
                case WindowEventType.KeyDown:
                    Input.OnKeyDown((KeyCode)@event.ValueA, Time);
                    break;
                case WindowEventType.KeyUp:
                    Input.OnKeyUp((KeyCode)@event.ValueA, Time);
                    break;
                case WindowEventType.Quit:
                    return false;
            }
            return true;
        }

        private bool ProcessEvents()
        {
            var events = Player.Window.DequeueEvents();
            foreach (ref var @event in events)
            {
                if (!ProcessEvent(ref @event)) return false;
            }
            return true;
        }

        private void Render()
        {
            if (!Graphics.Setup) return;

            // sync size
            Graphics.UpdateWindowSize(Player.Window.Size);

            var commands = Graphics.Commands;
            var frameBuffer = TargetRenderTexture?.Framebuffer ?? Graphics.SwapchainFramebuffer;
            Scene.Render(Graphics, commands, frameBuffer);
        }

        private void Update() => Scene.Update();

        private void UpdateTime()
        {
            var timestamp = Stopwatch.GetTimestamp();
            var newTime = timestamp - _startTime;
            var updateDelta = newTime - Time;

            Precise.Total = newTime;
            Precise.UpdateDelta = updateDelta;
            TimeTotal = newTime / (float)PreciseTime.TicksPerSecond;
            UpdateDelta = updateDelta / (float)PreciseTime.TicksPerSecond;
            Time = newTime;
        }
    }
}
