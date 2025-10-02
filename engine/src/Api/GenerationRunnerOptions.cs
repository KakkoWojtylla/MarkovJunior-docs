using System;
using MarkovJunior.Engine.Runtime;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Options that influence the behaviour of <see cref="GenerationRunner"/>.
/// </summary>
public sealed class GenerationRunnerOptions
{
    private int? _seed;

    /// <summary>
    /// Seed used by the interpreter. When <c>null</c> a random seed will be generated.
    /// </summary>
    public int? Seed
    {
        get => _seed;
        set => _seed = value;
    }

    /// <summary>
    /// Determines whether intermediate frames should be captured in addition to the final frame.
    /// </summary>
    public bool CaptureIntermediateFrames { get; set; }

    /// <summary>
    /// Optional session-level overrides applied when the runner creates a <see cref="GenerationSession"/>.
    /// </summary>
    public GenerationSessionOptions? SessionOptions { get; set; }

    internal int ResolveSeed()
    {
        if (_seed.HasValue)
        {
            return _seed.Value;
        }

        return Random.Shared.Next();
    }
}
