extends Area2D

@export var dialog_texts: Array[String] = []
@export var offset_position: Vector2 = Vector2(0, -20)

var player_inside: bool = false

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)

func _on_body_entered(body: Node2D) -> void:
	if body is Player:
		player_inside = true

func _on_body_exited(body: Node2D) -> void:
	if body is Player:
		player_inside = false

func _unhandled_input(event: InputEvent) -> void:
	if player_inside and event.is_action_pressed("interact"):
		if not DialogManager.is_showing_dialog:
			DialogManager.start_dialog(dialog_texts, global_position + offset_position)
