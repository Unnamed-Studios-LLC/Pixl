using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using WinApi.Gdi32;
using WinApi.Kernel32;
using WinApi.User32;

[assembly: InternalsVisibleTo("Pixl.Win.Editor")]
[assembly: InternalsVisibleTo("Pixl.Win.Player")]

namespace Pixl.Win;

internal class WinWindow : AppWindow
{
    private readonly IntPtr _instanceHandle;
    private readonly IntPtr _hwnd;
    private readonly WindowProc _windowProc;

    private Int2 _clientSize;
    private WindowStyle _windowStyle;
    private string _windowTitle;

    public WinWindow(string title, Int2 size)
    {
        _clientSize = size;
        _windowStyle = WindowStyle.Windowed;
        _instanceHandle = Kernel32Methods.GetModuleHandle(IntPtr.Zero);
        _windowProc = ProcessWindowMessage;
        _windowTitle = title;

        var fullWindowSize = size.ToWindowedSize();
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

    public int ExitCode { get; set; }

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
    public override SwapchainSource SwapchainSource { get; }
    public override string Title
    {
        get => _windowTitle;
        set => SetWindowTitle(value);
    }

    public void Run()
    {
        OnPaint();

        int result;
        while ((result = User32Methods.GetMessage(out var msg, IntPtr.Zero, 0, 0)) > 0)
        {
            User32Methods.TranslateMessage(ref msg);
            User32Methods.DispatchMessage(ref msg);
        }
        PushEvent(new WindowEvent(WindowEventType.Quit, result));
    }

    public void Stop()
    {
        User32Methods.PostQuitMessage(ExitCode);
    }

    private Int2 GetMousePosition()
    {
        if (!User32Methods.GetCursorPos(out var point) ||
            !User32Methods.ScreenToClient(_hwnd, ref point)) return Int2.Zero;
        point.Y = _clientSize.Y - point.Y - 1;
        return new Int2(point.X, point.Y);
    }

    private void OnPaint()
    {
        PushEvent(new WindowEvent(WindowEventType.Render));
    }

    private void OnKeyDown(int virtualKeyCode, int flags)
    {
        var isDown = ((flags >> 30) & 1) == 1;
        if (isDown) return;

        var keyCode = KeyHelper.GetKeyCode(virtualKeyCode);
        if (keyCode == KeyCode.None) return;
        PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)keyCode));
    }

    private void OnKeyUp(int virtualKeyCode, int flags)
    {
        var keyCode = KeyHelper.GetKeyCode(virtualKeyCode);
        if (keyCode == KeyCode.None) return;
        PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)keyCode));
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
                case WM.CLOSE:
                    Stop();
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
            }
        }
        return User32Methods.DefWindowProc(hwnd, umsg, wParam, lParam);
    }

    private void SetWindowTitle(string value)
    {
        if (!User32Methods.SetWindowText(_hwnd, value)) return;
        _windowTitle = value;
    }
}
