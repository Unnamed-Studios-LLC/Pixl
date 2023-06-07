using System;

namespace Pixl.Editor;

internal sealed class EditorWindows
{
    private readonly MainDockWindow _mainDockWindow = new();
    private readonly EditorGameWindow _gameWindow;

    public EditorWindows(EditorGameWindow gameWindow)
    {
        _gameWindow = gameWindow;
    }

    public void SubmitUI()
    {
        _mainDockWindow.SubmitUI();
        _gameWindow.SubmitUI();
    }
}

