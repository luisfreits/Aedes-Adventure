extends CharacterBody2D

enum PlayerState {
	idle,
	walk,
	jump,
	fall,
	dead
}

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D



@export var max_speed = 180.0
@export var acceleration = 100
@export var deceleration = 100
const JUMP_VELOCITY = -300.0

var status: PlayerState

var jump_count = 0
var max_jump_count = 2
var direction = 0

func move(delta):
	
	var direction := Input.get_axis("left", "right")
	
	if direction:
		velocity.x = move_toward(velocity.x, direction * max_speed, acceleration * delta)
		#quando aperta botao quer chegar na max speed
	else:
		velocity.x = move_toward(velocity.x, 0, deceleration * delta)
	
	if direction < 0:
		anim.flip_h = true
	elif direction > 0:
		anim.flip_h = false

func _ready() -> void:
	go_to_idle_state()

func _physics_process(delta: float) -> void:
	#essa funcao roda a cada frame
	
	if not is_on_floor():
		velocity += get_gravity() * delta
	
	match status:
		PlayerState.idle:
			idle_state(delta)
		PlayerState.walk:
			walk_state(delta)
		PlayerState.jump:
			jump_state(delta)
		PlayerState.fall:
			fall_state(delta)
		PlayerState.dead:
			dead_state(delta)
	move_and_slide()

func go_to_idle_state():
	status = PlayerState.idle
	anim.play("idle")


func go_to_walk_state():
	status = PlayerState.walk
	anim.play("walk")


func go_to_jump_state():
	status = PlayerState.jump
	anim.play("jump")
	velocity.y = JUMP_VELOCITY
	jump_count += 1

func go_to_fall_state():
	status = PlayerState.fall

func go_to_dead_state():
	status = PlayerState.dead
	velocity = Vector2.ZERO

func fall_state(delta):
	move(delta)
	
	if Input.is_action_just_pressed("jump") && can_jump():
		go_to_jump_state()
		return
		
	if is_on_floor():
		jump_count = 0
		if velocity.x == 0:
			go_to_idle_state()
		else:
			go_to_walk_state()
		return

func idle_state(delta):
	move(delta)
	if velocity.x != 0:
		#muda o estado
		go_to_walk_state()
		return
		
	if Input.is_action_just_pressed("jump"):
		go_to_jump_state()
		return

func dead_state(_delta):
	pass

func walk_state(delta):
	move(delta)
	if velocity.x == 0:
		go_to_idle_state()
		return
		
	if Input.is_action_just_pressed("jump"):
		go_to_jump_state()
		return
	
	if !is_on_floor():
		jump_count += 1
		go_to_fall_state()
		return


func jump_state(delta):
	move(delta)
	
	if Input.is_action_just_pressed("jump") && can_jump():
		go_to_jump_state()
		return
		
	if velocity.y > 0:
		go_to_fall_state()
		


func can_jump() -> bool:
	return jump_count < max_jump_count

func temp(delta: float) -> void:
	#add the gravity

		
	# Handle jump.
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY


func _on_hitbox_area_entered(area: Area2D) -> void:
	if velocity.y > 0:
		area.get_parent().take_damage()
		go_to_jump_state()
	else:
		#player morre
		go_to_dead_state()
	return
