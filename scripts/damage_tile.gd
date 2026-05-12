# damage_tile script limpo
extends TileMapLayer

func get_damage_at(pos: Vector2) -> bool:
	var cell = local_to_map(to_local(pos))
	var data = get_cell_tile_data(cell)
	if data:
		return data.get_custom_data("damage")
	return false
