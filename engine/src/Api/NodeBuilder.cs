using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Provides a fluent builder for MarkovJunior AST nodes without requiring XML files.
/// The resulting <see cref="XElement"/> mirrors the schema consumed by the legacy loader.
/// </summary>
public sealed class NodeBuilder
{
    private readonly string _name;
    private readonly Dictionary<string, string> _attributes;
    private readonly List<Func<XNode>> _children;

    public NodeBuilder(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Node name cannot be null or whitespace.", nameof(name));
        }

        _name = name;
        _attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _children = new List<Func<XNode>>();
    }

    /// <summary>
    /// Adds or replaces an attribute on the node.
    /// </summary>
    public NodeBuilder Attribute(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));
        }

        if (value is null)
        {
            _attributes.Remove(name);
            return this;
        }

        _attributes[name] = FormatValue(value);
        return this;
    }

    /// <summary>
    /// Adds a child node.
    /// </summary>
    public NodeBuilder Child(NodeBuilder child)
    {
        if (child is null) throw new ArgumentNullException(nameof(child));

        _children.Add(() => child.ToXElement());
        return this;
    }

    /// <summary>
    /// Adds multiple child nodes in the provided order.
    /// </summary>
    public NodeBuilder Children(params NodeBuilder[] children)
    {
        if (children is null) throw new ArgumentNullException(nameof(children));
        foreach (NodeBuilder child in children)
        {
            Child(child);
        }

        return this;
    }

    /// <summary>
    /// Adds a nested element configured by the supplied action.
    /// </summary>
    public NodeBuilder Element(string name, Action<NodeBuilder>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Element name cannot be null or whitespace.", nameof(name));
        }

        var child = new NodeBuilder(name);
        configure?.Invoke(child);
        _children.Add(() => child.ToXElement());
        return this;
    }

    /// <summary>
    /// Adds a pre-constructed XML element as a child.
    /// </summary>
    public NodeBuilder Raw(XElement element)
    {
        if (element is null) throw new ArgumentNullException(nameof(element));

        _children.Add(() => new XElement(element));
        return this;
    }

    /// <summary>
    /// Produces the XML representation consumed by the interpreter.
    /// </summary>
    public XElement ToXElement()
    {
        var element = new XElement(_name);
        foreach ((string key, string value) in _attributes)
        {
            element.SetAttributeValue(key, value);
        }

        foreach (Func<XNode> factory in _children)
        {
            element.Add(factory());
        }

        return element;
    }

    /// <summary>
    /// Creates a builder for a node with the specified name.
    /// </summary>
    public static NodeBuilder Create(string name) => new(name);

    private static string FormatValue(object value) => value switch
    {
        string s => s,
        char[] chars => new string(chars),
        IEnumerable<char> enumerable when enumerable is not string => new string(enumerable.ToArray()),
        bool b => b ? bool.TrueString : bool.FalseString,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}

/// <summary>
/// Provides convenience factories for commonly used node types.
/// </summary>
public static class Nodes
{
    public static NodeBuilder One(
        string? values = null,
        string? @in = null,
        string? @out = null,
        string? on = null,
        string? color = null,
        bool? origin = null,
        int? steps = null,
        double? temperature = null,
        bool? periodic = null,
        string? file = null,
        string? legend = null,
        string? symmetry = null,
        bool? search = null,
        int? limit = null,
        double? depthCoefficient = null,
        string? comment = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "one",
            children,
            attributes,
            ("values", values),
            ("in", @in),
            ("out", @out),
            ("on", on),
            ("color", color),
            ("origin", origin),
            ("steps", steps),
            ("temperature", temperature),
            ("periodic", periodic),
            ("file", file),
            ("legend", legend),
            ("symmetry", symmetry),
            ("search", search),
            ("limit", limit),
            ("depthCoefficient", depthCoefficient),
            ("comment", comment));

    public static NodeBuilder All(
        string? values = null,
        string? @in = null,
        string? @out = null,
        string? on = null,
        string? color = null,
        string? scope = null,
        string? grid = null,
        string? state = null,
        string? set = null,
        double? weight = null,
        double? probability = null,
        int? steps = null,
        double? temperature = null,
        bool? periodic = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "all",
            children,
            attributes,
            ("values", values),
            ("in", @in),
            ("out", @out),
            ("on", on),
            ("color", color),
            ("scope", scope),
            ("grid", grid),
            ("state", state),
            ("set", set),
            ("weight", weight),
            ("probability", probability),
            ("steps", steps),
            ("temperature", temperature),
            ("periodic", periodic));

    public static NodeBuilder Parallel(
        string? values = null,
        int? steps = null,
        double? temperature = null,
        bool? periodic = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "prl",
            children,
            attributes,
            ("values", values),
            ("steps", steps),
            ("temperature", temperature),
            ("periodic", periodic));

    public static NodeBuilder Markov(
        string? values = null,
        int? steps = null,
        double? temperature = null,
        bool? periodic = null,
        string? comment = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "markov",
            children,
            attributes,
            ("values", values),
            ("steps", steps),
            ("temperature", temperature),
            ("periodic", periodic),
            ("comment", comment));

    public static NodeBuilder Sequence(
        string? values = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "sequence",
            children,
            attributes,
            ("values", values));

    public static NodeBuilder Map(
        string? values = null,
        string? @in = null,
        string? @out = null,
        string? on = null,
        string? color = null,
        string? from = null,
        string? to = null,
        string? axes = null,
        string? axis = null,
        string? mode = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "map",
            children,
            attributes,
            ("values", values),
            ("in", @in),
            ("out", @out),
            ("on", on),
            ("color", color),
            ("from", from),
            ("to", to),
            ("axes", axes),
            ("axis", axis),
            ("mode", mode));

    public static NodeBuilder Convolution(
        string? values = null,
        string? sample = null,
        string? black = null,
        string? white = null,
        string? substrate = null,
        double? threshold = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "convolution",
            children,
            attributes,
            ("values", values),
            ("sample", sample),
            ("black", black),
            ("white", white),
            ("substrate", substrate),
            ("threshold", threshold));

    public static NodeBuilder ConvChain(
        string? values = null,
        string? sample = null,
        string? substrate = null,
        string? black = null,
        string? white = null,
        int? steps = null,
        int? N = null,
        double? temperature = null,
        bool? periodic = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "convchain",
            children,
            attributes,
            ("values", values),
            ("sample", sample),
            ("substrate", substrate),
            ("black", black),
            ("white", white),
            ("steps", steps),
            ("N", N),
            ("temperature", temperature),
            ("periodic", periodic));

    public static NodeBuilder Wfc(
        string? values = null,
        string? subset = null,
        bool? ground = null,
        bool? periodic = null,
        int? iterations = null,
        int? limit = null,
        double? temperature = null,
        string? heuristic = null,
        string? snapshot = null,
        string? select = null,
        bool? backtracking = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "wfc",
            children,
            attributes,
            ("values", values),
            ("subset", subset),
            ("ground", ground),
            ("periodic", periodic),
            ("iterations", iterations),
            ("limit", limit),
            ("temperature", temperature),
            ("heuristic", heuristic),
            ("snapshot", snapshot),
            ("select", select),
            ("backtracking", backtracking));

    public static NodeBuilder Path(
        string? values = null,
        string? from = null,
        string? to = null,
        string? on = null,
        string? color = null,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(
            "path",
            children,
            attributes,
            ("values", values),
            ("from", from),
            ("to", to),
            ("on", on),
            ("color", color));

    public static NodeBuilder Custom(
        string name,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IEnumerable<NodeBuilder>? children = null)
        => Node(name, children, attributes);

    private static NodeBuilder Node(
        string name,
        IEnumerable<NodeBuilder>? children,
        IReadOnlyDictionary<string, object?>? attributes,
        params (string Key, object? Value)[] knownAttributes)
    {
        var builder = new NodeBuilder(name);

        foreach ((string key, object? value) in knownAttributes)
        {
            if (value != null)
            {
                builder.Attribute(key, value);
            }
        }

        if (attributes != null)
        {
            foreach ((string key, object? value) in attributes)
            {
                if (value != null)
                {
                    builder.Attribute(key, value);
                }
            }
        }

        if (children != null)
        {
            builder.Children(children.ToArray());
        }

        return builder;
    }
}
