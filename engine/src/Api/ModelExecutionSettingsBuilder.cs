using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Fluent builder for <see cref="ModelExecutionSettings"/> instances used by <see cref="ModelBuilder"/>.
/// </summary>
public sealed class ModelExecutionSettingsBuilder
{
    private int _runs = 1;
    private int? _steps;
    private bool _emitGif;
    private bool _isometric;
    private int _pixelSize = 4;
    private int _guiScale;
    private readonly List<int> _seeds = new();

    public ModelExecutionSettingsBuilder Runs(int runs)
    {
        if (runs <= 0) throw new ArgumentOutOfRangeException(nameof(runs));
        _runs = runs;
        return this;
    }

    public ModelExecutionSettingsBuilder Steps(int? steps)
    {
        if (steps.HasValue && steps.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(steps));
        }

        _steps = steps;
        return this;
    }

    public ModelExecutionSettingsBuilder EmitGif(bool emitGif = true)
    {
        _emitGif = emitGif;
        return this;
    }

    public ModelExecutionSettingsBuilder Isometric(bool isometric = true)
    {
        _isometric = isometric;
        return this;
    }

    public ModelExecutionSettingsBuilder PixelSize(int pixelSize)
    {
        if (pixelSize <= 0) throw new ArgumentOutOfRangeException(nameof(pixelSize));
        _pixelSize = pixelSize;
        return this;
    }

    public ModelExecutionSettingsBuilder GuiScale(int guiScale)
    {
        if (guiScale < 0) throw new ArgumentOutOfRangeException(nameof(guiScale));
        _guiScale = guiScale;
        return this;
    }

    public ModelExecutionSettingsBuilder ClearSeeds()
    {
        _seeds.Clear();
        return this;
    }

    public ModelExecutionSettingsBuilder AddSeed(int seed)
    {
        _seeds.Add(seed);
        return this;
    }

    public ModelExecutionSettings Build()
    {
        IReadOnlyList<int>? seeds = _seeds.Count == 0 ? null : new ReadOnlyCollection<int>(_seeds);
        return new ModelExecutionSettings(_runs, _steps, _emitGif, _isometric, _pixelSize, _guiScale, seeds);
    }
}
