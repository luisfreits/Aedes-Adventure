using Godot;

public partial class PernilongoBoss : CharacterBody2D
//partial e obrigatorio no godot, permite que a classe seja dividida em multiplos arquivos

{
	//Exporta variável pro inspetor do Godot
	[Export] public float Speed = 100.0f;

	public int Direction = 1;
	public bool IsReversed = false;

	private Area2D _hitbox;

	//so roda quando o no entra na cena
	public override void _Ready()
	{
		//getnode busca filho pelo nome
		_hitbox = GetNode<Area2D>("Hitbox");

		_hitbox.AreaEntered += OnHitboxAreaEntered;

		//desativa mask por 1 frame
		_hitbox.CollisionMask = 0;

		ActivateAfterFrame();
	}

	// async função que pode pausar e esperar
	private async void ActivateAfterFrame()
	{
		//espera um frame de fisica
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		_hitbox.CollisionMask = 2;
	}

	//roda a cada frame de fisica
	public override void _PhysicsProcess(double delta)
	{
		//em c# delta é double, precisa converter pra float
		GlobalPosition = new Vector2(
			GlobalPosition.X + Speed * Direction * (float)delta,
			GlobalPosition.Y
		);

		//limites de tela
		if (GlobalPosition.X > GetViewportRect().Size.X + 200)
			QueueFree();

		if (GlobalPosition.X < -200)
			QueueFree();
	}

	//equivale ao func reverse()
	// Dentro de PernilongoBoss.cs

	public void Reverse()
	{
		Direction *= -1;
		Scale = new Vector2(Scale.X * -1, Scale.Y);
		IsReversed = true;

		// O mosquito procura quem está na Layer 8 (Boss)
		_hitbox.CollisionMask = 8;
	}

	private void OnHitboxAreaEntered(Area2D area)
	{
		// Pega o nó pai da área que colidiu
		var owner = area.GetParent();

		if (IsReversed)
		{
			// Se o pai da área tiver o método TakeDamage, aplica o dano
			if (owner.HasMethod("TakeDamage"))
			{
				owner.Call("TakeDamage");
				QueueFree(); // Se destrói após dar dano
			}
		}
		else if (!IsReversed && owner.IsInGroup("Player"))
		{
			owner.Call("TakeHit");
		}
	}
}
