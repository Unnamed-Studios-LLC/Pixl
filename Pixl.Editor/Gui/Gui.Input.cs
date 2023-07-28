using ImGuiNET;

namespace Pixl.Editor;

internal sealed partial class Gui
{
	private static ImGuiKey ConvertKey(KeyCode keyCode, out bool isMouse)
	{
		isMouse = keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6;
		if (isMouse) return (ImGuiKey)(keyCode - KeyCode.Mouse0);
		return keyCode switch
		{
			>= KeyCode.A and <= KeyCode.Z => ImGuiKey.A + (keyCode - KeyCode.A),
			>= KeyCode.Keypad0 and <= KeyCode.Keypad9 => ImGuiKey.Keypad0 + (keyCode - KeyCode.Keypad0),
			>= KeyCode.Alpha0 and <= KeyCode.Alpha9 => ImGuiKey._0 + (keyCode - KeyCode.Alpha0),
			>= KeyCode.F1 and <= KeyCode.F12 => ImGuiKey.F1 + (keyCode - KeyCode.F12),
			KeyCode.Backspace => ImGuiKey.Backspace,
			KeyCode.Escape => ImGuiKey.Escape,
			KeyCode.Delete => ImGuiKey.Delete,
			KeyCode.Tab => ImGuiKey.Tab,
			KeyCode.Enter => ImGuiKey.Enter,
			KeyCode.Space => ImGuiKey.Space,
			KeyCode.LeftShift => ImGuiKey.LeftShift,
			KeyCode.RightShift => ImGuiKey.RightShift,
            KeyCode.LeftControl => ImGuiKey.LeftCtrl,
            KeyCode.RightControl => ImGuiKey.RightCtrl,
            KeyCode.LeftAlt => ImGuiKey.LeftAlt,
            KeyCode.RightAlt => ImGuiKey.RightAlt,
            KeyCode.LeftArrow => ImGuiKey.LeftArrow,
            KeyCode.RightArrow => ImGuiKey.RightArrow,
            KeyCode.UpArrow => ImGuiKey.UpArrow,
            KeyCode.DownArrow => ImGuiKey.DownArrow,
            _ => ImGuiKey.None
		};
	}

	private static unsafe void ProcessInputEvents(Span<WindowEvent> events)
	{
		var io = ImGui.GetIO();
		foreach (ref var @event in events)
		{
			switch (@event.Type)
			{
				case WindowEventType.Character:
                    OnChar(in io, in @event);
					break;
                case WindowEventType.KeyDown:
					OnKey(in io, in @event, true);
					break;
				case WindowEventType.KeyUp:
					OnKey(in io, in @event, false);
					break;
				case WindowEventType.Scroll:
					var deltaXBits = @event.ValueA;
					var deltaYBits = @event.ValueB;
					io.AddMouseWheelEvent(*(float*)&deltaXBits, *(float*)&deltaYBits);
					break;
			}
		}
	}

    private static void OnChar(in ImGuiIOPtr io, in WindowEvent @event)
    {
		io.AddInputCharacter((uint)@event.ValueA);
    }

    private static void OnKey(in ImGuiIOPtr io, in WindowEvent @event, bool down)
	{
		var imGuiKey = ConvertKey((KeyCode)@event.ValueA, out var isMouse);
		if (isMouse)
		{
			io.AddMouseButtonEvent((int)imGuiKey, down);
			return;
		}

		if (imGuiKey == ImGuiKey.None) return;
		io.AddKeyEvent(imGuiKey, down);

		if (imGuiKey == ImGuiKey.LeftShift || 
			imGuiKey == ImGuiKey.RightShift)
        {
            io.AddKeyEvent(ImGuiKey.ModShift, down);
        }

        if (imGuiKey == ImGuiKey.LeftCtrl ||
            imGuiKey == ImGuiKey.RightCtrl)
        {
            io.AddKeyEvent(ImGuiKey.ModCtrl, down);
        }
    }
}
