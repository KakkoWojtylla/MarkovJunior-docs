class_name MJRuleStage
extends RefCounted

class StageContext:
    var rng := RandomNumberGenerator.new()
    var capture_frames: bool = false
    var frames: Array = []

    func snapshot(canvas: MJCanvas, label: String = "") -> void:
        if not capture_frames:
            return
        var record := {
            "label": label,
            "step": frames.size(),
            "canvas": canvas.duplicate()
        }
        frames.append(record)

var name: String

func _init(p_name: String = "") -> void:
    name = p_name

func apply(canvas: MJCanvas, context: StageContext) -> void:
    push_error("apply() not implemented for %s" % [get_class()])
