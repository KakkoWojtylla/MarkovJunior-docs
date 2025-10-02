class_name MarkovJunior
extends Node

static func create_canvas(width: int, height: int, fill_char: String = MJCanvas.DEFAULT_FILL) -> MJCanvas:
    return MJCanvas.new(width, height, fill_char)

static func create_tileset(symbols: PackedStringArray = PackedStringArray()) -> MJTileSet:
    var set := MJTileSet.new()
    for symbol in symbols:
        set.add_tile(symbol)
    return set

static func rule_chain(stages: Array = null) -> MJRuleChain:
    var chain := MJRuleChain.new()
    if stages == null:
        return chain
    for stage in stages:
        chain.add_stage(stage)
    return chain

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

static func runner() -> MJGenerationRunner:
    return MJGenerationRunner.new()
