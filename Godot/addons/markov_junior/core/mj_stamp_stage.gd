class_name MJStampStage
extends MJRuleStage

var pattern: PackedStringArray
var position := Vector2i.ZERO
var overwrite: bool = true
var ignore_symbol := " "

func _init(p_pattern: PackedStringArray, p_position: Vector2i = Vector2i.ZERO, p_overwrite: bool = true, p_ignore: String = " ", p_name: String = "stamp") -> void:
    pattern = p_pattern.duplicate()
    position = p_position
    overwrite = p_overwrite
    ignore_symbol = p_ignore
    super._init(p_name)

func apply(canvas: MJCanvas, context: StageContext) -> void:
    for y in pattern.size():
        var row := pattern[y]
        for x in row.length():
            var symbol := row[x]
            if symbol == ignore_symbol:
                continue
            var cx := position.x + x
            var cy := position.y + y
            if overwrite or canvas.get_cell(cx, cy) == ignore_symbol:
                canvas.set_cell(cx, cy, symbol)
    context.snapshot(canvas, name)
