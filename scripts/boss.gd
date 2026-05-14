extends CharacterBody2D
class_name Boss
enum BossState {
	walk,
	dash,
	hurt
}
@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var hitbox: Area2D = $Hitbox
@onready var collision_shape: CollisionShape2D = $CollisionShape2D
@onready var wall_detector: RayCast2D = $WallDetector
@onready var ground_detector: RayCast2D = $GroundDetector
@onready var player_detector: RayCast2D = $PlayerDetector
const SPEED = 60.0
const DASH_SPEED = 120.0
const DASH_DURATION = 0.25
const DASH_COOLDOWN = 1.2
const MAX_LIVES = 8
var status: BossState
var direction := -1
var lives := MAX_LIVES
var can_dash := true
func _ready() -> void:
	go_to_walk_state()
func _physics_process(delta: float) -> void:
	if status == BossState.hurt:
		velocity = Vector2.ZERO
		return
	if not is_on_floor():
		velocity += get_gravity() * delta
	match status:
		BossState.walk:
			walk_state(delta)
		BossState.dash:
			pass
	move_and_slide()
# ───── Flip ─────
func flip() -> void:
	direction *= -1
	scale.x *= -1
# ───── Estados ─────
var was_blocked := false
func walk_state(_delta: float) -> void:
	velocity.x = SPEED * direction
	var blocked := wall_detector.is_colliding() or not ground_detector.is_colliding()
	if blocked and not was_blocked:
		flip()
	was_blocked = blocked
	_check_player_detection()
func _check_player_detection() -> void:
	if not can_dash:
		return
	if player_detector.is_colliding():
		var hit = player_detector.get_collider()
		if hit is Player:
			go_to_dash_state(hit)
func go_to_walk_state() -> void:
	status = BossState.walk
	anim.play("walk")
func go_to_dash_state(target: Node2D) -> void:
	if not can_dash:
		return
	can_dash = false
	status = BossState.dash
	var dash_dir: float = sign(target.global_position.x - global_position.x)
	if dash_dir != direction:
		flip()
	velocity.x = DASH_SPEED * dash_dir
	if anim.sprite_frames.has_animation("dash"):
		anim.play("dash")
	else:
		anim.play("walk")
	await get_tree().create_timer(DASH_DURATION).timeout
	velocity.x = 0.0
	if status != BossState.hurt:
		go_to_walk_state()
	await get_tree().create_timer(DASH_COOLDOWN).timeout
	if is_instance_valid(self) and status != BossState.hurt:
		can_dash = true
# ───── Dano ─────
func take_damage() -> void:
	if status == BossState.hurt:
		return
	lives -= 1
	if lives <= 0:
		go_to_hurt_state()
	else:
		go_to_hit_state()
func go_to_hit_state() -> void:
	status = BossState.hurt
	if anim.sprite_frames.has_animation("hit"):
		anim.play("hit")
	await get_tree().create_timer(0.4).timeout
	go_to_walk_state()
func go_to_hurt_state() -> void:
	status = BossState.hurt
	velocity = Vector2.ZERO
	set_physics_process(false)
	collision_shape.set_deferred("disabled", true)
	hitbox.set_deferred("monitoring", false)
	hitbox.set_deferred("monitorable", false)
	if anim.sprite_frames.has_animation("dead"):
		anim.play("dead")
	await get_tree().create_timer(0.5).timeout
	queue_free()
