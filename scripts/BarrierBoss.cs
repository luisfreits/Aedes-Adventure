using Godot;

public partial class BarrierBoss : Node2D
{
    // Referência para o nó CollisionShape2D que fica DENTRO do StaticBody2D (Wall)
    [Export] public CollisionShape2D WallCollision;

    // Conecte o sinal 'body_entered' do nó FILHO "Check" (Area2D) a este método
    private void OnCheckBodyEntered(Node2D body)
    {
        // Verifica a quantidade de nós restantes no grupo "need"

        int remainingNeeds = GetTree().GetNodesInGroup("Need").Count;

        // Se ainda houver inimigos ou focos de lixo/água não limpos
        if (remainingNeeds > 0)
        {
            // Bloqueia o avanço e chama o diálogo de aviso do Dialogic
            TriggerBlockDialog();
        }
        else
        {
            // Remove a parede física se a área estiver totalmente limpa
            RemoveWall();
        }
    }

    private void TriggerBlockDialog()
    {
        // Acessa o nó Autoload global do Dialogic (/root/Dialogic)
        var dialogic = GetNodeOrNull("/root/Dialogic");

        if (dialogic != null)
        {
            // Dialogic é um singleton GDScript, usamos .Call() para acionar a timeline
            dialogic.Call("start", "timeline-restart");
        }
        else
        {
            GD.PrintErr("Erro: O Autoload do Dialogic não foi encontrado!");
        }
    }

    private void RemoveWall()
    {
        // Desabilita a colisão da parede para que o player possa passar livremente
        if (WallCollision != null)
        {
            // Usamos SetDeferred para evitar erros de física ao desabilitar colisões durante a colisão
            WallCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
    }
}