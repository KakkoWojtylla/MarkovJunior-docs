extends EditorPlugin

const _CORE_SCRIPTS := [
    preload("res://addons/markov_junior/core/mj_canvas.gd"),
    preload("res://addons/markov_junior/core/mj_tileset.gd"),
    preload("res://addons/markov_junior/core/mj_rule_stage.gd"),
    preload("res://addons/markov_junior/core/mj_rule.gd"),
    preload("res://addons/markov_junior/core/mj_fill_stage.gd"),
    preload("res://addons/markov_junior/core/mj_stamp_stage.gd"),
    preload("res://addons/markov_junior/core/mj_scatter_stage.gd"),
    preload("res://addons/markov_junior/core/mj_wfc_stage.gd"),
    preload("res://addons/markov_junior/core/mj_rule_chain.gd"),
    preload("res://addons/markov_junior/core/mj_sequence_stage.gd"),
    preload("res://addons/markov_junior/core/mj_markov_stage.gd"),
    preload("res://addons/markov_junior/core/mj_all_stage.gd"),
    preload("res://addons/markov_junior/core/mj_one_stage.gd"),
    preload("res://addons/markov_junior/core/mj_parallel_stage.gd"),
    preload("res://addons/markov_junior/core/mj_map_stage.gd"),
    preload("res://addons/markov_junior/core/mj_path_stage.gd"),
    preload("res://addons/markov_junior/core/mj_convolution_stage.gd"),
    preload("res://addons/markov_junior/core/mj_conv_chain_stage.gd"),
    preload("res://addons/markov_junior/core/mj_generation_runner.gd"),
    preload("res://addons/markov_junior/core/markov_junior.gd")
]

func _enter_tree() -> void:
    for script in _CORE_SCRIPTS:
        script
    print("MarkovJunior addon loaded")

func _exit_tree() -> void:
    print("MarkovJunior addon unloaded")
