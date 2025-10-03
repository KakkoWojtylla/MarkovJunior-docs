class_name MJMarkovStage
extends MJRuleStage

var stages: Array[MJRuleStage] = []
var max_iterations: int = 0

func _init(p_stages: Array = [], p_name: String = "markov", p_max_iterations: int = 0) -> void:
    for stage in p_stages:
        if stage is MJRuleStage:
            stages.append(stage)
    max_iterations = max(p_max_iterations, 0)
    super._init(p_name)

func add_stage(stage: MJRuleStage) -> MJMarkovStage:
    if stage is MJRuleStage:
        stages.append(stage)
    return self

func apply(canvas: MJCanvas, context: StageContext) -> bool:
    var changed := false
    var iterations := 0
    while stages.size() > 0:
        var iteration_changed := false
        for stage in stages:
            if stage.apply(canvas, context):
                changed = true
                iteration_changed = true
                break
        if not iteration_changed:
            break
        iterations += 1
        if max_iterations > 0 and iterations >= max_iterations:
            break
    if changed:
        context.snapshot(canvas, name)
    return changed
