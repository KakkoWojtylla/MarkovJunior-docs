class_name MJFillStage
extends MJRuleStage

var symbol := "?"

func _init(p_symbol: String, p_name: String = "fill") -> void:
    symbol = p_symbol
    super._init(p_name)

func apply(canvas: MJCanvas, context: StageContext) -> void:
    canvas.fill(symbol)
    context.snapshot(canvas, name)
