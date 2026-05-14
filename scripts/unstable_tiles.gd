extends AnimatableBody2D

@export var shake_time := 1.5
@export var fall_time  := 1.0

@onready var collision: CollisionShape2D = $CollisionShape2D
@onready var sprite: AnimatedSprite2D    = $AnimatedSprite2D  # <-- mudou

var triggered := false

func _ready() -> void:
	sprite.play("idle")

func _on_area_2d_body_entered(body: Node) -> void:
	if body.is_in_group("Player") and not triggered:
		triggered = true
		await _shake()
		await _fall()
		await _respawn()

func _shake() -> void:
	sprite.play("shake")          # ativa animação de shake
	var original_x = position.x
	var elapsed    := 0.0
	while elapsed < shake_time:
		position.x  = original_x + randf_range(-0.3, 0.3)
		elapsed     += get_physics_process_delta_time()
		await get_tree().process_frame
	position.x = original_x

func _fall() -> void:
	sprite.play("fall")           # ativa animação de queda
	collision.disabled = true
	await get_tree().create_timer(fall_time).timeout
	sprite.visible = false        # some depois da animação terminar

func _respawn() -> void:
	sprite.visible     = true
	collision.disabled = false
	triggered          = false
	sprite.play("idle")           # volta ao idle
