using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Builds runtime grids from their declarative definitions.
/// </summary>
public interface IGridCompiler
{
    Grid CreateGrid(GridDefinition<char> definition);
}
