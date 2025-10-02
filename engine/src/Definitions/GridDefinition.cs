using System;
using System.Collections.Generic;

namespace MarkovJunior.Engine.Definitions;

/// <summary>
/// Describes the logical dimensions and alphabet used by a grid before it is
/// compiled into runtime data structures.
/// </summary>
/// <typeparam name="TSymbol">The symbol type used to represent palette entries.</typeparam>
public sealed class GridDefinition<TSymbol>
{
    public GridDefinition(int width, int height, int depth, IReadOnlyList<TSymbol> symbols, IReadOnlyDictionary<TSymbol, IReadOnlyCollection<TSymbol>>? unions = null, IReadOnlyCollection<TSymbol>? transparentSymbols = null, string? resourceFolder = null)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        Unions = unions;
        TransparentSymbols = transparentSymbols;
        ResourceFolder = resourceFolder;
    }

    public int Width { get; }

    public int Height { get; }

    public int Depth { get; }

    public IReadOnlyList<TSymbol> Symbols { get; }

    public IReadOnlyDictionary<TSymbol, IReadOnlyCollection<TSymbol>>? Unions { get; }

    public IReadOnlyCollection<TSymbol>? TransparentSymbols { get; }

    public string? ResourceFolder { get; }
}
