using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

static class PaletteLoader
{
    public static Dictionary<char, int> LoadBasePalette(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Palette file '{path}' was not found.");
        }

        XDocument document = XDocument.Load(path);
        return document.Root?.Elements("color")
            .ToDictionary(x => x.Get<char>("symbol"), x => (255 << 24) + Convert.ToInt32(x.Get<string>("value"), 16))
            ?? new Dictionary<char, int>();
    }

    public static Dictionary<char, int> ComposePalette(Dictionary<char, int> basePalette, IReadOnlyDictionary<char, int>? overrides)
    {
        Dictionary<char, int> palette = new(basePalette);
        if (overrides != null)
        {
            foreach (var kvp in overrides)
            {
                palette[kvp.Key] = kvp.Value;
            }
        }

        return palette;
    }
}
