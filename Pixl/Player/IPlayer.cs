﻿namespace Pixl;

internal interface IPlayer
{
    int ExitCode { get; set; }

    /// <summary>
    /// The path pointing to a directory where application data may be read and written.
    /// </summary>
    string DataPath { get; }

    /// <summary>
    /// The path pointing to application assets. May be read-only on certain platforms. Use <see cref="Application.DataPath"/> to save game data.
    /// </summary>
    string AssetsPath { get; }

    /// <summary>
    /// The window the player is rendering to
    /// </summary>
    AppWindow Window { get; }

    /// <summary>
    /// Current logging implementation
    /// </summary>
    ILogger Logger { get; }
}
