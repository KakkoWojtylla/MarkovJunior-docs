using System;
using System.Collections.Generic;

namespace MarkovJunior.Engine;

/// <summary>
/// Default implementation of <see cref="ISymbolTable{Char}"/> that mirrors the
/// legacy character based palette behaviour.
/// </summary>
public sealed class CharacterSymbolTable : GenericSymbolTable<char>
{
    public CharacterSymbolTable(IEnumerable<char> symbols) : base(symbols)
    {
        WavesCore['*'] = AllMask;
    }

    public override void DefineUnion(char symbol, IEnumerable<char> members)
    {
        if (symbol == '*')
        {
            throw new ArgumentException("'*' is reserved for wildcard unions.", nameof(symbol));
        }

        base.DefineUnion(symbol, members);
    }
}
