using Godot;

public partial class GameOver : Control
{
    public override void _Ready()
    {
        // Garante que o HUD esteja escondido ao morrer
        GetNode("/root/Hud").Call("EsconderHud");
        GetNode("/root/LivesDisplay").Call("EsconderLivesDisplay");
    }

    private void OnSimBtnPressed()
    {
        // Lógica para reiniciar a fase ou continuar
        GameManager.ResetLives();
        GameManager.StartTimer();
        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
        GetTree().ChangeSceneToFile("res://scene/Fase-1-0.tscn");
    }

    private void OnNaoBtnPressed()
    {
        // Volta para o menu inicial
        GetTree().ChangeSceneToFile("res://scene/title_screen.tscn");
    }
}