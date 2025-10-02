using System;

namespace MarkovJunior.Engine;

/// <summary>
/// Represents a compiled runtime grid together with the palette used to translate
/// between user facing symbols and engine indices.
/// </summary>
/// <typeparam name="TSymbol">Symbol type.</typeparam>
public sealed class CompiledGrid<TSymbol>
{
    public CompiledGrid(Grid runtime, ISymbolTable<TSymbol> palette)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
    }

    /// <summary>The runtime grid consumed by the interpreter.</summary>
    public Grid Runtime { get; }

    /// <summary>Palette describing how runtime indices map back to user symbols.</summary>
    public ISymbolTable<TSymbol> Palette { get; }
}
