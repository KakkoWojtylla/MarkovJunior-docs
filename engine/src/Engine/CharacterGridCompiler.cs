using System;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Compiles <see cref="GridDefinition{Char}"/> instances into runtime grids
/// backed by a <see cref="CharacterSymbolTable"/>.
/// </summary>
public sealed class CharacterGridCompiler : IGridCompiler<char>
{
    public CompiledGrid<char> CreateGrid(GridDefinition<char> definition)
    {
        if (definition is null) throw new ArgumentNullException(nameof(definition));

        CharacterSymbolTable palette = new CharacterSymbolTable(definition.Symbols);
        if (definition.Unions != null)
        {
            foreach (var union in definition.Unions)
            {
                palette.DefineUnion(union.Key, union.Value);
            }
        }

        if (definition.TransparentSymbols != null)
        {
            palette.DefineTransparent(definition.TransparentSymbols);
        }

        Grid grid = new Grid(definition.Width, definition.Height, definition.Depth, palette, definition.ResourceFolder, null);
        Grid grid = new Grid(definition.Width, definition.Height, definition.Depth, palette, definition.ResourceFolder);
        return new CompiledGrid<char>(grid, palette);
    }
}
