using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MarkovJunior.Engine;
using MarkovJunior.Engine.Definitions;
using MarkovJunior.Engine.Serialization;

static class Program
{
    static void Main()
    {
        Stopwatch sw = Stopwatch.StartNew();
        string outputFolder = Path.Combine(Environment.CurrentDirectory, "output");
        if (Directory.Exists(outputFolder))
        {
            foreach (string file in Directory.GetFiles(outputFolder)) File.Delete(file);
        }
        else
        {
            Directory.CreateDirectory(outputFolder);
        }

        Dictionary<char, int> palette = PaletteLoader.LoadBasePalette(Path.Combine("resources", "palette.xml"));
        XmlModelCatalog catalog = new XmlModelCatalog("models.xml");
        EngineRunner runner = new EngineRunner(new DefinitionInterpreterFactory(new CharacterGridCompiler()));
        FileSystemGenerationSink sink = new FileSystemGenerationSink(outputFolder, palette);

        foreach (ModelDefinition model in catalog.GetModels())
        {
            Console.Write($"{model.Name} > ");
            runner.Run(model, sink);
        }

        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }
}
