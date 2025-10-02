class_name MJWFCStage
extends MJRuleStage

const DIRECTIONS := MJTileSet.DIRECTIONS
var tile_set: MJTileSet
var backtrack_limit: int = 128
var wildcard: String = MJCanvas.DEFAULT_FILL

func _init(p_tile_set: MJTileSet, p_name: String = "wfc", p_backtrack_limit: int = 128, p_wildcard: String = MJCanvas.DEFAULT_FILL) -> void:
    assert(p_tile_set != null, "Tile set must be provided")
    tile_set = p_tile_set
    backtrack_limit = p_backtrack_limit
    wildcard = p_wildcard
    super._init(p_name)

func apply(canvas: MJCanvas, context: StageContext) -> void:
    var solver := _build_solver(canvas, context)
    var ok := solver.solve()
    if not ok:
        push_warning("WFC stage %s failed after exhausting backtracking" % name)
    solver.commit(canvas)
    context.snapshot(canvas, name)

class WFCSolver:
    var tile_set: MJTileSet
    var width: int
    var height: int
    var possibilities: Array = []
    var rng: RandomNumberGenerator
    var backtrack_limit: int
    var wildcard: String

    func _init(p_tile_set: MJTileSet, canvas: MJCanvas, rng: RandomNumberGenerator, p_backtrack_limit: int, p_wildcard: String) -> void:
        tile_set = p_tile_set
        width = canvas.width
        height = canvas.height
        self.rng = rng
        backtrack_limit = p_backtrack_limit
        wildcard = p_wildcard
        var all_indices := tile_set.get_all_indices()
        for y in height:
            var row := []
            for x in width:
                var symbol := canvas.get_cell(x, y)
                if tile_set.has_symbol(symbol) and symbol != wildcard:
                    var idx := tile_set.symbols.find(symbol)
                    row.append(PackedInt32Array([idx]))
                else:
                    row.append(all_indices.duplicate())
            possibilities.append(row)

    func solve() -> bool:
        var stack: Array = []
        var steps := 0
        while steps <= backtrack_limit:
            var target := _lowest_entropy_cell()
            if target == null:
                return true
            var pos: Vector2i = target
            var x: int = pos.x
            var y: int = pos.y
            var options: PackedInt32Array = possibilities[y][x]
            if options.size() == 0:
                if not _rewind(stack):
                    return false
                steps += 1
                continue
            var branch := {
                "snapshot": _snapshot(),
                "x": x,
                "y": y,
                "options": PackedInt32Array(options)
            }
            var choice := _pick(options)
            branch["chosen"] = choice
            stack.append(branch)
            possibilities[y][x] = PackedInt32Array([choice])
            if not _propagate(Vector2i(x, y)):
                if not _rewind(stack):
                    return false
                steps += 1
                continue
            steps += 1
        return false

    func commit(canvas: MJCanvas) -> void:
        for y in height:
            for x in width:
                var options: PackedInt32Array = possibilities[y][x]
                if options.size() == 0:
                    continue
                canvas.set_cell(x, y, tile_set.get_symbol(options[0]))

    func _lowest_entropy_cell() -> Variant:
        var best_entropy := INF
        var best: Variant = null
        for y in height:
            for x in width:
                var options: PackedInt32Array = possibilities[y][x]
                if options.size() <= 1:
                    continue
                var entropy := float(options.size()) + rng.randf() * 0.01
                if entropy < best_entropy:
                    best_entropy = entropy
                    best = Vector2i(x, y)
        return best

    func _pick(options: PackedInt32Array) -> int:
        var total := 0.0
        for option in options:
            total += tile_set.get_weight(option)
        var roll := rng.randf_range(0.0, total)
        for option in options:
            var weight := tile_set.get_weight(option)
            if roll <= weight:
                return option
            roll -= weight
        return options[options.size() - 1]

    func _propagate(start: Vector2i) -> bool:
        var queue := [start]
        while queue.size() > 0:
            var cell: Vector2i = queue.pop_front()
            var x := cell.x
            var y := cell.y
            for dir_key in DIRECTIONS.keys():
                var offset: Vector2i = DIRECTIONS[dir_key]
                var nx := x + offset.x
                var ny := y + offset.y
                if nx < 0 or nx >= width or ny < 0 or ny >= height:
                    continue
                if _restrict_neighbor(Vector2i(nx, ny), dir_key, possibilities[y][x]):
                    if possibilities[ny][nx].size() == 0:
                        return false
                    queue.append(Vector2i(nx, ny))
        return true

    func _restrict_neighbor(cell: Vector2i, direction: String, source: PackedInt32Array) -> bool:
        var allowed := []
        var allowed_lookup := {}
        for option in source:
            var symbol := tile_set.get_symbol(option)
            for neighbor_symbol in tile_set.get_neighbors(symbol, direction):
                if not allowed_lookup.has(neighbor_symbol):
                    allowed_lookup[neighbor_symbol] = true
                    allowed.append(tile_set.symbols.find(neighbor_symbol))
        var neighbor_options: PackedInt32Array = possibilities[cell.y][cell.x]
        var changed := false
        for option_index in range(neighbor_options.size() - 1, -1, -1):
            var value := neighbor_options[option_index]
            if not allowed.has(value):
                neighbor_options.remove_at(option_index)
                changed = true
        if changed:
            possibilities[cell.y][cell.x] = neighbor_options
        return changed

    func _snapshot() -> Array:
        var snap := []
        for y in height:
            var row := []
            for x in width:
                row.append(PackedInt32Array(possibilities[y][x]))
            snap.append(row)
        return snap

    func _restore(snap: Array) -> void:
        possibilities.clear()
        for y in snap.size():
            var row := []
            for x in snap[y].size():
                row.append(PackedInt32Array(snap[y][x]))
            possibilities.append(row)

    func _rewind(stack: Array) -> bool:
        while stack.size() > 0:
            var branch := stack.pop_back()
            _restore(branch["snapshot"])
            var remaining: PackedInt32Array = branch["options"]
            remaining.remove_at(remaining.find(branch["chosen"]))
            if remaining.size() == 0:
                continue
            var choice := _pick(remaining)
            branch["chosen"] = choice
            branch["options"] = PackedInt32Array(remaining)
            branch["snapshot"] = _snapshot()
            stack.append(branch)
            possibilities[branch["y"]][branch["x"]] = PackedInt32Array([choice])
            if _propagate(Vector2i(branch["x"], branch["y"])):
                return true
        return false

func _build_solver(canvas: MJCanvas, context: StageContext) -> WFCSolver:
    return WFCSolver.new(tile_set, canvas, context.rng, backtrack_limit, wildcard)
