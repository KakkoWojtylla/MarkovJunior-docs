using System;
using System.Collections.Generic;
using System.Linq;
using MarkovJunior.Engine.Runtime;
using MarkovJunior.Engine;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Represents the output of a <see cref="GenerationRunner"/> execution using character legends.
/// </summary>
public sealed class GenerationResult
{
    internal GenerationResult(IReadOnlyList<GenerationFrame> frames)
    {
        Frames = frames ?? throw new ArgumentNullException(nameof(frames));
        if (Frames.Count == 0)
        {
            throw new ArgumentException("At least one frame must be captured.", nameof(frames));
        }
    }

    /// <summary>All captured frames.</summary>
    public IReadOnlyList<GenerationFrame> Frames { get; }

    /// <summary>The final frame in the run.</summary>
    public GenerationFrame FinalFrame => Frames[^1];

    /// <summary>
    /// Returns the final frame as an array of strings (one per row).
    /// Only valid for 2D grids.
    /// </summary>
    public IReadOnlyList<string> AsStrings() => FinalFrame.AsStrings();

    /// <summary>
    /// Projects the final frame legend into another symbol domain.
    /// </summary>
    public GenerationResult<TSymbol> ToTyped<TSymbol>(Func<char, TSymbol> selector)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        var typedFrames = Frames.Select(frame => frame.ToTyped(selector)).ToArray();
        return new GenerationResult<TSymbol>(typedFrames);
    }
}

/// <summary>
/// Represents the output of a <see cref="GenerationRunner"/> execution projected to custom symbols.
/// </summary>
public sealed class GenerationResult<TSymbol>
{
    internal GenerationResult(IReadOnlyList<TypedGenerationFrame<TSymbol>> frames)
    {
        Frames = frames ?? throw new ArgumentNullException(nameof(frames));
        if (Frames.Count == 0)
        {
            throw new ArgumentException("At least one frame must be captured.", nameof(frames));
        }
    }

    public IReadOnlyList<TypedGenerationFrame<TSymbol>> Frames { get; }

    public TypedGenerationFrame<TSymbol> FinalFrame => Frames[^1];

    /// <summary>
    /// Converts the final frame to row arrays. Only valid for 2D grids.
    /// </summary>
    public IReadOnlyList<TSymbol[]> AsRows()
    {
        return FinalFrame.AsRows();
    }
}
