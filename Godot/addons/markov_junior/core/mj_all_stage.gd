class_name MJAllStage
extends MJRuleStage

var rules: Array[MJRule] = []
var steps: int = 0

func _init(p_rules: Array = [], p_name: String = "all", p_steps: int = 0) -> void:
    for rule in p_rules:
        if rule is MJRule:
            rules.append(rule)
    steps = max(p_steps, 0)
    super._init(p_name)

func add_rule(rule: MJRule) -> MJAllStage:
    if rule is MJRule:
        rules.append(rule)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if rules.is_empty():
        return false
    var changed := false
    var iteration := 0
    while steps == 0 or iteration < steps:
        var matches := _find_matches(canvas)
        if matches.is_empty():
            break
        matches.shuffle()
        var occupied := {}
        var iteration_changed := false
        for entry in matches:
            var rule: MJRule = entry["rule"]
            var position: Vector2i = entry["position"]
            if _overlaps(rule, position, occupied):
                continue
            if rule.apply(canvas, position):
                for cell in rule.affected_cells(position):
                    occupied[cell] = true
                iteration_changed = true
                changed = true
        if not iteration_changed:
            break
        iteration += 1
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

func _overlaps(rule: MJRule, position: Vector2i, occupied: Dictionary) -> bool:
    for cell in rule.affected_cells(position):
        if occupied.has(cell):
            return true
    return false
