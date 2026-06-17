using Godot;

public partial class KillZone : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered (Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            var player = body as Player;
            if (player != null)
            {
                player.GoToDeadState(true);
            }
        }
    }
}