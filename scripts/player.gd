extends CharacterBody2D
class_name Player

enum PlayerState { idle, walk, jump, fall, dead, attack }

const JUMP_VELOCITY = -300.0

@onready var animation_player: AnimationPlayer = $AnimationPlayer
@onready var reload_timer: Timer = $ReloadTimer
@onready var attack_box: Area2D = $attack_box
@onready var hitbox: Area2D = $Hitbox
@onready var sprite: Sprite2D = $Sprite2D
@onready var attack_collision: CollisionShape2D = $attack_box/attack_collision

@export var max_speed: float = 180.0
@export var acceleration: float = 500.0
@export var deceleration: float = 400.0

var facing_dir := 1
var attack_box_base_x: float
var status: PlayerState = PlayerState.idle
var jump_count := 0
var max_jump_count := 2
var direction := 0.0
var attack_in_progress := false

func _ready() -> void:
	attack_box_base_x = abs(attack_box.position.x)
	attack_collision.disabled = true
	update_attack_box_position()
	go_to_idle_state()

func _physics_process(delta: float) -> void:
	update_attack_box_position()
	if not is_on_floor():
		velocity += get_gravity() * delta

	match status:
		PlayerState.idle: idle_state(delta)
		PlayerState.walk: walk_state(delta)
		PlayerState.jump: jump_state(delta)
		PlayerState.fall: fall_state(delta)
		PlayerState.attack: attack_state(delta)
		PlayerState.dead: dead_state(delta)

	move_and_slide()

	if position.y > 600 and status != PlayerState.dead:
		go_to_dead_state()

func move(delta: float) -> void:
	direction = Input.get_axis("left", "right")
	var speed_multiplier = 0.5 if status == PlayerState.attack else 1.0
	var target_velocity = direction * max_speed * speed_multiplier
	var current_step = acceleration if direction != 0 else deceleration
	velocity.x = move_toward(velocity.x, target_velocity, current_step * delta)

	if direction != 0:
		facing_dir = 1 if direction > 0 else -1
		sprite.flip_h = (facing_dir == -1)
		update_attack_box_position()

func update_attack_box_position() -> void:
	attack_box.position.x = attack_box_base_x * facing_dir
	attack_box.scale.x = facing_dir

func go_to_idle_state() -> void:
	status = PlayerState.idle
	animation_player.play("idle")

func go_to_walk_state() -> void:
	status = PlayerState.walk
	animation_player.play("walk")

func go_to_jump_state() -> void:
	status = PlayerState.jump
	animation_player.play("jump")
	velocity.y = JUMP_VELOCITY
	jump_count += 1

func go_to_fall_state() -> void:
	status = PlayerState.fall
	animation_player.play("fall")

func go_to_attack_state() -> void:
	if attack_in_progress or status == PlayerState.dead: return
	attack_in_progress = true
	status = PlayerState.attack
	attack_collision.disabled = false
	animation_player.play("attack")
	var attack_anim: Animation = animation_player.get_animation("attack")
	var attack_duration := 0.18
	if attack_anim != null: attack_duration = attack_anim.length
	await get_tree().create_timer(attack_duration).timeout
	attack_collision.disabled = true
	attack_in_progress = false
	if status == PlayerState.dead: return
	if is_on_floor():
		if Input.get_axis("left", "right") != 0: go_to_walk_state()
		else: go_to_idle_state()
	else:
		go_to_fall_state()

func go_to_dead_state() -> void:
	status = PlayerState.dead
	velocity = Vector2.ZERO
	attack_collision.disabled = true
	attack_in_progress = false
	animation_player.play("dead")
	reload_timer.start()

func idle_state(delta: float) -> void:
	move(delta)
	if Input.is_action_just_pressed("attack"): go_to_attack_state()
	elif velocity.x != 0: go_to_walk_state()
	elif Input.is_action_just_pressed("jump"): go_to_jump_state()

func walk_state(delta: float) -> void:
	move(delta)
	if Input.is_action_just_pressed("attack"): go_to_attack_state()
	elif direction == 0: go_to_idle_state()
	elif Input.is_action_just_pressed("jump"): go_to_jump_state()
	elif not is_on_floor(): go_to_fall_state()

func jump_state(delta: float) -> void:
	move(delta)
	if Input.is_action_just_pressed("attack"): go_to_attack_state()
	elif Input.is_action_just_pressed("jump") and can_jump(): go_to_jump_state()
	elif velocity.y > 0: go_to_fall_state()

func fall_state(delta: float) -> void:
	move(delta)
	if Input.is_action_just_pressed("attack"): go_to_attack_state()
	elif Input.is_action_just_pressed("jump") and can_jump(): go_to_jump_state()
	elif is_on_floor():
		jump_count = 0
		if Input.get_axis("left", "right") != 0: go_to_walk_state()
		else: go_to_idle_state()

func attack_state(delta: float) -> void:
	move(delta)
	if Input.is_action_just_pressed("jump") and can_jump(): go_to_jump_state()

func dead_state(_delta: float) -> void: pass

func can_jump() -> bool: return jump_count < max_jump_count

func _get_enemy_from_collider(collider: Node) -> Node:
	if collider.is_in_group("Enemies"): return collider
	if collider.get_parent() and collider.get_parent().is_in_group("Enemies"): return collider.get_parent()
	return null


func _on_hitbox_area_entered(area: Area2D) -> void:
	var enemy = _get_enemy_from_collider(area)
	
	if enemy != null:
		if velocity.y > 0:
			if enemy.has_method("take_damage"):
				enemy.take_damage()
			else:
				enemy.queue_free()
			go_to_jump_state()
		else:
			go_to_dead_state()
	# O elif deve vir ANTES do else, ou ser um IF separado. 
	# Aqui, vamos usar IF para garantir que detecte a área letal:
	if area.is_in_group("LethalArea"):
		go_to_dead_state()

func _on_attack_box_area_entered(area: Area2D) -> void:
	var enemy = _get_enemy_from_collider(area)
	if enemy != null: # Adicionei o TAB aqui que faltava na sua imagem
		if enemy.has_method("take_damage"):
			enemy.take_damage()
		else:
			enemy.queue_free()

func _on_reload_timer_timeout() -> void:
	get_tree().reload_current_scene()
