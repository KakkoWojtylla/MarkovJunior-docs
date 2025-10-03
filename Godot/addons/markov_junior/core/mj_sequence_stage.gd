class_name MJSequenceStage
extends MJRuleStage

var stages: Array[MJRuleStage] = []

func _init(p_stages: Array = [], p_name: String = "sequence") -> void:
    stages = []
    for stage in p_stages:
        if stage is MJRuleStage:
            stages.append(stage)
    super._init(p_name)

func add_stage(stage: MJRuleStage) -> MJSequenceStage:
    if stage is MJRuleStage:
        stages.append(stage)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    var changed := false
    for stage in stages:
        if stage.apply(canvas, context):
            changed = true
    if changed:
        context.snapshot(canvas, name)
    return changed
