extends AnimatableBody2D

@export var shake_time := 0.5
@export var fall_time := 2.0

@onready var collision = $CollisionShape2D
@onready var sprite = $Sprite2D

var triggered := false

func _on_area_2d_body_entered(body: Node) -> void:
	if body is Player and not triggered:
		triggered = true
		await _shake()
		await _fall()
		await _respawn()

func _shake() -> void:
	var original_pos = position
	var elapsed := 0.0
	while elapsed < shake_time:
		position.x = original_pos.x + randf_range(-2.0, 2.0)
		elapsed += get_physics_process_delta_time()
		await get_tree().process_frame
	position = original_pos

func _fall() -> void:
	sprite.visible = false
	collision.disabled = true
	await get_tree().create_timer(fall_time).timeout

func _respawn() -> void:
	sprite.visible = true
	collision.disabled = false
	triggered = false
