using System;
using System.Collections.Generic;
using System.IO;
using MarkovJunior.Engine;
using MarkovJunior.Engine.Definitions;

sealed class FileSystemGenerationSink : IGenerationSink
{
    readonly string outputDirectory;
    readonly Dictionary<char, int> basePalette;

    public FileSystemGenerationSink(string outputDirectory, Dictionary<char, int> basePalette)
    {
        this.outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        this.basePalette = basePalette ?? throw new ArgumentNullException(nameof(basePalette));
    }

    Dictionary<char, int> currentPalette = new();

    public void BeginRun(ModelDefinition model, GenerationRunContext context)
    {
        Directory.CreateDirectory(outputDirectory);
        currentPalette = PaletteLoader.ComposePalette(basePalette, model.PaletteOverrides);
    }

    public void HandleFrame(ModelDefinition model, GenerationRunContext context, GenerationFrame frame)
    {
        int[] colors = BuildColorArray(frame.Legend);
        string baseName = context.EmitGif
            ? Path.Combine(outputDirectory, model.Name + $"_{frame.Step:0000}")
            : Path.Combine(outputDirectory, model.Name + $"_{context.Seed}");

        if (frame.Depth == 1 || model.Execution.Isometric)
        {
            var (bitmap, width, height) = Graphics.Render(frame.State, frame.Width, frame.Height, frame.Depth, colors, model.Execution.PixelSize, model.Execution.GuiScale);
            Graphics.SaveBitmap(bitmap, width, height, baseName + ".png");
        }
        else
        {
            VoxHelper.SaveVox(frame.State, (byte)frame.Width, (byte)frame.Height, (byte)frame.Depth, colors, baseName + ".vox");
        }
    }

    public void CompleteRun(ModelDefinition model, GenerationRunContext context)
    {
        Console.WriteLine("DONE");
    }

    int[] BuildColorArray(char[] legend)
    {
        int[] colors = new int[legend.Length];
        for (int i = 0; i < legend.Length; i++)
        {
            colors[i] = currentPalette.TryGetValue(legend[i], out int value) ? value : unchecked((int)0xFFFFFFFF);
        }

        return colors;
    }
}
