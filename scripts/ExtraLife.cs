using Godot;

public partial class ExtraLife : Area2D
{
    private AnimatedSprite2D _sprite;
    public override void _Ready()
    {
        // conecta o sinal de entrada de corpo
        BodyEntered += OnBodyEntered;
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _sprite.Play("default");
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;

        // só adiciona se não estiver no máximo
        if (GameManager.Lives < GameManager.MaxLives)
        {
            GameManager.Lives++;
            GD.Print($"Vida aumentada para: {GameManager.Lives}"); // Confirmação no console

            // Busca o nó e tenta converter
            var node = GetTree().GetFirstNodeInGroup("LivesDisplay");
            var livesDisplay = node as LivesDisplay;

            if (livesDisplay != null)
            {
                livesDisplay.UpdateDisplay();
                GD.Print("UpdateDisplay chamado com sucesso!");
            }
            else if (node != null)
            {
                GD.PrintErr("ERRO: O nó encontrado no grupo 'LivesDisplay' não tem o script LivesDisplay.cs anexado!");
            }
            else
            {
                GD.PrintErr("ERRO: Nenhum nó encontrado no grupo 'LivesDisplay'.");
            }
        }

        // some independente de ter adicionado vida ou não
        QueueFree();
    }
}