# MarkovJunior-docs
[MarkovJunior](https://github.com/mxgmn/MarkovJunior) is a probabilistic programming language where programs are combinations of rewrite rules and inference is performed via constraint propagation, developed by Maxim Gumin. This fork adds documentation and a few explanatory comments, which may be useful to others reading the code.

## In-memory API

The `MarkovJunior.Engine.Api` namespace exposes builders that let you describe models entirely in code without touching the legacy XML files. You create nodes with `NodeBuilder`/`Nodes`, assemble the grid with `ModelBuilder`, and run everything through `GenerationRunner`:

```csharp
using MarkovJunior.Engine.Api;

var root = Nodes.Sequence(
    children: new[]
    {
        Nodes.One(values: "BW", @in: "B", @out: "W"),
        Nodes.Markov(
            children: new[]
            {
                Nodes.All(@in: "W", @out: "BW"),
                Nodes.Path(from: "B", to: "W", on: "B", color: "W")
            })
    });

var model = new ModelBuilder()
    .WithName("code-first-demo")
    .WithSize(32, 32)
    .WithAlphabet("BW")
    .AddRulePattern("line", "BBB", "BWB", "BBB")
    .AddSample("rooms",
        "########",
        "#......#",
        "#.####.#",
        "#.#  #.#",
        "########")
    .AddConvChainSample("hatch", new[]
    {
        "###",
        "# #",
        "###"
    })
    .WithRoot(root)
    .Build();

var runner = new GenerationRunner();
var result = runner.Run(model, new GenerationRunnerOptions { Seed = 1337 });

foreach (var row in result.AsStrings())
{
    Console.WriteLine(row);
}
```

All MarkovJunior nodes and attributes can be expressed via builder calls, and `GenerationRunner` returns the produced frames as in-memory buffers so they can be consumed directly by a game engine. When you need raw characters rather than formatted rows, use `GenerationResult.AsCharArray()` for a flattened buffer, `AsCharGrid2D()` for a `[y, x]` matrix, or `AsCharGrid3D()` for volumetric outputs. Resource-heavy nodes such as `convchain`, `wfc` and tile models can be fed entirely from memory using `AddRulePattern`, `AddSample`, `AddConvChainSample`, `AddVoxResource` and `AddXmlResource`, so no PNG/VOX/XML files are required at runtime.
