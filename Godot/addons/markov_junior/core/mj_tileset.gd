class_name MJTileSet
extends RefCounted

const DIRECTIONS := {
    "north": Vector2i(0, -1),
    "south": Vector2i(0, 1),
    "west": Vector2i(-1, 0),
    "east": Vector2i(1, 0)
}

var symbols: PackedStringArray = PackedStringArray()
var weights: PackedFloat32Array = PackedFloat32Array()
var adjacency := {}

func add_tile(symbol: String, weight: float = 1.0) -> MJTileSet:
    assert(symbol.length() == 1, "Tiles must be single characters")
    if symbols.has(symbol):
        return self
    symbols.append(symbol)
    weights.append(weight)
    adjacency[symbol] = {}
    for dir_key in DIRECTIONS.keys():
        adjacency[symbol][dir_key] = []
    return self

func set_weight(symbol: String, weight: float) -> MJTileSet:
    var index := symbols.find(symbol)
    assert(index != -1, "Tile %s is not registered" % symbol)
    weights[index] = weight
    return self

func add_adjacency(origin: String, neighbor: String, direction: String = "any") -> MJTileSet:
    assert(symbols.has(origin) and symbols.has(neighbor), "Tiles must be registered before adding adjacency")
    if direction == "any":
        for dir_key in DIRECTIONS.keys():
            _add_single_adjacency(origin, neighbor, dir_key)
    else:
        assert(DIRECTIONS.has(direction), "Unknown direction %s" % direction)
        _add_single_adjacency(origin, neighbor, direction)
    return self

func get_neighbors(symbol: String, direction: String) -> Array:
    if not adjacency.has(symbol):
        return []
    return adjacency[symbol][direction].duplicate()

func get_all_indices() -> PackedInt32Array:
    var arr := PackedInt32Array()
    arr.resize(symbols.size())
    for i in symbols.size():
        arr[i] = i
    return arr

func get_weight(index: int) -> float:
    return weights[index]

func get_symbol(index: int) -> String:
    return symbols[index]

func has_symbol(symbol: String) -> bool:
    return symbols.has(symbol)

func _add_single_adjacency(origin: String, neighbor: String, direction: String) -> void:
    var list: Array = adjacency[origin][direction]
    if not list.has(neighbor):
        list.append(neighbor)
