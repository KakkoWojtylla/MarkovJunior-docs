# MarkovJunior Godot Addon

This addon provides a code-first, in-memory procedural generation toolkit for Godot projects inspired by MarkovJunior. Everything runs entirely inside GDScript and operates on character grids, making it easy to drive tilemaps, textures, or gameplay logic without relying on XML manifests or external files.

## Installation

1. Copy the `addons/markov_junior` folder into your Godot project's `res://addons/` directory.
2. Enable **MarkovJunior** in the **Project > Project Settings > Plugins** tab.

## Quickstart

```gdscript
extends Node

func _ready() -> void:
    var tileset := MarkovJunior.create_tileset([".", "#", "S", "E", "T"])
    tileset.add_adjacency(".", ".")
    tileset.add_adjacency(".", "#")
    tileset.add_adjacency("#", "#")
    tileset.add_adjacency("S", ".", "east")
    tileset.add_adjacency("E", ".", "west")

    var carve := MarkovJunior.rule([
        "???",
        "?#?",
        "???"
    ], [
        "...",
        ".#.",
        "..."
    ])

    var treasures := MarkovJunior.rule([
        "...",
        "...",
        "..."
    ], [
        "...",
        ".T.",
        "..."
    ], weight = 0.5)

    var rules := MarkovJunior.sequence_stage([
        MarkovJunior.fill_stage("?", "prepare"),
        MarkovJunior.scatter_stage("S", 1, [], "spawn"),
        MarkovJunior.scatter_stage("E", 1, [], "exit"),
        MarkovJunior.one_stage([treasures], "seed"),
        MarkovJunior.all_stage([carve], "carve", steps = 3),
        MarkovJunior.path_stage(["S"], ["E"], "T", "path").set_passable([".", "T"]).set_keep_endpoints(true),
        MarkovJunior.wfc_stage(tileset, "rooms", backtrack_limit = 256),
        MarkovJunior.convolution_stage([
            MarkovJunior.convolution_rule("#", ".", ["."], [0, 1, 4, 5, 6, 7, 8]),
            MarkovJunior.convolution_rule(".", "#", ["#"], [5, 6, 7, 8])
        ], "erosion", neighborhood = "moore")
    ], "pipeline")

    var post := MarkovJunior.markov_stage([
        MarkovJunior.conv_chain_stage([
            "....",
            ".##.",
            ".##.",
            "...."
        ], "texture", window = 2, steps = 64)
    ], "polish", max_iterations = 2)

    var chain := MarkovJunior.rule_chain([rules, post])

    var runner := MarkovJunior.runner()
    var options := MJGenerationRunner.Options.new()
    options.seed = randi()
    options.capture_frames = true

    var result := runner.run(chain, 32, 18, "?", options)
    for row in result.to_rows():
        print(row)

    for frame in result.frames:
        print(frame.label, frame.step)
```

## Core concepts

- **MJCanvas** – compact character grid with helpers for cloning, counting symbols, and transforming into `PackedStringArray` or 2D arrays.
- **MJTileSet** – defines symbols, weights, and adjacency rules consumed by the Wave Function Collapse stage. Use `add_tile`, `set_weight`, and `add_adjacency` to model valid neighbours.
- **MJRuleStage** – base class for atomic operations; included stages cover fill, scatter, stamping patterns, and WFC. Extend it to add custom logic.
- **MJRuleChain** – ordered list of stages that execute against a canvas with a shared random generator.
- **MJGenerationRunner** – orchestrates runs, records frames, and exposes session-based stepping when you need finer control.

## Building rule chains

Use the helpers exposed on `MarkovJunior` to assemble stages fluently:

```gdscript
var chain := MarkovJunior.rule_chain()
chain.add_stage(MarkovJunior.fill_stage("."))
chain.add_stage(MarkovJunior.stamp_stage([
    "#####",
    "#   #",
    "#####"
], Vector2i(4, 4), overwrite = false))
chain.add_stage(MarkovJunior.scatter_stage("T", 6, ["."], "treasure"))
chain.add_stage(MarkovJunior.parallel_stage([
    MarkovJunior.rule([
        "..",
        ".."
    ], [
        "##",
        "##"
    ], probability = 0.1)
], "moss", iterations = 4))
chain.add_stage(MarkovJunior.map_stage([
    MarkovJunior.rule([
        "#"
    ], [
        "?"
    ])
], "remap", fill_symbol = "?").add_child(MarkovJunior.conv_chain_stage([
    "?##",
    "#??",
    "?##"
], "detail", window = 2, steps = 16)))
chain.add_stage(MarkovJunior.wfc_stage(tileset, "polish", backtrack_limit = 512))
```

### Rule-driven stages

- `MarkovJunior.rule` defines rewrite rules used by `all_stage`, `one_stage`, and `parallel_stage`.
- `all_stage` applies a maximal set of non-overlapping matches per iteration.
- `one_stage` selects a single weighted match, retrying when desired.
- `parallel_stage` evaluates every match with per-rule probabilities.

### Structural stages

- `sequence_stage` and `markov_stage` let you build nested flows, mirroring the classic `sequence` and `markov` nodes.
- `map_stage` rewrites the whole canvas into a fresh grid before running optional child stages.
- `path_stage` finds a route between markers with BFS and draws it using your chosen symbol.

### Sampling stages

- `convolution_stage` performs Life-like neighbourhood filtering with configurable kernels.
- `conv_chain_stage` stamps overlapping patches from a sample texture for quick tiling noise.
- `wfc_stage` wraps the tile-based Wave Function Collapse solver from the tileset helpers.

## Runner options and sessions

`MJGenerationRunner.Options` lets you control randomness and frame capture:

- `seed` – provide a deterministic seed (set to `0` to randomise per run).
- `capture_frames` – store intermediate canvas snapshots after each stage.
- `initial_state` – optional `PackedStringArray` that seeds the canvas before stages execute.

For step-by-step control, create a session:

```gdscript
var session := MarkovJunior.runner().start_session(chain, MJCanvas.new(16, 16))
while session.step():
    pass
var final_rows := session.get_canvas().to_rows()
```

## Extending the addon

Create new stages by inheriting from `MJRuleStage` and overriding `apply(canvas, context)`. Call `context.snapshot(canvas, label)` whenever you want to capture an intermediate frame for debugging or visualisation.

## License

This addon is distributed under the same license as the upstream repository (MIT).
