extends Camera2D

var target: Node2D

func _ready() -> void:
	get_target()
	#executa assim que carregar a arvore

func _process(_delta: float) -> void:
	position = target.position
	#trocando posição da camera pela do player a cada frame
	

func get_target():
	var nodes = get_tree().get_nodes_in_group("Player")
	if nodes.size() == 0:
		push_error("Player não encontrado")
		return
		#se nao encontrar player retorna
		
	target = nodes[0] 
