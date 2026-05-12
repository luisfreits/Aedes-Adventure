extends TileMapLayer

@export var shake_time := 0.5
@export var fall_time := 2.0

# guarda o estado de cada célula instável
var saved_cells: Dictionary = {}
var triggered_cells: Dictionary = {}

func _physics_process(delta: float) -> void:
	var player = get_node_or_null("../Player")
	if player == null:
		return

	var feet_pos = player.global_position + Vector2(0, 8)
	var cell = local_to_map(to_local(feet_pos))
	var on_floor = player.is_on_floor()
	var unstable = is_unstable_tile(cell)

	print("cell: ", cell, " | source_id: ", get_cell_source_id(cell), " | on_floor: ", on_floor, " | unstable: ", unstable)

	if unstable and on_floor:
		if not triggered_cells.has(cell):
			triggered_cells[cell] = true
			trigger_tile(cell)

	print("cell: ", cell, " | source_id: ", get_cell_source_id(cell), " | on_floor: ", on_floor, " | unstable: ", unstable)

	if unstable and on_floor:
		if not triggered_cells.has(cell):
			triggered_cells[cell] = true
			trigger_tile(cell)

func is_unstable_tile(cell: Vector2i) -> bool:
	var data = get_cell_tile_data(cell)
	print("source_id: ", get_cell_source_id(cell), " | atlas: ", get_cell_atlas_coords(cell), " | data: ", data != null)
	if data:
		print("unstable value: ", data.get_custom_data("unstable"))
		return data.get_custom_data("unstable")
	return false

func trigger_tile(cell: Vector2i) -> void:
	await _shake_tile(cell)
	await _hide_tile(cell)
	await _respawn_tile(cell)

func _shake_tile(cell: Vector2i) -> void:
	var elapsed := 0.0
	while elapsed < shake_time:
		# efeito visual de tremida, move o tile no tileset não é possível,
		# então chacoalha o tilemap inteiro levemente
		position.x = randf_range(-1.0, 1.0)
		elapsed += get_physics_process_delta_time()
		await get_tree().process_frame
	position.x = 0.0

func _hide_tile(cell: Vector2i) -> void:
	# salva os dados reais do tile antes de apagar
	saved_cells[cell] = {
		"source_id": get_cell_source_id(cell),
		"atlas_coords": get_cell_atlas_coords(cell)
	}
	erase_cell(cell)
	await get_tree().create_timer(fall_time).timeout

func _respawn_tile(cell: Vector2i) -> void:
	if saved_cells.has(cell):
		var data = saved_cells[cell]
		set_cell(cell, data["source_id"], data["atlas_coords"])
		saved_cells.erase(cell)
	triggered_cells.erase(cell)
