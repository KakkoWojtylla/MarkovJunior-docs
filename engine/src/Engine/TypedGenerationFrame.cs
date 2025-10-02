using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    /// <summary>
    /// Converts the frame into row arrays. Only valid for 2D grids.
    /// </summary>
    public IReadOnlyList<TSymbol[]> AsRows()
    {
        if (Depth != 1)
        {
            throw new InvalidOperationException("AsRows is only supported for 2D grids.");
        }

        var rows = new TSymbol[Height][];
        for (int y = 0; y < Height; y++)
        {
            var row = new TSymbol[Width];
            for (int x = 0; x < Width; x++)
            {
                int index = x + y * Width;
                byte paletteIndex = State[index];
                row[x] = Legend[paletteIndex];
            }

            rows[y] = row;
        }

        return Array.AsReadOnly(rows);
    }
}
