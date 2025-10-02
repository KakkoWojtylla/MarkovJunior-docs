using System.Collections.Generic;

namespace MarkovJunior.Engine;

/// <summary>
/// Defines the minimal surface needed to translate user facing symbols to the
/// engine's internal indices and wave masks.
/// </summary>
/// <typeparam name="TSymbol">Symbol type.</typeparam>
public interface ISymbolTable<TSymbol>
{
    int Cardinality { get; }

    IReadOnlyList<TSymbol> Symbols { get; }

    bool TryGetIndex(TSymbol symbol, out byte index);

    byte GetIndex(TSymbol symbol);

    bool TryGetMask(TSymbol symbol, out int mask);

    int GetMask(IEnumerable<TSymbol> symbols);

    int AllMask { get; }
}
