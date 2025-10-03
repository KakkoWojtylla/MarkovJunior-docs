class_name MJPathStage
extends MJRuleStage

var start_symbols: Array = []
var end_symbols: Array = []
var path_symbol: String = "*"
var passable: Array = []
var allow_diagonal: bool = false
var keep_endpoints: bool = true

func _init(p_start: Variant, p_end: Variant, p_path_symbol: String = "*", p_name: String = "path") -> void:
    start_symbols = _normalize_symbol_list(p_start)
    end_symbols = _normalize_symbol_list(p_end)
    path_symbol = p_path_symbol
    super._init(p_name)

func set_passable(symbols: Variant) -> MJPathStage:
    passable = _normalize_symbol_list(symbols)
    return self

func set_allow_diagonal(value: bool) -> MJPathStage:
    allow_diagonal = value
    return self

func set_keep_endpoints(value: bool) -> MJPathStage:
    keep_endpoints = value
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    if start_symbols.is_empty() or end_symbols.is_empty():
        return false
    var starts := _locate(canvas, start_symbols)
    var ends := _locate(canvas, end_symbols)
    if starts.is_empty() or ends.is_empty():
        return false
    var target := _find_path(canvas, starts, ends)
    if target.size() == 0:
        return false
    var changed := false
    for i in range(1, target.size() - 1):
        var cell: Vector2i = target[i]
        if canvas.get_cell(cell.x, cell.y) != path_symbol:
            canvas.set_cell(cell.x, cell.y, path_symbol)
            changed = true
    if not keep_endpoints and target.size() >= 2:
        var first := target.front()
        var last := target.back()
        if canvas.get_cell(first.x, first.y) != path_symbol:
            canvas.set_cell(first.x, first.y, path_symbol)
            changed = true
        if canvas.get_cell(last.x, last.y) != path_symbol:
            canvas.set_cell(last.x, last.y, path_symbol)
            changed = true
    if changed:
        context.snapshot(canvas, name)
    return changed

func _locate(canvas: MJCanvas, symbols: Array) -> Array:
    var result := []
    for y in canvas.height:
        for x in canvas.width:
            var cell := canvas.get_cell(x, y)
            if symbols.has(cell):
                result.append(Vector2i(x, y))
    return result

func _find_path(canvas: MJCanvas, starts: Array, ends: Array) -> Array:
    var end_lookup := {}
    for end in ends:
        end_lookup[end] = true
    var queue := []
    var came_from := {}
    for start in starts:
        queue.append(start)
        came_from[start] = null
    var directions := [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)]
    if allow_diagonal:
        directions += [Vector2i(1, 1), Vector2i(-1, -1), Vector2i(1, -1), Vector2i(-1, 1)]
    while not queue.is_empty():
        var current: Vector2i = queue.pop_front()
        if end_lookup.has(current):
            return _reconstruct_path(current, came_from)
        for dir in directions:
            var next := current + dir
            if next.x < 0 or next.y < 0 or next.x >= canvas.width or next.y >= canvas.height:
                continue
            if came_from.has(next):
                continue
            var symbol := canvas.get_cell(next.x, next.y)
            if passable.size() > 0 and not passable.has(symbol) and not end_lookup.has(next):
                continue
            queue.append(next)
            came_from[next] = current
    return []

func _reconstruct_path(target: Vector2i, came_from: Dictionary) -> Array:
    var path := []
    var current := target
    while current != null:
        path.push_front(current)
        current = came_from.get(current, null)
    return path

static func _normalize_symbol_list(value: Variant) -> Array:
    var list := []
    if value == null:
        return list
    if typeof(value) == TYPE_STRING:
        list.append(value)
    elif typeof(value) == TYPE_ARRAY:
        for entry in value:
            list.append(String(entry))
    elif typeof(value) == TYPE_PACKED_STRING_ARRAY:
        for entry in value:
            list.append(entry)
    else:
        list.append(String(value))
    return list
