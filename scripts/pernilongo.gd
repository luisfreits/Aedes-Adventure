extends CharacterBody2D
class_name Pernilongo

enum PernilongoState {
	walk,
	hurt
}

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var hitbox: Area2D = $Hitbox
@onready var collision_shape: CollisionShape2D = $CollisionShape2D
@onready var wall_detector: RayCast2D = $WallDetector
@onready var ground_detector: RayCast2D = $GroundDetector

const SPEED = 30.0

var status: PernilongoState
var direction := -1

func _ready() -> void:
	go_to_walk_state()

func _physics_process(delta: float) -> void:
	# TRAVA FÍSICA: Se estiver morto, zera a velocidade e sai da função AGORA
	if status == PernilongoState.hurt:
		velocity = Vector2.ZERO
		return 

	if not is_on_floor():
		velocity += get_gravity() * delta

	match status:
		PernilongoState.walk:
			walk_state(delta)
		PernilongoState.hurt: # Adicionado ao match por segurança
			velocity = Vector2.ZERO

	move_and_slide()

func walk_state(_delta: float) -> void:
	# Nova trava: se por acaso entrar aqui morrendo, não move
	if status == PernilongoState.hurt:
		velocity.x = 0
		return

	velocity.x = SPEED * direction
	
	if wall_detector.is_colliding() or not ground_detector.is_colliding():
		scale.x *= -1
		direction *= -1

func go_to_walk_state() -> void:
	status = PernilongoState.walk
	anim.play("walk")
func take_damage():
	if status == PernilongoState.hurt:
		return  # evita chamar duas vezes
	go_to_hurt_state()

func go_to_hurt_state() -> void:
	status = PernilongoState.hurt
	velocity = Vector2.ZERO

	# Para física e colisões imediatamente
	set_physics_process(false)
	collision_shape.set_deferred("disabled", true)
	hitbox.set_deferred("monitoring", false)
	hitbox.set_deferred("monitorable", false)

	if anim.sprite_frames.has_animation("dead"):
		anim.play("dead")

	await get_tree().create_timer(0.5).timeout
	queue_free()
