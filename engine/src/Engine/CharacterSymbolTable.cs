using System;
using System.Collections.Generic;

namespace MarkovJunior.Engine;

/// <summary>
/// Default implementation of <see cref="ISymbolTable{Char}"/> that mirrors the
/// legacy character based palette behaviour.
/// </summary>
public sealed class CharacterSymbolTable : ISymbolTable<char>
{
    private readonly Dictionary<char, byte> _indices;
    private readonly Dictionary<char, int> _waves;
    private readonly List<char> _symbols;
    private int _transparentMask;

    public CharacterSymbolTable(IEnumerable<char> symbols)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));

        _indices = new Dictionary<char, byte>();
        _waves = new Dictionary<char, int>();
        _symbols = new List<char>();

        byte index = 0;
        foreach (char symbol in symbols)
        {
            if (_indices.ContainsKey(symbol))
            {
                throw new ArgumentException($"Duplicate symbol '{symbol}' detected in palette.", nameof(symbols));
            }

            _indices.Add(symbol, index);
            _symbols.Add(symbol);
            _waves.Add(symbol, 1 << index);
            index++;
        }

        AllMask = (1 << _symbols.Count) - 1;
        _waves['*'] = AllMask;
    }

    public IReadOnlyList<char> Symbols => _symbols;

    public int Cardinality => _symbols.Count;

    public int AllMask { get; private set; }

    public IReadOnlyDictionary<char, byte> Indices => _indices;

    public IReadOnlyDictionary<char, int> Waves => _waves;

    public void DefineUnion(char symbol, IEnumerable<char> members)
    {
        if (_waves.ContainsKey(symbol))
        {
            throw new ArgumentException($"Symbol '{symbol}' already defined.", nameof(symbol));
        }

        int mask = GetMask(members ?? throw new ArgumentNullException(nameof(members)));
        _waves.Add(symbol, mask);
    }

    public void DefineTransparent(IEnumerable<char> symbols)
    {
        _transparentMask = GetMask(symbols ?? throw new ArgumentNullException(nameof(symbols)));
    }

    public int TransparentMask => _transparentMask;

    public bool TryGetIndex(char symbol, out byte index) => _indices.TryGetValue(symbol, out index);

    public byte GetIndex(char symbol) => _indices[symbol];

    public bool TryGetMask(char symbol, out int mask) => _waves.TryGetValue(symbol, out mask);

    public int GetMask(IEnumerable<char> symbols)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));

        int mask = 0;
        foreach (char symbol in symbols)
        {
            mask |= _waves[symbol];
        }

        return mask;
    }
}
