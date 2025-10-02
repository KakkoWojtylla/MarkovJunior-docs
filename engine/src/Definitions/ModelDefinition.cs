using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MarkovJunior.Engine.Api;

namespace MarkovJunior.Engine.Definitions;

/// <summary>
/// Represents a fully parsed MarkovJunior model that can be compiled and executed by the engine.
/// </summary>
/// <typeparam name="TSymbol">Symbol type used by the model's grid definition.</typeparam>
public class ModelDefinition<TSymbol>
{
    public ModelDefinition(
        string name,
        GridDefinition<TSymbol> grid,
        XElement rootNode,
        ModelExecutionSettings execution,
        IReadOnlyDictionary<TSymbol, int>? paletteOverrides = null,
        string? symmetry = null,
        bool origin = false,
        IResourceStore? resources = null)
        bool origin = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        RootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
        Execution = execution ?? throw new ArgumentNullException(nameof(execution));
        PaletteOverrides = paletteOverrides;
        Symmetry = symmetry;
        Origin = origin;
        Resources = resources;
    }

    public string Name { get; }

    public GridDefinition<TSymbol> Grid { get; }

    public XElement RootNode { get; }

    public ModelExecutionSettings Execution { get; }

    public IReadOnlyDictionary<TSymbol, int>? PaletteOverrides { get; }

    public string? Symmetry { get; }

    public bool Origin { get; }

    /// <summary>
    /// Optional in-memory resource store supplying patterns, samples, tilesets and voxels without touching the filesystem.
    /// </summary>
    public IResourceStore? Resources { get; }
}

/// <summary>
/// Convenience alias for character-based models that mirror the legacy XML workflow.
/// </summary>
public sealed class ModelDefinition : ModelDefinition<char>
{
    public ModelDefinition(
        string name,
        GridDefinition<char> grid,
        XElement rootNode,
        ModelExecutionSettings execution,
        IReadOnlyDictionary<char, int>? paletteOverrides = null,
        string? symmetry = null,
        bool origin = false,
        IResourceStore? resources = null)
        : base(name, grid, rootNode, execution, paletteOverrides, symmetry, origin, resources)
        bool origin = false)
        : base(name, grid, rootNode, execution, paletteOverrides, symmetry, origin)
    {
    }
}
