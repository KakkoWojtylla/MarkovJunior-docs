using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Fluent builder for character-based MarkovJunior models that avoids XML files.
/// </summary>
public sealed class ModelBuilder
{
    private readonly List<char> _symbols = new();
    private readonly HashSet<char> _symbolLookup = new();
    private readonly Dictionary<char, HashSet<char>> _unions = new();
    private readonly HashSet<char> _transparent = new();
    private readonly Dictionary<char, int> _paletteOverrides = new();
    private readonly ModelExecutionSettingsBuilder _executionBuilder = new();
    private readonly ResourceStoreBuilder _resourceBuilder = new();

    private string _name = "runtime";
    private int _width;
    private int _height;
    private int _depth = 1;
    private string? _resourceFolder;
    private string? _symmetry;
    private bool _origin;
    private NodeBuilder? _root;

    public ModelBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Model name cannot be null or whitespace.", nameof(name));
        _name = name;
        return this;
    }

    public ModelBuilder WithSize(int width, int height, int depth = 1)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));

        _width = width;
        _height = height;
        _depth = depth;
        return this;
    }

    public ModelBuilder WithAlphabet(params char[] symbols)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));
        foreach (char symbol in symbols)
        {
            AddSymbol(symbol);
        }

        return this;
    }

    public ModelBuilder WithAlphabet(string symbols)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));
        foreach (char symbol in symbols)
        {
            AddSymbol(symbol);
        }

        return this;
    }

    public ModelBuilder WithResourceFolder(string? folder)
    {
        _resourceFolder = folder;
        return this;
    }

    public ModelBuilder AddRulePattern(string name, params string[] rows)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        (char[] data, int width, int height) = Flatten2D(rows);
        _resourceBuilder.AddPattern(name, data, width, height);
        return this;
    }

    public ModelBuilder AddRulePattern(string name, IEnumerable<IReadOnlyList<string>> layers)
    {
        if (layers is null) throw new ArgumentNullException(nameof(layers));
        (char[] data, int width, int height, int depth) = Flatten3D(layers);
        _resourceBuilder.AddPattern(name, data, width, height, depth);
        return this;
    }

    public ModelBuilder AddSample(string name, params string[] rows)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        (char[] data, int width, int height) = Flatten2D(rows);
        _resourceBuilder.AddSample(name, data, width, height);
        return this;
    }

    public ModelBuilder AddConvChainSample(string name, IEnumerable<string> rows, Func<char, bool>? selector = null)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        selector ??= DefaultConvChainSelector;
        (bool[] data, int width, int height) = FlattenBool(rows, selector);
        _resourceBuilder.AddConvChainSample(name, data, width, height);
        return this;
    }

    public ModelBuilder AddXmlResource(string name, string xml)
    {
        if (xml is null) throw new ArgumentNullException(nameof(xml));
        _resourceBuilder.AddXml(name, XDocument.Parse(xml));
        return this;
    }

    public ModelBuilder AddXmlResource(string name, XDocument document)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));
        _resourceBuilder.AddXml(name, document);
        return this;
    }

    public ModelBuilder AddVoxResource(string name, IEnumerable<IReadOnlyList<string>> layers, Func<char, int> palette)
    {
        if (layers is null) throw new ArgumentNullException(nameof(layers));
        if (palette is null) throw new ArgumentNullException(nameof(palette));
        (char[] chars, int width, int height, int depth) = Flatten3D(layers);
        int[] data = new int[chars.Length];
        for (int i = 0; i < chars.Length; i++) data[i] = palette(chars[i]);
        _resourceBuilder.AddVox(name, data, width, height, depth);
        return this;
    }

    public ModelBuilder AddUnion(char name, params char[] members)
    {
        if (!_symbolLookup.Contains(name))
        {
            throw new InvalidOperationException($"Symbol '{name}' must be added to the alphabet before defining unions.");
        }

        if (!_unions.TryGetValue(name, out HashSet<char>? union))
        {
            union = new HashSet<char>();
            _unions[name] = union;
        }

        foreach (char member in members ?? Array.Empty<char>())
        {
            if (!_symbolLookup.Contains(member))
            {
                throw new InvalidOperationException($"Union member '{member}' must be part of the alphabet.");
            }

            union.Add(member);
        }

        return this;
    }

    public ModelBuilder AddTransparent(params char[] symbols)
    {
        if (symbols is null) throw new ArgumentNullException(nameof(symbols));
        foreach (char symbol in symbols)
        {
            if (!_symbolLookup.Contains(symbol))
            {
                throw new InvalidOperationException($"Transparent symbol '{symbol}' must be part of the alphabet.");
            }

            _transparent.Add(symbol);
        }

        return this;
    }

    public ModelBuilder OverrideColor(char symbol, Color color)
    {
        if (!_symbolLookup.Contains(symbol))
        {
            throw new InvalidOperationException($"Symbol '{symbol}' must be part of the alphabet before overriding colors.");
        }

        _paletteOverrides[symbol] = color.ToArgb();
        return this;
    }

    public ModelBuilder OverrideColor(char symbol, int argb)
    {
        if (!_symbolLookup.Contains(symbol))
        {
            throw new InvalidOperationException($"Symbol '{symbol}' must be part of the alphabet before overriding colors.");
        }

        _paletteOverrides[symbol] = argb;
        return this;
    }

    public ModelBuilder WithSymmetry(string? symmetry)
    {
        _symmetry = symmetry;
        return this;
    }

    public ModelBuilder WithOrigin(bool origin = true)
    {
        _origin = origin;
        return this;
    }

    public ModelBuilder WithRoot(NodeBuilder root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        return this;
    }

    public ModelBuilder ConfigureExecution(Action<ModelExecutionSettingsBuilder> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));
        configure(_executionBuilder);
        return this;
    }

    public ModelDefinition Build()
    {
        if (_width <= 0 || _height <= 0 || _depth <= 0)
        {
            throw new InvalidOperationException("Grid dimensions must be configured using WithSize().");
        }

        if (_symbols.Count == 0)
        {
            throw new InvalidOperationException("At least one symbol must be added using WithAlphabet().");
        }

        if (_root is null)
        {
            throw new InvalidOperationException("A root node must be supplied using WithRoot().");
        }

        var unions = _unions.Count == 0
            ? null
            : _unions.ToDictionary(pair => pair.Key, pair => (IReadOnlyCollection<char>)new ReadOnlyCollection<char>(pair.Value.ToList()));

        IReadOnlyCollection<char>? transparent = _transparent.Count == 0
            ? null
            : new ReadOnlyCollection<char>(_transparent.ToList());

        var grid = new GridDefinition<char>(_width, _height, _depth, new ReadOnlyCollection<char>(_symbols), unions, transparent, _resourceFolder);
        var execution = _executionBuilder.Build();
        var paletteOverrides = _paletteOverrides.Count == 0 ? null : new ReadOnlyDictionary<char, int>(_paletteOverrides);
        XElement rootElement = _root.ToXElement();

        IResourceStore? resources = _resourceBuilder.HasResources ? _resourceBuilder.Build() : null;
        return new ModelDefinition(_name, grid, rootElement, execution, paletteOverrides, _symmetry, _origin, resources);
    }

    private void AddSymbol(char symbol)
    {
        if (_symbolLookup.Add(symbol))
        {
            _symbols.Add(symbol);
        }
    }

    private static (char[] data, int width, int height) Flatten2D(IEnumerable<string> rows)
    {
        List<string> list = rows.Select(r => r ?? throw new ArgumentNullException(nameof(rows), "Row value cannot be null.")).ToList();
        if (list.Count == 0) throw new InvalidOperationException("At least one row is required.");
        int width = list[0].Length;
        if (width == 0) throw new InvalidOperationException("Rows must not be empty.");
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].Length != width)
            {
                throw new InvalidOperationException("All rows must have the same length.");
            }
        }

        char[] data = new char[width * list.Count];
        for (int y = 0; y < list.Count; y++)
        {
            string row = list[y];
            for (int x = 0; x < width; x++)
            {
                data[x + y * width] = row[x];
            }
        }

        return (data, width, list.Count);
    }

    private static (bool[] data, int width, int height) FlattenBool(IEnumerable<string> rows, Func<char, bool> selector)
    {
        (char[] chars, int width, int height) = Flatten2D(rows);
        bool[] data = new bool[chars.Length];
        for (int i = 0; i < chars.Length; i++) data[i] = selector(chars[i]);
        return (data, width, height);
    }

    private static (char[] data, int width, int height, int depth) Flatten3D(IEnumerable<IReadOnlyList<string>> layers)
    {
        List<IReadOnlyList<string>> list = layers.Select(layer => layer ?? throw new ArgumentNullException(nameof(layers), "Layer cannot be null.")).ToList();
        if (list.Count == 0) throw new InvalidOperationException("At least one layer is required.");
        int depth = list.Count;
        int height = list[0].Count;
        if (height == 0) throw new InvalidOperationException("Each layer must contain at least one row.");
        int width = list[0][0].Length;
        if (width == 0) throw new InvalidOperationException("Rows must not be empty.");

        for (int z = 0; z < depth; z++)
        {
            IReadOnlyList<string> layer = list[z];
            if (layer.Count != height)
            {
                throw new InvalidOperationException("All layers must have the same number of rows.");
            }
            for (int y = 0; y < height; y++)
            {
                string row = layer[y];
                if (row.Length != width)
                {
                    throw new InvalidOperationException("All rows must have the same length.");
                }
            }
        }

        char[] data = new char[width * height * depth];
        for (int z = 0; z < depth; z++)
        {
            IReadOnlyList<string> layer = list[depth - 1 - z];
            for (int y = 0; y < height; y++)
            {
                string row = layer[y];
                for (int x = 0; x < width; x++)
                {
                    data[x + y * width + z * width * height] = row[x];
                }
            }
        }

        return (data, width, height, depth);
    }

    private static bool DefaultConvChainSelector(char c) => c != '0' && c != '.' && !char.IsWhiteSpace(c);
}
