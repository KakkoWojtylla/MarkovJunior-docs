using System;
using System.Collections.Generic;

namespace MarkovJunior.Engine.Definitions;

/// <summary>
/// Captures execution parameters sourced from <c>models.xml</c>.
/// </summary>
public sealed class ModelExecutionSettings
{
    public ModelExecutionSettings(int runs, int? steps, bool emitGif, bool isometric, int pixelSize, int guiScale, IReadOnlyList<int>? seeds)
    {
        if (runs <= 0) throw new ArgumentOutOfRangeException(nameof(runs));
        if (pixelSize <= 0) throw new ArgumentOutOfRangeException(nameof(pixelSize));
        if (guiScale < 0) throw new ArgumentOutOfRangeException(nameof(guiScale));

        Runs = runs;
        Steps = steps;
        EmitGif = emitGif;
        Isometric = isometric;
        PixelSize = pixelSize;
        GuiScale = guiScale;
        Seeds = seeds;
    }

    public int Runs { get; }

    public int? Steps { get; }

    public bool EmitGif { get; }

    public bool Isometric { get; }

    public int PixelSize { get; }

    public int GuiScale { get; }

    public IReadOnlyList<int>? Seeds { get; }
}
