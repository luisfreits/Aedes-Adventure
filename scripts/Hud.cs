using Godot;

public partial class Hud : CanvasLayer
{
    // referência ao sprite animado dos corações
    private AnimatedSprite2D _heartsDisplay;

    public override void _Ready()
{
    _heartsDisplay = GetNode<AnimatedSprite2D>("HeartDisplay");
    UpdateHearts(0);
    Visible = false; // começa escondido, só aparece quando chamado
}

    // public chamado de fora pelo Player quando toma dano
    // int hitCount  quantidade de hits recebidos até agora
    public void UpdateHearts(int hitCount)
    {
        // compara hitCount com cada case e executa o correspondente
        switch (hitCount)
        {
            case 0:
                // sem dano mostra três corações
                _heartsDisplay.Play("three_hearts");
                break; // break encerra o case  obrigatório em C#

            case 1:
                // um hit mostra dois corações
                _heartsDisplay.Play("two_hearts");
                break;

            case 2:
                // dois hits mostra um coração piscando
                _heartsDisplay.Play("one_heart");
                break;
        }
    }

    // esconde o HUD inteiro usado no menu principal
    // Visible é propriedade herdada de CanvasLayer
    public void EsconderHud()
    {
        Visible = false;
    }

    // mostra o HUD usado ao iniciar uma fase
    public void MostrarHud()
    {
        Visible = true;
    }
}