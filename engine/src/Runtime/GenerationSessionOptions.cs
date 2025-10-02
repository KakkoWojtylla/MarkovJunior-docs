using MarkovJunior.Engine;

namespace MarkovJunior.Engine.Runtime;

/// <summary>
/// Optional settings that influence the behaviour of <see cref="GenerationSession"/>.
/// </summary>
public sealed class GenerationSessionOptions
{
    /// <summary>
    /// When specified, overrides the default decision to emit every intermediate
    /// frame during generation.
    /// </summary>
    public bool? EmitIntermediateFrames { get; init; }

    /// <summary>
    /// When specified, constrains the interpreter to execute at most this many
    /// steps. A non-positive value indicates no explicit limit.
    /// </summary>
    public int? MaxSteps { get; init; }

    /// <summary>
    /// Optional logger that receives interpreter output for the lifetime of the session.
    /// </summary>
    public IInterpreterLogger? Logger { get; init; }
}
