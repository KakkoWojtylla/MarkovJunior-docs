namespace MarkovJunior.Engine;

/// <summary>
/// Represents a single cell modification performed during a generation step.
/// </summary>
public readonly struct GridChange
{
    public GridChange(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>The x-coordinate of the changed cell.</summary>
    public int X { get; }

    /// <summary>The y-coordinate of the changed cell.</summary>
    public int Y { get; }

    /// <summary>The z-coordinate of the changed cell.</summary>
    public int Z { get; }
}
