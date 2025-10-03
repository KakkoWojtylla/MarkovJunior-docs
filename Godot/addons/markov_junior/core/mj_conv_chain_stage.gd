class_name MJConvChainStage
extends MJRuleStage

var sample: PackedStringArray
var window_size: int = 3
var steps: int = 128
var periodic: bool = true

func _init(p_sample: Variant, p_name: String = "convchain", p_window: int = 3, p_steps: int = 128, p_periodic: bool = true) -> void:
    sample = _normalize_rows(p_sample)
    window_size = max(p_window, 1)
    steps = max(p_steps, 1)
    periodic = p_periodic
    super._init(p_name)

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if sample.is_empty():
        return false
    var patterns := _extract_patterns()
    if patterns.is_empty():
        return false
    var changed := false
    for i in steps:
        var pattern: PackedStringArray = patterns[context.rng.randi_range(0, patterns.size() - 1)]
        var x := context.rng.randi_range(0, canvas.width - 1)
        var y := context.rng.randi_range(0, canvas.height - 1)
        if _paste_pattern(canvas, pattern, Vector2i(x, y)):
            changed = true
    if changed:
        context.snapshot(canvas, name)
    return changed

func _extract_patterns() -> Array:
    var patterns := []
    var width := sample[0].length()
    var height := sample.size()
    for y in range(height):
        for x in range(width):
            var block := PackedStringArray()
            for dy in window_size:
                var row := ""
                for dx in window_size:
                    var sx := (x + dx) % width if periodic else x + dx
                    var sy := (y + dy) % height if periodic else y + dy
                    if not periodic and (sx >= width or sy >= height):
                        row += MJCanvas.DEFAULT_FILL
                    else:
                        row += sample[sy][sx]
                block.append(row)
            patterns.append(block)
    return patterns

func _paste_pattern(canvas: MJCanvas, pattern: PackedStringArray, position: Vector2i) -> bool:
    var changed := false
    for y in pattern.size():
        var row := pattern[y]
        for x in row.length():
            var cx := position.x + x
            var cy := position.y + y
            if periodic:
                cx = (cx + canvas.width) % canvas.width
                cy = (cy + canvas.height) % canvas.height
            elif cx < 0 or cy < 0 or cx >= canvas.width or cy >= canvas.height:
                continue
            var symbol := row[x]
            if canvas.get_cell(cx, cy) != symbol:
                canvas.set_cell(cx, cy, symbol)
                changed = true
    return changed

static func _normalize_rows(data: Variant) -> PackedStringArray:
    if data == null:
        return PackedStringArray()
    if typeof(data) == TYPE_PACKED_STRING_ARRAY:
        return data.duplicate()
    if typeof(data) == TYPE_ARRAY:
        var rows := PackedStringArray()
        for row in data:
            rows.append(String(row))
        return rows
    if typeof(data) == TYPE_STRING:
        return PackedStringArray([data])
    push_error("Unsupported sample type for ConvChain stage: %s" % typeof(data))
    return PackedStringArray()
