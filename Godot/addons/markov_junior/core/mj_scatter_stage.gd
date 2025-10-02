class_name MJScatterStage
extends MJRuleStage

var symbol := "#"
var count := 10
var allowed := []

func _init(p_symbol: String, p_count: int, p_allowed: Array = [], p_name: String = "scatter") -> void:
    symbol = p_symbol
    count = p_count
    allowed = p_allowed.duplicate() if p_allowed != null else []
    super._init(p_name)

func apply(canvas: MJCanvas, context: StageContext) -> void:
    var placed := 0
    var attempts := 0
    var max_attempts := count * 10 + 100
    while placed < count and attempts < max_attempts:
        attempts += 1
        var x := context.rng.randi_range(0, canvas.width - 1)
        var y := context.rng.randi_range(0, canvas.height - 1)
        var current := canvas.get_cell(x, y)
        if allowed.size() > 0 and not allowed.has(current):
            continue
        canvas.set_cell(x, y, symbol)
        placed += 1
    context.snapshot(canvas, name)
