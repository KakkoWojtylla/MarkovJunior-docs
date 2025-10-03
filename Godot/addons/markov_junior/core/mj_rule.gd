class_name MJRule
extends RefCounted

const WILDCARD := "?"

var input: PackedStringArray
var output: PackedStringArray
var weight: float = 1.0
var probability: float = 1.0

func _init(p_input: Variant, p_output: Variant = null, p_weight: float = 1.0, p_probability: float = 1.0) -> void:
    input = _normalize_rows(p_input)
    output = _normalize_rows(p_output if p_output != null else p_input)
    weight = max(p_weight, 0.0)
    probability = clamp(p_probability, 0.0, 1.0)
    assert(input.size() == output.size(), "Input and output must have the same number of rows")
    if input.size() > 0:
        var width := input[0].length()
        for row in input:
            assert(row.length() == width, "All input rows must have equal length")
        for row in output:
            assert(row.length() == width, "All output rows must have equal length")

func get_width() -> int:
    return input.size() > 0 ? input[0].length() : 0

func get_height() -> int:
    return input.size()

func matches(canvas: MJCanvas, position: Vector2i) -> bool:
    var w := get_width()
    var h := get_height()
    if position.x < 0 or position.y < 0:
        return false
    if position.x + w > canvas.width or position.y + h > canvas.height:
        return false
    for y in h:
        var row := input[y]
        for x in row.length():
            var expected := row[x]
            if expected == WILDCARD:
                continue
            if canvas.get_cell(position.x + x, position.y + y) != expected:
                return false
    return true

func apply(canvas: MJCanvas, position: Vector2i) -> bool:
    var changed := false
    for y in output.size():
        var row := output[y]
        for x in row.length():
            var symbol := row[x]
            if symbol == WILDCARD:
                continue
            var cx := position.x + x
            var cy := position.y + y
            var previous := canvas.get_cell(cx, cy)
            if previous != symbol:
                canvas.set_cell(cx, cy, symbol)
                changed = true
    return changed

func affected_cells(position: Vector2i) -> Array:
    var cells := []
    for y in output.size():
        for x in output[y].length():
            cells.append(position + Vector2i(x, y))
    return cells

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
    push_error("Unsupported rule row data type: %s" % typeof(data))
    return PackedStringArray()
