using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Veldrid;
using WinApi.Gdi32;
using WinApi.Kernel32;
using WinApi.User32;

[assembly: InternalsVisibleTo("Pixl.Win.Editor")]
[assembly: InternalsVisibleTo("Pixl.Win.Player")]

namespace Pixl.Win;

internal class WinWindow : Window, IWin32Window
{
    private const int TimerId = 1;

    private readonly WindowProc _windowProc;
    private readonly ManualResetEvent _windowCreated = new(false);
    private IntPtr _instanceHandle;
    private IntPtr _hwnd;
    private SwapchainSource? _swapchainSource;

    private Thread? _windowThread;
    private Int2 _clientSize;
    private WindowStyle _windowStyle;
    private string _windowTitle;
    private CursorState _cursorState = CursorState.None;

    private readonly Dictionary<CursorState, nint> _systemCursors = new();

    public WinWindow(string title, Int2 size)
    {
        _clientSize = size;
        _windowStyle = WindowStyle.Windowed;
        _windowTitle = title;
        _windowProc = ProcessWindowMessage;
    }

    public int ExitCode { get; set; }
    public bool Timer { get; set; } = false;

    public override Int2 MousePosition => GetMousePosition();

    public override Int2 Size
    {
        get => _clientSize;
        set => throw new NotImplementedException();
    }
    public override WindowStyle Style
    {
        get => _windowStyle;
        set => throw new NotImplementedException();
    }
    public override SwapchainSource SwapchainSource => _swapchainSource ?? throw new Exception("Window not created!");
    public override string Title
    {
        get => _windowTitle;
        set => SetWindowTitle(value);
    }
    public override CursorState CursorState
    {
        get => _cursorState;
        set => SetCursorState(value);
    }

    public nint Handle => _hwnd;

    public override void PushEvent(in WindowEvent @event)
    {
        switch (@event.Type)
        {
            case WindowEventType.MouseMove:
                UpdateCursor();
                break;
        }

        base.PushEvent(@event);
    }

    public override void Start()
    {
        _windowThread = new Thread(Run);
        _windowThread.Start();
        _windowCreated.WaitOne();
    }

    public override void Stop()
    {
        User32Methods.PostQuitMessage(ExitCode);
        _windowThread?.Join();
    }

    private void CreateWindow()
    {
        _instanceHandle = Kernel32Methods.GetModuleHandle(IntPtr.Zero);
        var fullWindowSize = _clientSize.ToWindowedSize();
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

        _swapchainSource = SwapchainSource.CreateWin32(_hwnd, _instanceHandle);

        User32Methods.ShowWindow(_hwnd, ShowWindowCommands.SW_SHOWNORMAL);
        User32Methods.UpdateWindow(_hwnd);

        LoadSystemCursor(CursorState.None, SystemCursor.IDC_ARROW);
        LoadSystemCursor(CursorState.Hand, SystemCursor.IDC_HAND);
        LoadSystemCursor(CursorState.TextInput, SystemCursor.IDC_IBEAM);
        LoadSystemCursor(CursorState.Resize, SystemCursor.IDC_SIZEALL);
        LoadSystemCursor(CursorState.ResizeHorizontal, SystemCursor.IDC_SIZEWE);
        LoadSystemCursor(CursorState.ResizeVertical, SystemCursor.IDC_SIZENS);
        _windowCreated.Set();
    }

    private Int2 GetMousePosition()
    {
        if (!User32Methods.GetCursorPos(out var point) ||
            !User32Methods.ScreenToClient(_hwnd, ref point)) return Int2.Zero;
        point.Y = _clientSize.Y - point.Y - 1;
        return new Int2(point.X, point.Y);
    }

    private void LoadSystemCursor(CursorState cursorState, SystemCursor systemCursor)
    {
        var cursorId = User32Methods.LoadCursor(default, (nint)systemCursor);
        if (cursorId == 0) return;
        _systemCursors[cursorState] = cursorId;
    }

    private void OnChar(int character)
    {
        PushEvent(new WindowEvent(WindowEventType.Character, character));
    }

    private void OnKeyDown(int virtualKeyCode, int flags)
    {
        var isDown = ((flags >> 30) & 1) == 1;
        if (isDown) return;
        var isExtended = ((flags >> 24) & 1) == 1;

        var keyCode = KeyHelper.GetKeyCode(virtualKeyCode, isExtended);
        if (keyCode == KeyCode.None) return;
        PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)keyCode));
    }

    private void OnKeyUp(int virtualKeyCode, int flags)
    {
        var isExtended = ((flags >> 24) & 1) == 1;
        var keyCode = KeyHelper.GetKeyCode(virtualKeyCode, isExtended);
        if (keyCode == KeyCode.None) return;
        PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)keyCode));
    }

    private void OnMouseDown(long wParam, long lParam, int buttonIndex)
    {
        if (buttonIndex > 2)
        {
            if (lParam <= 0 || lParam > 2) return; // unknown
            buttonIndex += (int)lParam - 1;
        }

        var keyCode = KeyHelper.GetKeyCodeForMouseIndex(buttonIndex);
        if (keyCode == KeyCode.None) return;
        PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)keyCode));
    }

    private void OnMouseMove()
    {
        PushEvent(new WindowEvent(WindowEventType.MouseMove));
    }

    private void OnMouseUp(long wParam, long lParam, int buttonIndex)
    {
        if (buttonIndex > 2)
        {
            if (lParam <= 0 || lParam > 2) return; // unknown
            buttonIndex += (int)lParam - 1;
        }

        var keyCode = KeyHelper.GetKeyCodeForMouseIndex(buttonIndex);
        if (keyCode == KeyCode.None) return;
        PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)keyCode));
    }

    private unsafe void OnMouseWheel(long wParam, long lParam)
    {
        var delta = (int)(wParam & 0xffff0000) >> 16;
        if (delta == 0) return;
        var deltaF = delta / 120f;
        var deltaFBits = (int*)&deltaF;
        PushEvent(new WindowEvent(WindowEventType.Scroll, 0, *deltaFBits));
    }

    private void OnPaint()
    {
        PushEvent(new WindowEvent(WindowEventType.Render));
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
            var wmg = (WM)umsg;
            switch (wmg)
            {
                case WM.CLOSE:
                    User32Methods.PostQuitMessage(ExitCode);
                    break;
                case WM.CHAR:
                    OnChar((int)wParam.ToInt64());
                    break;
                case WM.KEYDOWN:
                    OnKeyDown((int)wParam.ToInt64(), (int)lParam.ToInt64());
                    break;
                case WM.KEYUP:
                    OnKeyUp((int)wParam.ToInt64(), (int)lParam.ToInt64());
                    break;
                case WM.PAINT:
                    OnPaint();
                    break;
                case WM.SIZE:
                    OnSize((int)wParam.ToInt64(), (int)lParam.ToInt64());
                    break;
                case WM.LBUTTONDOWN:
                    OnMouseDown(wParam.ToInt64(), lParam.ToInt64(), 0);
                    break;
                case WM.LBUTTONUP:
                    OnMouseUp(wParam.ToInt64(), lParam.ToInt64(), 0);
                    break;
                case WM.RBUTTONDOWN:
                    OnMouseDown(wParam.ToInt64(), lParam.ToInt64(), 1);
                    break;
                case WM.RBUTTONUP:
                    OnMouseUp(wParam.ToInt64(), lParam.ToInt64(), 1);
                    break;
                case WM.MBUTTONDOWN:
                    OnMouseDown(wParam.ToInt64(), lParam.ToInt64(), 2);
                    break;
                case WM.MBUTTONUP:
                    OnMouseUp(wParam.ToInt64(), lParam.ToInt64(), 2);
                    break;
                case WM.XBUTTONDOWN:
                    OnMouseDown(wParam.ToInt64(), lParam.ToInt64(), 3);
                    break;
                case WM.XBUTTONUP:
                    OnMouseUp(wParam.ToInt64(), lParam.ToInt64(), 3);
                    break;
                case WM.MOUSEMOVE:
                    OnMouseMove();
                    break;
                case WM.MOUSEWHEEL:
                    OnMouseWheel(wParam.ToInt64(), lParam.ToInt64());
                    break;
            }
        }
        return User32Methods.DefWindowProc(hwnd, umsg, wParam, lParam);
    }

    private void Run()
    {
        CreateWindow();
        OnPaint();
        int result;
        while ((result = User32Methods.GetMessage(out var msg, IntPtr.Zero, 0, 0)) > 0)
        {
            User32Methods.TranslateMessage(ref msg);
            User32Methods.DispatchMessage(ref msg);
        }
        PushEvent(new WindowEvent(WindowEventType.Quit, result));
    }

    private void SetCursorState(CursorState value)
    {
        if (_cursorState == value) return;
        _cursorState = value;
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (!_systemCursors.TryGetValue(_cursorState, out var cursorId) &&
            !_systemCursors.TryGetValue(CursorState.None, out cursorId)) return;
        User32Methods.SetCursor(cursorId);
    }

    private void SetWindowTitle(string value)
    {
        if (!User32Methods.SetWindowText(_hwnd, value)) return;
        _windowTitle = value;
    }
}
