using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine.Serialization;

/// <summary>
/// Loads model definitions from the legacy <c>models.xml</c> manifest.
/// </summary>
public sealed class XmlModelCatalog : IModelCatalog
{
    private readonly Dictionary<string, ModelDefinition> _models;
    private readonly List<ModelDefinition> _ordered;

    public XmlModelCatalog(string manifestPath)
    {
        if (manifestPath is null) throw new ArgumentNullException(nameof(manifestPath));
        if (!File.Exists(manifestPath)) throw new FileNotFoundException("Model manifest not found.", manifestPath);

        XDocument index = XDocument.Load(manifestPath, LoadOptions.SetLineInfo);
        string baseDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? Environment.CurrentDirectory;

        _models = new Dictionary<string, ModelDefinition>(StringComparer.OrdinalIgnoreCase);
        _ordered = new List<ModelDefinition>();
        foreach (XElement entry in index.Root?.Elements("model") ?? Enumerable.Empty<XElement>())
        {
            ModelDefinition definition = LoadModel(entry, baseDirectory);
            _models[definition.Name] = definition;
            _ordered.Add(definition);
        }
    }

    public IEnumerable<ModelDefinition> GetModels() => _ordered;

    public bool TryGet(string name, out ModelDefinition? model) => _models.TryGetValue(name, out model);

    private static ModelDefinition LoadModel(XElement manifestEntry, string baseDirectory)
    {
        string name = manifestEntry.Get<string>("name");
        int linearSize = manifestEntry.Get("size", -1);
        int dimension = manifestEntry.Get("d", 2);
        int MX = manifestEntry.Get("length", linearSize);
        int MY = manifestEntry.Get("width", linearSize);
        int MZ = manifestEntry.Get("height", dimension == 2 ? 1 : linearSize);

        string modelPath = Path.Combine(baseDirectory, "models", name + ".xml");
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model '{name}' references missing file {modelPath}.");
        }

        XDocument modelDocument = XDocument.Load(modelPath, LoadOptions.SetLineInfo);
        XElement root = modelDocument.Root ?? throw new InvalidDataException($"Model '{name}' has no root element.");

        GridDefinition<char> gridDefinition = XmlGridDefinitionLoader.FromElement(root, MX, MY, MZ);

        bool gif = manifestEntry.Get("gif", false);
        int amount = manifestEntry.Get("amount", 2);
        if (gif) amount = 1;

        string? seedString = manifestEntry.Get<string>("seeds", null);
        IReadOnlyList<int>? seeds = seedString == null
            ? null
            : seedString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

        int steps = manifestEntry.Get("steps", -1);
        int? maxSteps = steps > 0 ? steps : null;

        var execution = new ModelExecutionSettings(
            amount,
            maxSteps,
            gif,
            manifestEntry.Get("iso", false),
            manifestEntry.Get("pixelsize", 4),
            manifestEntry.Get("gui", 0),
            seeds);

        Dictionary<char, int>? paletteOverrides = null;
        foreach (XElement colorOverride in manifestEntry.Elements("color"))
        {
            paletteOverrides ??= new Dictionary<char, int>();
            paletteOverrides[colorOverride.Get<char>("symbol")] = (255 << 24) + Convert.ToInt32(colorOverride.Get<string>("value"), 16);
        }

        ModelDefinition definition = new ModelDefinition(
            name,
            gridDefinition,
            root,
            execution,
            paletteOverrides,
            root.Get<string>("symmetry", null),
            root.Get("origin", false));

        return definition;
    }

}
