using Godot;

public partial class GameOver : Control
{

    private bool _botaoClicado = false; 

    public override void _Ready()
    {
        // Garante que o HUD esteja escondido ao morrer
        GetNode("/root/Hud").Call("EsconderHud");
        GetNode("/root/LivesDisplay").Call("EsconderLivesDisplay");

        // Cria o player de áudio dinamicamente na memória para o Game Over
        AudioStreamPlayer somGameOver = new AudioStreamPlayer();
        AddChild(somGameOver);
        somGameOver.Stream = GD.Load<AudioStream>("res://audio_assets/game_over.mp3");
        somGameOver.Play();
    }

    // Transformamos o método em "async void" para podermos usar o comando "await"
    private async void OnSimBtnPressed()
    {
        // Se o botão já foi clicado uma vez, ignora os próximos cliques
        if (_botaoClicado) return;
        _botaoClicado = true;

        // Cria player de áudio temporário para o botão Sim
        AudioStreamPlayer somBtn = new AudioStreamPlayer();
        AddChild(somBtn);
        somBtn.Stream = GD.Load<AudioStream>("res://audio_assets/sim.mp3");
        somBtn.Play();

        await ToSignal(somBtn, AudioStreamPlayer.SignalName.Finished);

        // Lógica para reiniciar a fase ou continuar
        GameManager.ResetLives();
        GameManager.StartTimer();
        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
        GetTree().ChangeSceneToFile("res://scene/Fase-1-0.tscn");
    }

    private async void OnNaoBtnPressed()
    {
        if (_botaoClicado) return;
        _botaoClicado = true;

        // Cria player de áudio temporário para o botão Não
        AudioStreamPlayer somBtn = new AudioStreamPlayer();
        AddChild(somBtn);
        somBtn.Stream = GD.Load<AudioStream>("res://audio_assets/nao.mp3");
        somBtn.Play();

        // Espera o som terminar
        await ToSignal(somBtn, AudioStreamPlayer.SignalName.Finished);

        // Volta para o menu inicial
        GetTree().ChangeSceneToFile("res://scene/title_screen.tscn");
    }
}