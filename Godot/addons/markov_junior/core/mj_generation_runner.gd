class_name MJGenerationRunner
extends RefCounted

class Options:
    var seed: int = 0
    var capture_frames: bool = false
    var initial_state: PackedStringArray = PackedStringArray()

class Result:
    var final_canvas: MJCanvas
    var frames: Array = []

    func to_rows() -> PackedStringArray:
        return final_canvas.to_rows()

    func to_char_grid() -> Array:
        return final_canvas.to_char_grid()

func run(rule_chain: MJRuleChain, width: int, height: int, fill_char: String = MJCanvas.DEFAULT_FILL, options: Options = Options.new()) -> Result:
    var canvas := MJCanvas.new(width, height, fill_char)
    if options.initial_state.size() > 0:
        canvas.apply_rows(options.initial_state)
    var context := MJRuleStage.StageContext.new()
    context.capture_frames = options.capture_frames
    if options.seed != 0:
        context.rng.seed = options.seed
    else:
        context.rng.randomize()
    if options.capture_frames:
        context.snapshot(canvas, "initial")
    rule_chain.run(canvas, context)
    var result := Result.new()
    result.final_canvas = canvas
    result.frames = context.frames
    return result

func start_session(rule_chain: MJRuleChain, canvas: MJCanvas, options: Options = Options.new()) -> MJGenerationSession:
    return MJGenerationSession.new(rule_chain, canvas, options)

class MJGenerationSession:
    extends RefCounted

    var _rule_chain: MJRuleChain
    var _canvas: MJCanvas
    var _context: MJRuleStage.StageContext
    var _current_stage := 0

    func _init(rule_chain: MJRuleChain, canvas: MJCanvas, options: Options) -> void:
        _rule_chain = rule_chain
        _canvas = canvas.duplicate()
        _context = MJRuleStage.StageContext.new()
        _context.capture_frames = options.capture_frames
        if options.seed != 0:
            _context.rng.seed = options.seed
        else:
            _context.rng.randomize()
        if options.capture_frames:
            _context.snapshot(_canvas, "initial")

    func step() -> bool:
        if _current_stage >= _rule_chain.stages.size():
            return false
        var stage: MJRuleStage = _rule_chain.stages[_current_stage]
        stage.apply(_canvas, _context)
        _current_stage += 1
        return _current_stage < _rule_chain.stages.size()

    func run_to_end() -> void:
        while step():
            pass

    func get_canvas() -> MJCanvas:
        return _canvas

    func get_frames() -> Array:
        return _context.frames
