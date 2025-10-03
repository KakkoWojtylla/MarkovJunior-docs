class_name MJConvolutionStage
extends MJRuleStage

class ConvolutionRule:
    var input_symbol: String
    var output_symbol: String
    var contributor_symbols: Array
    var allowed_counts: PackedInt32Array
    var probability: float

    func _init(p_input: String, p_output: String, p_contributors: Variant = null, p_counts: Variant = null, p_probability: float = 1.0) -> void:
        input_symbol = p_input
        output_symbol = p_output
        contributor_symbols = _normalize_symbols(p_contributors)
        allowed_counts = _normalize_counts(p_counts)
        probability = clamp(p_probability, 0.0, 1.0)

    func should_apply(symbol: String, counts: Dictionary, rng: RandomNumberGenerator) -> bool:
        if symbol != input_symbol:
            return false
        if probability < 1.0 and rng.randf() > probability:
            return false
        var total := 0
        if contributor_symbols.is_empty():
            for value in counts.values():
                total += int(value)
        else:
            for contributor in contributor_symbols:
                total += counts.get(contributor, 0)
        if allowed_counts.size() == 0:
            return true
        return allowed_counts.has(total)

    static func _normalize_symbols(data: Variant) -> Array:
        if data == null:
            return []
        if typeof(data) == TYPE_ARRAY:
            var arr := []
            for entry in data:
                arr.append(String(entry))
            return arr
        if typeof(data) == TYPE_PACKED_STRING_ARRAY:
            var arr := []
            for entry in data:
                arr.append(entry)
            return arr
        return [String(data)]

    static func _normalize_counts(data: Variant) -> PackedInt32Array:
        if data == null:
            return PackedInt32Array()
        if typeof(data) == TYPE_PACKED_INT32_ARRAY:
            return data.duplicate()
        var counts := PackedInt32Array()
        if typeof(data) == TYPE_ARRAY:
            for entry in data:
                counts.append(int(entry))
        else:
            counts.append(int(data))
        return counts

var rules: Array[ConvolutionRule] = []
var neighborhood: String = "moore"
var periodic: bool = false

func _init(p_rules: Array = [], p_name: String = "convolution", p_neighborhood: String = "moore", p_periodic: bool = false) -> void:
    for rule in p_rules:
        if rule is ConvolutionRule:
            rules.append(rule)
    neighborhood = p_neighborhood
    periodic = p_periodic
    super._init(p_name)

func add_rule(rule: ConvolutionRule) -> MJConvolutionStage:
    if rule is ConvolutionRule:
        rules.append(rule)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if rules.is_empty():
        return false
    var offsets := _kernel_offsets()
    var counts := []
    counts.resize(canvas.width * canvas.height)
    for y in canvas.height:
        for x in canvas.width:
            var index := x + y * canvas.width
            counts[index] = _count_neighbors(canvas, x, y, offsets)
    var changed := false
    for y in canvas.height:
        for x in canvas.width:
            var index := x + y * canvas.width
            var current := canvas.get_cell(x, y)
            for rule in rules:
                if rule.should_apply(current, counts[index], context.rng):
                    if current != rule.output_symbol:
                        canvas.set_cell(x, y, rule.output_symbol)
                        changed = true
                    break
    if changed:
        context.snapshot(canvas, name)
    return changed

func _kernel_offsets() -> Array:
    match neighborhood.to_lower():
        "vonneumann":
            return [Vector2i(0, -1), Vector2i(1, 0), Vector2i(0, 1), Vector2i(-1, 0)]
        "moore":
            return [
                Vector2i(-1, -1), Vector2i(0, -1), Vector2i(1, -1),
                Vector2i(-1, 0), Vector2i(1, 0),
                Vector2i(-1, 1), Vector2i(0, 1), Vector2i(1, 1)
            ]
        _:
            push_warning("Unknown neighborhood %s, defaulting to Moore" % neighborhood)
            return _kernel_offsets_moore()

func _kernel_offsets_moore() -> Array:
    return [
        Vector2i(-1, -1), Vector2i(0, -1), Vector2i(1, -1),
        Vector2i(-1, 0), Vector2i(1, 0),
        Vector2i(-1, 1), Vector2i(0, 1), Vector2i(1, 1)
    ]

func _count_neighbors(canvas: MJCanvas, x: int, y: int, offsets: Array) -> Dictionary:
    var tally := {}
    for offset in offsets:
        var nx := x + offset.x
        var ny := y + offset.y
        if periodic:
            nx = (nx + canvas.width) % canvas.width
            ny = (ny + canvas.height) % canvas.height
        elif nx < 0 or ny < 0 or nx >= canvas.width or ny >= canvas.height:
            continue
        var symbol := canvas.get_cell(nx, ny)
        tally[symbol] = tally.get(symbol, 0) + 1
    return tally
