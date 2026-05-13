extends AnimatableBody2D

@export var point_b_offset := Vector2(150.0, 0.0)  # relativo à posição inicial
@export var speed := 60.0
@export var pause_time_a := 1.0
@export var pause_time_b := 1.0

var point_a: Vector2
var point_b: Vector2

func _ready() -> void:
	point_a = global_position
	point_b = global_position + point_b_offset
	_move_loop()

func _move_loop() -> void:
	while true:
		await _move_to(point_b)
		await get_tree().create_timer(pause_time_b).timeout
		await _move_to(point_a)
		await get_tree().create_timer(pause_time_a).timeout

func _move_to(destination: Vector2) -> void:
	while global_position.distance_to(destination) > 1.0:
		var direction = (destination - global_position).normalized()
		move_and_collide(direction * speed * get_physics_process_delta_time())
		await get_tree().physics_frame
	global_position = destination
