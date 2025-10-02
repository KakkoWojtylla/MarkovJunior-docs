using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine.Serialization;

/// <summary>
/// Helper responsible for translating grid metadata embedded in a model XML into a typed definition.
/// </summary>
public static class XmlGridDefinitionLoader
{
    public static GridDefinition<char> FromElement(XElement root, int width, int height, int depth)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        string? valueString = root.Get<string>("values", null)?.Replace(" ", string.Empty);
        if (string.IsNullOrEmpty(valueString))
        {
            throw new InvalidDataException($"Model at line {root.LineNumber()} is missing 'values'.");
        }

        List<char> symbols = valueString.ToList();
        if (symbols.Count != symbols.Distinct().Count())
        {
            throw new InvalidDataException($"Model at line {root.LineNumber()} has duplicate symbols in 'values'.");
        }

        Dictionary<char, IReadOnlyCollection<char>>? unions = null;
        foreach (XElement unionElement in root.MyDescendants("markov", "sequence", "union").Where(x => x.Name == "union"))
        {
            unions ??= new Dictionary<char, IReadOnlyCollection<char>>();
            char unionSymbol = unionElement.Get<char>("symbol");
            IReadOnlyCollection<char> members = unionElement.Get<string>("values").Select(c => c).ToArray();
            unions[unionSymbol] = members;
        }

        IReadOnlyCollection<char>? transparent = null;
        string? transparentString = root.Get<string>("transparent", null);
        if (!string.IsNullOrEmpty(transparentString))
        {
            transparent = transparentString.Select(c => c).ToArray();
        }

        string? folder = root.Get<string>("folder", null);
        return new GridDefinition<char>(width, height, depth, symbols, unions, transparent, folder);
    }
}
