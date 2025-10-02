namespace MarkovJunior.Engine;

/// <summary>
/// Provides metadata about an individual generation run.
/// </summary>
public readonly struct GenerationRunContext
{
    public GenerationRunContext(int runIndex, int seed, bool emitGif, int? maxSteps)
    {
        RunIndex = runIndex;
        Seed = seed;
        EmitGif = emitGif;
        MaxSteps = maxSteps;
    }

    public int RunIndex { get; }

    public int Seed { get; }

    public bool EmitGif { get; }

    public int? MaxSteps { get; }
}
