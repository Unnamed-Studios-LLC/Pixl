using System.Runtime.InteropServices;
using Veldrid;
using WinApi.Gdi32;
using WinApi.Kernel32;
using WinApi.User32;

namespace Pixl.Player.Win
{
    internal sealed class WinPlayer : IPlayer
    {
        private readonly IntPtr _instanceHandle;
        private readonly IntPtr _hwnd;
        private readonly WindowProc _windowProc;

        private List<PlayerEvent> _events = new();
        private List<PlayerEvent> _eventsProcessing = new();

        private Int2 _clientSize;
        private WindowStyle _windowStyle;
        private string _windowTitle;

        public int ExitCode { get; set; }

        public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pixl Game");
        public string AssetsPath => "Assets";
        public string InternalAssetsPath => "InternalAssets";

        public InputState Input { get; } = new();

        public Int2 WindowSize
        {
            get => _clientSize;
            set => SetWindowSize(value);
        }
        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set => throw new NotImplementedException();
        }
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetWindowTitle(value);
        }

        public GraphicsApi GraphicsApi => GraphicsApi.DirectX;
        public SwapchainSource SwapchainSource { get; }

        public WinPlayer(Int2 windowSize)
        {
            _clientSize = windowSize;
            _windowStyle = WindowStyle.Windowed;
            _instanceHandle = Kernel32Methods.GetModuleHandle(IntPtr.Zero);
            _windowProc = ProcessWindowMessage;

            var fullWindowSize = windowSize.ToWindowedSize();
            _windowTitle = "Pixl Game";
            var wc = new WindowClassEx
            {
                Size = (uint)Marshal.SizeOf<WindowClassEx>(),
                ClassName = "MainWindow",
                CursorHandle = User32Helpers.LoadCursor(IntPtr.Zero, SystemCursor.IDC_ARROW),
                IconHandle = User32Helpers.LoadIcon(IntPtr.Zero, SystemIcon.IDI_APPLICATION),
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW,
                BackgroundBrushHandle = new IntPtr((int)StockObject.WHITE_BRUSH),
                WindowProc = _windowProc,
                InstanceHandle = _instanceHandle
            };

            var resReg = User32Methods.RegisterClassEx(ref wc);
            if (resReg == 0)
            {
                throw new Exception("Failed to register window class");
            }

            _hwnd = User32Methods.CreateWindowEx(WindowExStyles.WS_EX_APPWINDOW,
                wc.ClassName, _windowTitle, _windowStyle.GetWindowStyles(),
                (int)CreateWindowFlags.CW_USEDEFAULT, (int)CreateWindowFlags.CW_USEDEFAULT,
                fullWindowSize.X, fullWindowSize.Y,
                IntPtr.Zero, IntPtr.Zero, _instanceHandle, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
            {
                throw new Exception("Failed to create window");
            }

            SwapchainSource = SwapchainSource.CreateWin32(_hwnd, _instanceHandle);

            User32Methods.ShowWindow(_hwnd, ShowWindowCommands.SW_SHOWNORMAL);
            User32Methods.UpdateWindow(_hwnd);
        }

        public void Log(object @object)
        {
            System.Diagnostics.Debug.WriteLine(@object);
        }

        public Span<PlayerEvent> DequeueEvents()
        {
            lock (_events)
            {
                _eventsProcessing.Clear();
                (_events, _eventsProcessing) = (_eventsProcessing, _events);
            }
            return CollectionsMarshal.AsSpan(_eventsProcessing);
        }

        public void Run()
        {
            int result;
            while ((result = User32Methods.GetMessage(out var msg, IntPtr.Zero, 0, 0)) > 0)
            {
                User32Methods.TranslateMessage(ref msg);
                User32Methods.DispatchMessage(ref msg);
            }
            PushEvent(new PlayerEvent(PlayerEventType.Quit, result));
        }

        public void Stop()
        {
            User32Methods.PostQuitMessage(ExitCode);
        }

        private void OnKeyDown(int virtualKeyCode, int flags)
        {
            var isDown = ((flags >> 30) & 1) == 1;
            if (isDown) return;

            var keyCode = KeyHelper.GetKeyCode(virtualKeyCode);
            if (keyCode == KeyCode.None) return;
            PushEvent(new PlayerEvent(PlayerEventType.KeyDown, (int)keyCode));
        }

        private void OnKeyUp(int virtualKeyCode, int flags)
        {
            var keyCode = KeyHelper.GetKeyCode(virtualKeyCode);
            if (keyCode == KeyCode.None) return;
            PushEvent(new PlayerEvent(PlayerEventType.KeyUp, (int)keyCode));
        }

        private void OnSize(int action, int packedSize)
        {
            var updateSize = action switch
            {
                0 or 2 => true,
                _ => false
            };
            if (!updateSize) return;

            var width = packedSize & 0xffff;
            var height = (int)(packedSize & 0xffff0000) >> 16;
            _clientSize = new Int2(width, height);
        }

        private IntPtr ProcessWindowMessage(IntPtr hwnd, uint umsg, IntPtr wParam, IntPtr lParam)
        {
            if (_hwnd == hwnd)
            {
                switch ((WM)umsg)
                {
                    case WM.KEYDOWN:
                        OnKeyDown((int)wParam.ToInt64(), (int)lParam.ToInt64());
                        break;
                    case WM.KEYUP:
                        OnKeyUp((int)wParam.ToInt64(), (int)lParam.ToInt64());
                        break;
                    case WM.SIZE:
                        OnSize((int)wParam.ToInt64(), (int)lParam.ToInt64());
                        break;
                    case WM.CLOSE:
                        Stop();
                        break;
                }
            }
            return User32Methods.DefWindowProc(hwnd, umsg, wParam, lParam);
        }

        private void PushEvent(in PlayerEvent @event)
        {
            lock (_events)
            {
                _events.Add(@event);
            }
        }

        private void SetWindowSize(Int2 value)
        {
            if (WindowStyle != WindowStyle.Windowed) return;
            var windowSize = value.ToWindowedSize();
            if (!User32Methods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, windowSize.X, windowSize.Y, WindowPositionFlags.SWP_NOMOVE)) return;
            _clientSize = value;
        }

        private void SetWindowTitle(string value)
        {
            if (!User32Methods.SetWindowText(_hwnd, value)) return;
            _windowTitle = value;
        }
    }
}
