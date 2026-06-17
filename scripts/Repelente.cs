using Godot;

public partial class Repelente : Area2D
{
    public override void _Ready()
    {
        // Conecta o sinal de colisão nativo do Area2D
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Verifica se o corpo que entrou na área é o Player
        if (body is Player player)
        {
            player.ZeroHit();

            // Remove o item da cena
            QueueFree();
        }
    }
}