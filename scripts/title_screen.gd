extends Control

#em producao, no momento nao funciona!!!!

const SAVE_PATH = "user://savegame.save"

func _on_new_btn_pressed() -> void:
	# apaga o save existente e começa do zero
	if FileAccess.file_exists(SAVE_PATH):
		DirAccess.remove_absolute(SAVE_PATH)
	get_tree().change_scene_to_file("res://scene/Fase_1-1.tscn")

func _on_load_btn_pressed() -> void:
	if FileAccess.file_exists(SAVE_PATH):
		# carrega a cena salva
		var file = FileAccess.open(SAVE_PATH, FileAccess.READ)
		var scene_path = file.get_line()
		file.close()
		get_tree().change_scene_to_file(scene_path)
	# se não tiver save, não faz nada, fica na tela inicial

func _on_credits_btn_pressed() -> void:
	get_tree().change_scene_to_file("res://scene/creditos.tscn")

func _on_quit_btn_pressed() -> void:
	get_tree().quit()
