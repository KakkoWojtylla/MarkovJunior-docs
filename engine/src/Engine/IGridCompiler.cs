using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Builds runtime grids from their declarative definitions.
/// </summary>
/// <typeparam name="TSymbol">Symbol type used by the grid definition.</typeparam>
public interface IGridCompiler<TSymbol>
{
    CompiledGrid<TSymbol> CreateGrid(GridDefinition<TSymbol> definition);
}
