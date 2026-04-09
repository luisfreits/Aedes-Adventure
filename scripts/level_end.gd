extends Node2D

@export var next_level = ""
#informa qual proxima fase individualmente

func _on_body_entered(_body: Node2D) -> void:
	call_deferred("load_next_scene")
	#carrega a fisica e evita bugs
	
func load_next_scene():
	get_tree().change_scene_to_file("res://scene/" + next_level + ".tscn") #troca a cena
