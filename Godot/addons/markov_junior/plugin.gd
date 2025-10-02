extends EditorPlugin

const _CORE_SCRIPTS := [
    preload("res://addons/markov_junior/core/mj_canvas.gd"),
    preload("res://addons/markov_junior/core/mj_tileset.gd"),
    preload("res://addons/markov_junior/core/mj_rule_stage.gd"),
    preload("res://addons/markov_junior/core/mj_fill_stage.gd"),
    preload("res://addons/markov_junior/core/mj_stamp_stage.gd"),
    preload("res://addons/markov_junior/core/mj_scatter_stage.gd"),
    preload("res://addons/markov_junior/core/mj_wfc_stage.gd"),
    preload("res://addons/markov_junior/core/mj_rule_chain.gd"),
    preload("res://addons/markov_junior/core/mj_generation_runner.gd"),
    preload("res://addons/markov_junior/core/markov_junior.gd")
]

func _enter_tree() -> void:
    for script in _CORE_SCRIPTS:
        script
    print("MarkovJunior addon loaded")

func _exit_tree() -> void:
    print("MarkovJunior addon unloaded")
