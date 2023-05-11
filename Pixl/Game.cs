using System.Diagnostics;
using System.Reflection;

namespace Pixl
{
    internal sealed class Game : Api
    {
        private static readonly List<Game> s_games = new();

        private long _startTime;
        private long _nextFixedTime;
        private readonly ManualResetEvent _waitEvent = new(false);

        public Game(IPlayer player, Assembly gameAssembly) : base(player)
        {
            if (gameAssembly is null) throw new ArgumentNullException(nameof(gameAssembly));

            GameAssembly = new GameAssembly(gameAssembly);
            Resources = new Resources(
                Material.CreateDefault(this),
                Material.CreateError(this)
            );
        }

        internal static Game Current => s_games.First(); // TODO thread matching

        public string InternalAssetsPath => Player.InternalAssetsPath;

        public GameAssembly GameAssembly { get; }
        public Graphics Graphics { get; } = new();
        public Resources Resources { get; }
        public Scene Scene { get; } = new();

        public void ForceRender() => Render();

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
            Graphics.Start(Resources, Player);
            Scene.Start(this);
            GameAssembly.Entry.OnStart(Scene);
        }

        public void Stop()
        {
            Graphics.Stop(Resources);

            Remove(this);
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

        private bool ProcessEvent(ref PlayerEvent @event)
        {
            switch (@event.Type)
            {
                case PlayerEventType.KeyDown:
                    Input.OnKeyDown((KeyCode)@event.ValueA, Time);
                    break;
                case PlayerEventType.KeyUp:
                    Input.OnKeyUp((KeyCode)@event.ValueA, Time);
                    break;
                case PlayerEventType.Quit:
                    return false;
            }
            return true;
        }

        private bool ProcessEvents()
        {
            var events = Player.DequeueEvents();
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
            Graphics.UpdateWindowSize(Player.WindowSize);

            var commands = Graphics.Commands;
            var frameBuffer = Graphics.FrameBuffer;

            Scene.Render(commands, frameBuffer);
            Graphics.Submit(commands);
            Graphics.SwapBuffers();
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
