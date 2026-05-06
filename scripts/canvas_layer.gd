extends CanvasLayer

@onready var hearts_display: AnimatedSprite2D = $HeartDisplay

func _ready() -> void:
	update_hearts(0)

func update_hearts(hit_count: int) -> void:
	match hit_count:
		0: hearts_display.play("three_hearts")
		1: hearts_display.play("two_hearts")
		2: hearts_display.play("one_heart")   # pisca automaticamente pelo loop

func show_dead() -> void:
	
	hearts_display.play("dead_hearts")
