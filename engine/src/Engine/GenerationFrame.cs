using System;

namespace MarkovJunior.Engine;

/// <summary>
/// Represents a snapshot of the grid state produced during execution, including
/// the palette legend and the cell changes since the previous frame.
/// </summary>
public readonly struct GenerationFrame
{
    public GenerationFrame(byte[] state, char[] legend, int width, int height, int depth, int step, bool isFinal, GridChange[] changes)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        Legend = legend ?? throw new ArgumentNullException(nameof(legend));
        Width = width;
        Height = height;
        Depth = depth;
        Step = step;
        IsFinal = isFinal;
        Changes = changes ?? Array.Empty<GridChange>();
    }

    /// <summary>A copy of the grid state at the time the frame was captured.</summary>
    public byte[] State { get; }

    /// <summary>The palette legend associated with <see cref="State"/>.</summary>
    public char[] Legend { get; }

    public int Width { get; }

    public int Height { get; }

    public int Depth { get; }

    public int Step { get; }

    public bool IsFinal { get; }

    /// <summary>Cells that changed since the previous frame.</summary>
    public GridChange[] Changes { get; }
}
