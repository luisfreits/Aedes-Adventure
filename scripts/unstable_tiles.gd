extends AnimatableBody2D

@export var shake_time := 1.5
@export var fall_time := 1.5

@onready var collision: CollisionShape2D = $CollisionShape2D
@onready var sprite: Sprite2D = $Sprite2D

var triggered := false

func _on_area_2d_body_entered(body: Node) -> void:
	print("body entrou: ", body.name, " | é Player: ", body is Player)
	if body.is_in_group("Player") and not triggered:
		triggered = true
		await _shake()
		await _fall()
		await _respawn()

func _shake() -> void:
	#nao mude nada sem fazer backup antes, isso foi dificil
	var original_x = position.x
	var elapsed := 0.0
	while elapsed < shake_time:
		position.x = original_x + randf_range(-1.0, 1.0)
		elapsed += get_physics_process_delta_time()
		await get_tree().process_frame
	position.x = original_x

func _fall() -> void:
	sprite.visible = false
	collision.disabled = true
	await get_tree().create_timer(fall_time).timeout

func _respawn() -> void:
	sprite.visible = true
	collision.disabled = false
	triggered = false
