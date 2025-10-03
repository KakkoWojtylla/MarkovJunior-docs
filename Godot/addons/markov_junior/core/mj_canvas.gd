class_name MJCanvas
extends RefCounted

const DEFAULT_FILL := "?"

var width: int
var height: int
var _data: PackedByteArray

func _init(p_width: int, p_height: int, fill_char: String = DEFAULT_FILL) -> void:
    assert(p_width > 0 and p_height > 0, "Canvas dimensions must be positive")
    width = p_width
    height = p_height
    var code := _char_to_byte(fill_char)
    _data = PackedByteArray()
    _data.resize(width * height)
    for i in _data.size():
        _data[i] = code

func duplicate() -> MJCanvas:
    var copy := MJCanvas.new(width, height)
    copy._data = _data.duplicate()
    return copy

func fill(symbol: String) -> void:
    var code := _char_to_byte(symbol)
    for i in _data.size():
        _data[i] = code

func set_cell(x: int, y: int, symbol: String) -> void:
    if _in_bounds(x, y):
        _data[_index(x, y)] = _char_to_byte(symbol)

func get_cell(x: int, y: int) -> String:
    if _in_bounds(x, y):
        return _byte_to_char(_data[_index(x, y)])
    return DEFAULT_FILL

func apply_rows(rows: PackedStringArray) -> void:
    assert(rows.size() == height, "Row count mismatch")
    for y in rows.size():
        var row := rows[y]
        assert(row.length() == width, "Row width mismatch at line %d" % y)
        for x in width:
            set_cell(x, y, row[x])

func to_rows() -> PackedStringArray:
    var rows := PackedStringArray()
    rows.resize(height)
    for y in height:
        var builder := ""
        for x in width:
            builder += get_cell(x, y)
        rows[y] = builder
    return rows

func to_char_grid() -> Array:
    var grid := []
    for y in height:
        var row := []
        row.resize(width)
        for x in width:
            row[x] = get_cell(x, y)
        grid.append(row)
    return grid

func from_char_grid(grid: Array) -> void:
    assert(grid.size() == height, "Grid height mismatch")
    for y in grid.size():
        var row: Array = grid[y]
        assert(row.size() == width, "Grid width mismatch at line %d" % y)
        for x in row.size():
            var symbol := String(row[x])
            set_cell(x, y, symbol)

func copy_from(other: MJCanvas) -> void:
    assert(other.width == width and other.height == height, "Canvas size mismatch")
    _data = other._data.duplicate()

func as_byte_array() -> PackedByteArray:
    return _data.duplicate()

func load_from_bytes(bytes: PackedByteArray) -> void:
    assert(bytes.size() == _data.size(), "Byte array size mismatch")
    _data = bytes.duplicate()

func count_symbol(symbol: String) -> int:
    var code := _char_to_byte(symbol)
    var total := 0
    for value in _data:
        if value == code:
            total += 1
    return total

func _in_bounds(x: int, y: int) -> bool:
    return x >= 0 and x < width and y >= 0 and y < height

func _index(x: int, y: int) -> int:
    return y * width + x

static func _char_to_byte(symbol: String) -> int:
    assert(symbol.length() == 1, "Symbols must be single characters")
    return symbol.unicode_at(0)

static func _byte_to_char(value: int) -> String:
    return String.chr(value)
