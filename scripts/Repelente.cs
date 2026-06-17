using Godot;

public partial class Repelente : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		// reseta as vidas pro máximo
		GameManager.ResetLives();

		// atualiza o display de vidas
		var livesDisplay = GetTree().Root.GetNode<LivesDisplay>("LivesDisplay");
		livesDisplay?.UpdateDisplay();

		// reseta os corações do HUD pra 3
		var hud = GetTree().Root.GetNode<Hud>("Hud");
		hud?.UpdateHearts(0);

		QueueFree();
	}
}