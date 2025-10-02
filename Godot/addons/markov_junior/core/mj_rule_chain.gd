class_name MJRuleChain
extends RefCounted

var stages: Array = []

func add_stage(stage: MJRuleStage) -> MJRuleChain:
    stages.append(stage)
    return self

func extend(other: MJRuleChain) -> MJRuleChain:
    for stage in other.stages:
        stages.append(stage)
    return self

func clear() -> void:
    stages.clear()

func run(canvas: MJCanvas, context: MJRuleStage.StageContext) -> void:
    for stage in stages:
        stage.apply(canvas, context)
