extends CharacterBody2D
class_name Albo

enum AlboState {
	walk,
	dash,
	hurt
}

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var hitbox: Area2D = $Hitbox
@onready var collision_shape: CollisionShape2D = $CollisionShape2D
@onready var wall_detector: RayCast2D = $WallDetector
@onready var ground_detector: RayCast2D = $GroundDetector
@onready var player_detector: RayCast2D = $PlayerDetector   # <-- novo

const SPEED = 60.0
const DASH_SPEED = 220.0
const DASH_DURATION = 0.25
const DASH_COOLDOWN = 1.2
const MAX_LIVES = 2

var status: AlboState
var direction := -1
var lives := MAX_LIVES
var can_dash := true

func _ready() -> void:
	# sincroniza o ray com a direção inicial (direction = -1)
	player_detector.target_position.x = abs(player_detector.target_position.x) * direction
	go_to_walk_state()

func _physics_process(delta: float) -> void:
	if status == AlboState.hurt:
		velocity = Vector2.ZERO
		return

	if not is_on_floor():
		velocity += get_gravity() * delta

	match status:
		AlboState.walk:
			walk_state(delta)
		AlboState.dash:
			pass

	move_and_slide()

# ───── Estados ─────

func walk_state(_delta: float) -> void:
	velocity.x = SPEED * direction

	if wall_detector.is_colliding() or not ground_detector.is_colliding():
		scale.x *= -1
		direction *= -1
		player_detector.target_position.x *= -1   # <-- flipa junto com a virada de parede

	_check_player_detection()

func _check_player_detection() -> void:
	if not can_dash:
		return
	# aponta o ray na direção que o Albo está olhando

	if player_detector.is_colliding():
		var hit = player_detector.get_collider()
		if hit is Player:
			go_to_dash_state(hit)

func go_to_walk_state() -> void:
	status = AlboState.walk
	anim.play("walk")

func go_to_dash_state(target: Node2D) -> void:
	if not can_dash:
		return
	can_dash = false
	status = AlboState.dash

	var dash_dir: float = sign(target.global_position.x - global_position.x)
	direction = dash_dir
	velocity.x = DASH_SPEED * dash_dir

	if anim.sprite_frames.has_animation("dash"):
		anim.play("dash")
	else:
		anim.play("walk")

	await get_tree().create_timer(DASH_DURATION).timeout
	velocity.x = 0.0

	# desflipa sprite E o ray junto
	player_detector.target_position.x *= -1

	go_to_walk_state()

	await get_tree().create_timer(DASH_COOLDOWN).timeout
	can_dash = true

# ───── Dano ─────

func take_damage() -> void:
	if status == AlboState.hurt:
		return
	lives -= 1
	if lives <= 0:
		go_to_hurt_state()
	else:
		go_to_hit_state()

func go_to_hit_state() -> void:
	status = AlboState.hurt
	if anim.sprite_frames.has_animation("hit"):
		anim.play("hit")
	await get_tree().create_timer(0.4).timeout
	go_to_walk_state()

func go_to_hurt_state() -> void:
	status = AlboState.hurt
	velocity = Vector2.ZERO
	set_physics_process(false)
	collision_shape.set_deferred("disabled", true)
	hitbox.set_deferred("monitoring", false)
	hitbox.set_deferred("monitorable", false)
	if anim.sprite_frames.has_animation("dead"):
		anim.play("dead")
	await get_tree().create_timer(0.5).timeout
	queue_free()
