using Godot;

public partial class LivesDisplay : CanvasLayer
{
    // imagens no inspetor
    [Export] public Texture2D ThreeLives;  // imagem com 3 vidas
    [Export] public Texture2D TwoLives;    // imagem com 2 vidas
    [Export] public Texture2D OneLife;     // imagem com 1 vida
    [Export] public Texture2D ZeroLives;   // imagem com 0 vidas

    private TextureRect _display;

    public override void _Ready()
{
    _display = GetNode<TextureRect>("Control/TextureRect");
    UpdateDisplay();
    Visible = false; 
}

    // atualiza a textura conforme as vidas atuais
    // chamado sempre que GameManager.Lives muda
    public void UpdateDisplay()
    {
        // verifica o valor de GameManager.Lives
        // e atribui a textura correspondente
        switch (GameManager.Lives)
        {
            case 3:
                _display.Texture = ThreeLives;
                break;

            case 2:
                _display.Texture = TwoLives;
                break;

            case 1:
                _display.Texture = OneLife;
                break;

            default:
                _display.Texture = ZeroLives;
                break;
        }
    }

    // esconde o display inteiro usado no menu principal
    // Visible é propriedade herdada de CanvasLayer
   public void EsconderLivesDisplay()
{
    Visible = false;
    GD.Print("[DEBUG LivesDisplay] Método EsconderLivesDisplay() foi chamado.");
}

public void MostrarLivesDisplay()
{
    Visible = true;
    GD.Print("[DEBUG LivesDisplay] Método MostrarLivesDisplay() foi chamado! Ficou visível.");
}
}