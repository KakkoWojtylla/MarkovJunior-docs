using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Provides access to in-memory resources required by legacy nodes that previously loaded assets from disk.
/// </summary>
public interface IResourceStore
{
    bool TryGetPattern(string name, out PatternResource pattern);

    bool TryGetSample(string name, out SampleResource sample);

    bool TryGetConvChainSample(string name, out ConvChainSampleResource sample);

    bool TryGetVox(string name, out VoxResource vox);

    bool TryGetXml(string name, out XDocument document);
}

/// <summary>
/// Represents a character pattern used by rule resources.
/// </summary>
/// <param name="Name">Logical identifier.</param>
/// <param name="Data">Flattened character data in XYZ order.</param>
/// <param name="Width">Pattern width.</param>
/// <param name="Height">Pattern height.</param>
/// <param name="Depth">Pattern depth (1 for 2D).</param>
public sealed record PatternResource(string Name, char[] Data, int Width, int Height, int Depth);

/// <summary>
/// Represents a 2D character sample used by overlapping models.
/// </summary>
/// <param name="Name">Logical identifier.</param>
/// <param name="Data">Flattened character data in XY order.</param>
/// <param name="Width">Sample width.</param>
/// <param name="Height">Sample height.</param>
public sealed record SampleResource(string Name, char[] Data, int Width, int Height);

/// <summary>
/// Represents a boolean ConvChain sample.
/// </summary>
/// <param name="Name">Logical identifier.</param>
/// <param name="Data">Flattened boolean data in XY order.</param>
/// <param name="Width">Sample width.</param>
/// <param name="Height">Sample height.</param>
public sealed record ConvChainSampleResource(string Name, bool[] Data, int Width, int Height);

/// <summary>
/// Represents raw voxel data.
/// </summary>
/// <param name="Name">Logical identifier.</param>
/// <param name="Data">Flattened palette indices in XYZ order.</param>
/// <param name="Width">Voxel volume width.</param>
/// <param name="Height">Voxel volume height.</param>
/// <param name="Depth">Voxel volume depth.</param>
public sealed record VoxResource(string Name, int[] Data, int Width, int Height, int Depth);

/// <summary>
/// Mutable builder for in-memory resource stores.
/// </summary>
public sealed class ResourceStoreBuilder
{
    private readonly Dictionary<string, PatternResource> _patterns = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SampleResource> _samples = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ConvChainSampleResource> _convChainSamples = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, VoxResource> _vox = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, XDocument> _xml = new(StringComparer.OrdinalIgnoreCase);

    public ResourceStoreBuilder AddPattern(string name, char[] data, int width, int height, int depth = 1)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Pattern name cannot be null or whitespace.", nameof(name));
        if (data is null) throw new ArgumentNullException(nameof(data));
        _patterns[name] = new PatternResource(name, (char[])data.Clone(), width, height, depth);
        return this;
    }

    public ResourceStoreBuilder AddSample(string name, char[] data, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Sample name cannot be null or whitespace.", nameof(name));
        if (data is null) throw new ArgumentNullException(nameof(data));
        _samples[name] = new SampleResource(name, (char[])data.Clone(), width, height);
        return this;
    }

    public ResourceStoreBuilder AddConvChainSample(string name, bool[] data, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Sample name cannot be null or whitespace.", nameof(name));
        if (data is null) throw new ArgumentNullException(nameof(data));
        _convChainSamples[name] = new ConvChainSampleResource(name, (bool[])data.Clone(), width, height);
        return this;
    }

    public ResourceStoreBuilder AddVox(string name, int[] data, int width, int height, int depth)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Vox name cannot be null or whitespace.", nameof(name));
        if (data is null) throw new ArgumentNullException(nameof(data));
        _vox[name] = new VoxResource(name, (int[])data.Clone(), width, height, depth);
        return this;
    }

    public ResourceStoreBuilder AddXml(string name, XDocument document)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("XML name cannot be null or whitespace.", nameof(name));
        if (document is null) throw new ArgumentNullException(nameof(document));
        _xml[name] = new XDocument(document);
        return this;
    }

    public bool HasResources
        => _patterns.Count != 0 || _samples.Count != 0 || _convChainSamples.Count != 0 || _vox.Count != 0 || _xml.Count != 0;

    public IResourceStore Build()
        => new ImmutableResourceStore(_patterns, _samples, _convChainSamples, _vox, _xml);

    private sealed class ImmutableResourceStore : IResourceStore
    {
        private readonly IReadOnlyDictionary<string, PatternResource> _patterns;
        private readonly IReadOnlyDictionary<string, SampleResource> _samples;
        private readonly IReadOnlyDictionary<string, ConvChainSampleResource> _convChainSamples;
        private readonly IReadOnlyDictionary<string, VoxResource> _vox;
        private readonly IReadOnlyDictionary<string, XDocument> _xml;

        public ImmutableResourceStore(
            IDictionary<string, PatternResource> patterns,
            IDictionary<string, SampleResource> samples,
            IDictionary<string, ConvChainSampleResource> convChainSamples,
            IDictionary<string, VoxResource> vox,
            IDictionary<string, XDocument> xml)
        {
            _patterns = new ReadOnlyDictionary<string, PatternResource>(patterns.ToDictionary(p => p.Key, p => p.Value));
            _samples = new ReadOnlyDictionary<string, SampleResource>(samples.ToDictionary(p => p.Key, p => p.Value));
            _convChainSamples = new ReadOnlyDictionary<string, ConvChainSampleResource>(convChainSamples.ToDictionary(p => p.Key, p => p.Value));
            _vox = new ReadOnlyDictionary<string, VoxResource>(vox.ToDictionary(p => p.Key, p => p.Value));
            _xml = new ReadOnlyDictionary<string, XDocument>(xml.ToDictionary(p => p.Key, p => new XDocument(p.Value)));
        }

        public bool TryGetPattern(string name, out PatternResource pattern)
            => _patterns.TryGetValue(name, out pattern);

        public bool TryGetSample(string name, out SampleResource sample)
            => _samples.TryGetValue(name, out sample);

        public bool TryGetConvChainSample(string name, out ConvChainSampleResource sample)
            => _convChainSamples.TryGetValue(name, out sample);

        public bool TryGetVox(string name, out VoxResource vox)
            => _vox.TryGetValue(name, out vox);

        public bool TryGetXml(string name, out XDocument document)
        {
            if (_xml.TryGetValue(name, out XDocument? stored))
            {
                document = new XDocument(stored);
                return true;
            }

            document = null!;
            return false;
        }
    }
}
