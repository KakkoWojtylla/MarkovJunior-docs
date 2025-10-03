class_name MJOneStage
extends MJRuleStage

var rules: Array[MJRule] = []
var retries: int = 1

func _init(p_rules: Array = [], p_name: String = "one", p_retries: int = 1) -> void:
    for rule in p_rules:
        if rule is MJRule:
            rules.append(rule)
    retries = max(p_retries, 1)
    super._init(p_name)

func add_rule(rule: MJRule) -> MJOneStage:
    if rule is MJRule:
        rules.append(rule)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if rules.is_empty():
        return false
    var attempts := 0
    while attempts < retries:
        attempts += 1
        var matches := _find_matches(canvas)
        if matches.is_empty():
            return false
        var choice := _select_match(matches, context.rng)
        if choice == null:
            return false
        var changed := choice.rule.apply(canvas, choice.position)
        if changed:
            context.snapshot(canvas, name)
            return true
    return false

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

func _select_match(matches: Array, rng: RandomNumberGenerator) -> Variant:
    var total := 0.0
    for entry in matches:
        var rule: MJRule = entry["rule"]
        total += max(rule.weight, 0.0)
    if total <= 0.0:
        return matches[rng.randi_range(0, matches.size() - 1)]
    var roll := rng.randf_range(0.0, total)
    for entry in matches:
        var rule: MJRule = entry["rule"]
        var weight := max(rule.weight, 0.0)
        if roll <= weight:
            return entry
        roll -= weight
    return matches.back()
