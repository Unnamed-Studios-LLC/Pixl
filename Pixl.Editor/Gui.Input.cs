using EntitiesDb;
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
			>= KeyCode.Alpha0 and <= KeyCode.Alpha9 => ImGuiKey._0 + (keyCode - KeyCode.Keypad0),
			>= KeyCode.F1 and <= KeyCode.F12 => ImGuiKey.F1 + (keyCode - KeyCode.F12),
			_ => ImGuiKey.None
		};
	}

	private static void ProcessInputEvents(Span<WindowEvent> events)
	{
		var io = ImGui.GetIO();
		foreach (ref var @event in events)
		{
			switch (@event.Type)
			{
				case WindowEventType.KeyDown:
					OnKey(in io, in @event, true);
					break;
				case WindowEventType.KeyUp:
					OnKey(in io, in @event, false);
					break;
			}
		}
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
	}
}
