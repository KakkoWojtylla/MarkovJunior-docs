using System;
using System.Collections.Generic;

namespace MarkovJunior.Engine;

/// <summary>
/// Generic implementation of <see cref="ISymbolTable{TSymbol}"/> that can map any
/// comparable symbol type to palette indices and wave masks.
/// </summary>
public class GenericSymbolTable<TSymbol> : ISymbolTable<TSymbol>
{
    protected readonly Dictionary<TSymbol, byte> IndicesCore;
    protected readonly Dictionary<TSymbol, int> WavesCore;
    protected readonly List<TSymbol> SymbolsCore;
    private readonly IEqualityComparer<TSymbol> _comparer;
    private int _transparentMask;

    public GenericSymbolTable(IEnumerable<TSymbol> symbols, IEqualityComparer<TSymbol>? comparer = null)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));

        _comparer = comparer ?? EqualityComparer<TSymbol>.Default;
        IndicesCore = new Dictionary<TSymbol, byte>(_comparer);
        WavesCore = new Dictionary<TSymbol, int>(_comparer);
        SymbolsCore = new List<TSymbol>();

        byte index = 0;
        foreach (TSymbol symbol in symbols)
        {
            if (IndicesCore.ContainsKey(symbol))
            {
                throw new ArgumentException("Duplicate symbol detected in palette.", nameof(symbols));
            }

            IndicesCore.Add(symbol, index);
            SymbolsCore.Add(symbol);
            WavesCore.Add(symbol, 1 << index);
            index++;
        }

        AllMask = (1 << SymbolsCore.Count) - 1;
    }

    public IReadOnlyList<TSymbol> Symbols => SymbolsCore;

    public int Cardinality => SymbolsCore.Count;

    public int AllMask { get; protected set; }

    public IReadOnlyDictionary<TSymbol, byte> Indices => IndicesCore;

    public IReadOnlyDictionary<TSymbol, int> Waves => WavesCore;

    public int TransparentMask => _transparentMask;

    public virtual void DefineUnion(TSymbol symbol, IEnumerable<TSymbol> members)
    {
        if (WavesCore.ContainsKey(symbol))
        {
            throw new ArgumentException("Symbol already defined.", nameof(symbol));
        }

        int mask = GetMask(members ?? throw new ArgumentNullException(nameof(members)));
        WavesCore.Add(symbol, mask);
    }

    public virtual void DefineTransparent(IEnumerable<TSymbol> symbols)
    {
        _transparentMask = GetMask(symbols ?? throw new ArgumentNullException(nameof(symbols)));
    }

    public bool TryGetIndex(TSymbol symbol, out byte index) => IndicesCore.TryGetValue(symbol, out index);

    public byte GetIndex(TSymbol symbol) => IndicesCore[symbol];

    public bool TryGetMask(TSymbol symbol, out int mask) => WavesCore.TryGetValue(symbol, out mask);

    public int GetMask(IEnumerable<TSymbol> symbols)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));

        int mask = 0;
        foreach (TSymbol symbol in symbols)
        {
            mask |= WavesCore[symbol];
        }

        return mask;
    }
}
