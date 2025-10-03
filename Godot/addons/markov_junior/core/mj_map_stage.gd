class_name MJMapStage
extends MJRuleStage

var rules: Array[MJRule] = []
var fill_symbol: String = MJCanvas.DEFAULT_FILL
var wrap: bool = true
var children: Array[MJRuleStage] = []

func _init(p_rules: Array = [], p_name: String = "map", p_fill: String = MJCanvas.DEFAULT_FILL, p_wrap: bool = true, p_children: Array = []) -> void:
    for rule in p_rules:
        if rule is MJRule:
            rules.append(rule)
    fill_symbol = p_fill
    wrap = p_wrap
    for child in p_children:
        if child is MJRuleStage:
            children.append(child)
    super._init(p_name)

func add_rule(rule: MJRule) -> MJMapStage:
    if rule is MJRule:
        rules.append(rule)
    return self

func add_child(stage: MJRuleStage) -> MJMapStage:
    if stage is MJRuleStage:
        children.append(stage)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if rules.is_empty():
        return false
    var target := MJCanvas.new(canvas.width, canvas.height, fill_symbol)
    var changed := false
    for rule in rules:
        var width := rule.get_width()
        var height := rule.get_height()
        for y in canvas.height:
            for x in canvas.width:
                var pos := Vector2i(x, y)
                if _matches(rule, canvas, pos):
                    if _write(rule, target, pos):
                        changed = true
    for child in children:
        if child.apply(target, context):
            changed = true
    if changed:
        canvas.copy_from(target)
        context.snapshot(canvas, name)
    return changed

func _matches(rule: MJRule, canvas: MJCanvas, position: Vector2i) -> bool:
    var width := rule.get_width()
    var height := rule.get_height()
    for dy in height:
        for dx in width:
            var sx := position.x + dx
            var sy := position.y + dy
            if wrap:
                sx = (sx + canvas.width) % canvas.width
                sy = (sy + canvas.height) % canvas.height
            elif sx < 0 or sy < 0 or sx >= canvas.width or sy >= canvas.height:
                return false
            var row := rule.input[dy]
            var expected := row[dx]
            if expected == MJRule.WILDCARD:
                continue
            if canvas.get_cell(sx, sy) != expected:
                return false
    return true

func _write(rule: MJRule, canvas: MJCanvas, position: Vector2i) -> bool:
    if not wrap:
        return rule.apply(canvas, position)
    var changed := false
    for dy in range(rule.get_height()):
        var row := rule.output[dy]
        for dx in range(rule.get_width()):
            var symbol := row[dx]
            if symbol == MJRule.WILDCARD:
                continue
            var cx := (position.x + dx + canvas.width) % canvas.width
            var cy := (position.y + dy + canvas.height) % canvas.height
            if canvas.get_cell(cx, cy) != symbol:
                canvas.set_cell(cx, cy, symbol)
                changed = true
    return changed
