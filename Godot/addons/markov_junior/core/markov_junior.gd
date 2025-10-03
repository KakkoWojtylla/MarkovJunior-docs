class_name MarkovJunior
extends Node

static func create_canvas(width: int, height: int, fill_char: String = MJCanvas.DEFAULT_FILL) -> MJCanvas:
    return MJCanvas.new(width, height, fill_char)

static func create_tileset(symbols: PackedStringArray = PackedStringArray()) -> MJTileSet:
    var set := MJTileSet.new()
    for symbol in symbols:
        set.add_tile(symbol)
    return set

static func rule_chain(stages: Array = []) -> MJRuleChain:
    var chain := MJRuleChain.new()
    for stage in stages:
        chain.add_stage(stage)
    return chain

static func rule(input: Variant, output: Variant = null, weight: float = 1.0, probability: float = 1.0) -> MJRule:
    return MJRule.new(input, output, weight, probability)

static func fill_stage(symbol: String, name: String = "fill") -> MJFillStage:
    return MJFillStage.new(symbol, name)

static func stamp_stage(pattern: Variant, position: Vector2i = Vector2i.ZERO, overwrite: bool = true, ignore_symbol: String = " ", name: String = "stamp") -> MJStampStage:
    var rows := PackedStringArray()
    if typeof(pattern) == TYPE_PACKED_STRING_ARRAY:
        rows = pattern
    elif typeof(pattern) == TYPE_ARRAY:
        for row in pattern:
            rows.append(String(row))
    elif typeof(pattern) == TYPE_STRING:
        rows = PackedStringArray([pattern])
    else:
        push_error("Unsupported pattern type %s" % [typeof(pattern)])
    return MJStampStage.new(rows, position, overwrite, ignore_symbol, name)

static func scatter_stage(symbol: String, count: int, allowed: Array = [], name: String = "scatter") -> MJScatterStage:
    var allow_copy := allowed.duplicate() if allowed != null else []
    return MJScatterStage.new(symbol, count, allow_copy, name)

static func wfc_stage(tile_set: MJTileSet, name: String = "wfc", backtrack_limit: int = 128, wildcard: String = MJCanvas.DEFAULT_FILL) -> MJWFCStage:
    return MJWFCStage.new(tile_set, name, backtrack_limit, wildcard)

static func all_stage(rules: Array = [], name: String = "all", steps: int = 0) -> MJAllStage:
    return MJAllStage.new(rules, name, steps)

static func one_stage(rules: Array = [], name: String = "one", retries: int = 1) -> MJOneStage:
    return MJOneStage.new(rules, name, retries)

static func parallel_stage(rules: Array = [], name: String = "parallel", iterations: int = 1) -> MJParallelStage:
    return MJParallelStage.new(rules, name, iterations)

static func markov_stage(stages: Array = [], name: String = "markov", max_iterations: int = 0) -> MJMarkovStage:
    return MJMarkovStage.new(stages, name, max_iterations)

static func sequence_stage(stages: Array = [], name: String = "sequence") -> MJSequenceStage:
    return MJSequenceStage.new(stages, name)

static func map_stage(rules: Array = [], name: String = "map", fill_symbol: String = MJCanvas.DEFAULT_FILL, wrap: bool = true, children: Array = []) -> MJMapStage:
    return MJMapStage.new(rules, name, fill_symbol, wrap, children)

static func path_stage(start: Variant, end: Variant, path_symbol: String = "*", name: String = "path") -> MJPathStage:
    return MJPathStage.new(start, end, path_symbol, name)

static func convolution_rule(input_symbol: String, output_symbol: String, contributors: Variant = null, counts: Variant = null, probability: float = 1.0) -> MJConvolutionStage.ConvolutionRule:
    return MJConvolutionStage.ConvolutionRule.new(input_symbol, output_symbol, contributors, counts, probability)

static func convolution_stage(rules: Array = [], name: String = "convolution", neighborhood: String = "moore", periodic: bool = false) -> MJConvolutionStage:
    return MJConvolutionStage.new(rules, name, neighborhood, periodic)

static func conv_chain_stage(sample: Variant, name: String = "convchain", window: int = 3, steps: int = 128, periodic: bool = true) -> MJConvChainStage:
    return MJConvChainStage.new(sample, name, window, steps, periodic)

static func runner() -> MJGenerationRunner:
    return MJGenerationRunner.new()