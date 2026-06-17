using Godot;

public partial class RunSetup : Control
{
    private LineEdit _nameInput;

    public override void _Ready()
{
    _nameInput = GetNode<LineEdit>("VBoxContainer/LineEdit");

    // foca o campo de texto automaticamente ao abrir a tela
    _nameInput.GrabFocus();
}

    // conectado ao sinal pressed do StartButton via editor
    private void OnStartButtonPressed()
    {
        // StripEdges remove espaços no início e fim do texto
        string playerName = _nameInput.Text.StripEdges();

        // se o campo estiver vazio usa "Anonimo" como padrão
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonimo";

        // salva o nome no GameManager pra usar ao salvar a run
        GameManager.PlayerName = playerName;

        GameManager.ResetLives();
        GameManager.StartTimer();

        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
        GetTree().ChangeSceneToFile("res://scene/Fase-1-0.tscn");
    }
}