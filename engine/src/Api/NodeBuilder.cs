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
    public static NodeBuilder One(params NodeBuilder[] children) => Node("one", children);

    public static NodeBuilder All(params NodeBuilder[] children) => Node("all", children);

    public static NodeBuilder Parallel(params NodeBuilder[] children) => Node("prl", children);

    public static NodeBuilder Markov(params NodeBuilder[] children) => Node("markov", children);

    public static NodeBuilder Sequence(params NodeBuilder[] children) => Node("sequence", children);

    public static NodeBuilder Map(params NodeBuilder[] children) => Node("map", children);

    public static NodeBuilder Convolution(params NodeBuilder[] children) => Node("convolution", children);

    public static NodeBuilder ConvChain(params NodeBuilder[] children) => Node("convchain", children);

    public static NodeBuilder Wfc(params NodeBuilder[] children) => Node("wfc", children);

    public static NodeBuilder Path(params NodeBuilder[] children) => Node("path", children);

    public static NodeBuilder Custom(string name, params NodeBuilder[] children) => Node(name, children);

    private static NodeBuilder Node(string name, params NodeBuilder[] children)
    {
        var builder = new NodeBuilder(name);
        if (children != null && children.Length > 0)
        {
            builder.Children(children);
        }

        return builder;
    }
}
