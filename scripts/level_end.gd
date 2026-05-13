extends Node2D

const SAVE_PATH = "user://savegame.save"

@export var next_level = ""
#informa qual proxima fase individualmente
@export var is_checkpoint := false 
# marca no Inspector quando quiser salvar

func _on_body_entered(_body: Node2D) -> void:
	call_deferred("load_next_scene")
	#carrega a fisica e evita bugs
	
func load_next_scene():
	if is_checkpoint:
		_save(next_level)
	get_tree().change_scene_to_file("res://scene/" + next_level + ".tscn") #troca a cena

func _save(scene_name: String) -> void:
	var file = FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	file.store_line("res://scene/" + scene_name + ".tscn")
	file.close()
