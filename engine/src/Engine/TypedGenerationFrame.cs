using System;

namespace MarkovJunior.Engine;

/// <summary>
/// Represents a snapshot of the grid state with a legend projected to an arbitrary
/// symbol domain.
/// </summary>
/// <typeparam name="TSymbol">Legend symbol type.</typeparam>
public readonly struct TypedGenerationFrame<TSymbol>
{
    public TypedGenerationFrame(byte[] state, TSymbol[] legend, int width, int height, int depth, int step, bool isFinal, GridChange[] changes)
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

    public byte[] State { get; }

    public TSymbol[] Legend { get; }

    public int Width { get; }

    public int Height { get; }

    public int Depth { get; }

    public int Step { get; }

    public bool IsFinal { get; }

    public GridChange[] Changes { get; }
}
