using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    /// <summary>
    /// Converts the frame's legend to another symbol type.
    /// </summary>
    public TypedGenerationFrame<TSymbol> ToTyped<TSymbol>(Func<char, TSymbol> selector)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));

        TSymbol[] typedLegend = new TSymbol[Legend.Length];
        for (int i = 0; i < Legend.Length; i++)
        {
            typedLegend[i] = selector(Legend[i]);
        }

        return new TypedGenerationFrame<TSymbol>(State, typedLegend, Width, Height, Depth, Step, IsFinal, Changes);
    }

    /// <summary>
    /// Converts the legend using a lookup table.
    /// </summary>
    public TypedGenerationFrame<TSymbol> ToTyped<TSymbol>(IReadOnlyDictionary<char, TSymbol> legendMap)
    {
        if (legendMap is null) throw new ArgumentNullException(nameof(legendMap));

        return ToTyped(symbol => legendMap[symbol]);
    }

    /// <summary>
    /// Converts the frame into a collection of strings representing each row.
    /// Only valid for 2D grids.
    /// </summary>
    public IReadOnlyList<string> AsStrings()
    {
        if (Depth != 1)
        {
            throw new InvalidOperationException("AsStrings is only supported for 2D grids.");
        }

        var rows = new string[Height];
        for (int y = 0; y < Height; y++)
        {
            var row = new char[Width];
            for (int x = 0; x < Width; x++)
            {
                int index = x + y * Width;
                byte paletteIndex = State[index];
                row[x] = Legend[paletteIndex];
            }

            rows[y] = new string(row);
        }

        return Array.AsReadOnly(rows);
    }
}
