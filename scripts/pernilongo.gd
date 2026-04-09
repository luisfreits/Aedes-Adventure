extends CharacterBody2D

enum PernilongoState {
	walk,
	hurt
}

@onready var anim: AnimatedSprite2D = $AnimatedSprite2D
@onready var hitbox: Area2D = $Hitbox
@onready var wall_detector: RayCast2D = $WallDetector
@onready var ground_detector: RayCast2D = $GroundDetector

const SPEED = 30.0
const JUMP_VELOCITY = -400.0

var status: PernilongoState

var direction = -1

func _ready() -> void:
	go_to_walk_state()

func _physics_process(delta: float) -> void:
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta
	match status:
		PernilongoState.walk:
			walk_state(delta)
		PernilongoState.hurt:
			hurt_state(delta)
	move_and_slide()
	
func go_to_walk_state():
	status = PernilongoState.walk
	anim.play("walk")
	
func go_to_hurt_state():
	status = PernilongoState.hurt
	anim.play("dead")
	hitbox.process_mode = Node.PROCESS_MODE_DISABLED
	velocity = Vector2.ZERO
	
	
func hurt_state(delta):
	pass
	
	
func walk_state(delta):
	velocity.x = SPEED * direction
	
	if wall_detector.is_colliding():
		scale.x *= -1
		direction *= -1
		#faz os npc virar
	
	if not ground_detector.is_colliding():
		scale.x *= -1
		direction *= -1
		#faz os npc n cair
	
func take_damage():
	go_to_hurt_state()
