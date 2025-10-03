class_name MJParallelStage
extends MJRuleStage

var rules: Array[MJRule] = []
var iterations: int = 1

func _init(p_rules: Array = [], p_name: String = "parallel", p_iterations: int = 1) -> void:
    for rule in p_rules:
        if rule is MJRule:
            rules.append(rule)
    iterations = max(p_iterations, 1)
    super._init(p_name)

func add_rule(rule: MJRule) -> MJParallelStage:
    if rule is MJRule:
        rules.append(rule)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if rules.is_empty():
        return false
    var changed := false
    for i in iterations:
        var matches := _find_matches(canvas)
        if matches.is_empty():
            break
        matches.shuffle()
        for entry in matches:
            var rule: MJRule = entry["rule"]
            var position: Vector2i = entry["position"]
            if context.rng.randf() > rule.probability:
                continue
            if rule.apply(canvas, position):
                changed = true
    if changed:
        context.snapshot(canvas, name)
    return changed

func _find_matches(canvas: MJCanvas) -> Array:
    var matches := []
    for rule in rules:
        var width := rule.get_width()
        var height := rule.get_height()
        if width == 0 or height == 0 or width > canvas.width or height > canvas.height:
            continue
        for y in range(0, canvas.height - height + 1):
            for x in range(0, canvas.width - width + 1):
                var pos := Vector2i(x, y)
                if rule.matches(canvas, pos):
                    matches.append({"rule": rule, "position": pos})
    return matches
