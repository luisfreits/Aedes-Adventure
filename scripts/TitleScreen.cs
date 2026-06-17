using Godot;

public partial class TitleScreen : Control
{
    private Button _runBtn;
    private Button _rankBtn;
    private const string SavePath = "user://savegame.save";

    //nos de audio
    private AudioStreamPlayer _soundtrack;
    private AudioStreamPlayer _audio;

    public override void _Ready()
    {
        _soundtrack = GetNode<AudioStreamPlayer>("soundtrack");
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        // busca os botões
        _runBtn = GetNode<Button>("HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/run_btn");
        _rankBtn = GetNode<Button>("HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/rank_btn");

        // só mostra se o jogador já zerou
        _runBtn.Visible = GameManager.HasCompleted;
        _rankBtn.Visible = GameManager.HasCompleted;

        GetNode("/root/Hud").Call("EsconderHud");
        GetNode("/root/LivesDisplay").Call("EsconderLivesDisplay");

        _soundtrack.Stream = GD.Load<AudioStream>("res://audio_assets/tela_inicial.mp3");
        _soundtrack.Play();
    }

    // método conectado ao sinal pressed do new_btn via editor
    private void OnNewBtnPressed()
    {
        GameManager.ResetLives();
        GameManager.StartTimer();
        if (FileAccess.FileExists(SavePath))
            DirAccess.RemoveAbsolute(SavePath);
        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
        GetTree().ChangeSceneToFile("res://scene/Fase-1-0.tscn");
    }
    private void OnLoadBtnPressed()
    {
        //verifica se o arquivo existe
        if (!FileAccess.FileExists(SavePath))
        {
            return; //se nao tiver save nao roda
        }

        //open abre o arquivo pra ler
        //modeflags.read equivale ao read do gdscript
        var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);

        //GetLine le uma linha do arquivo
        string scenePath = file.GetLine();

        //fecha arquivo apos ler
        file.Close();

        GetTree().ChangeSceneToFile(scenePath);
    }

    private void OnRunBtnPressed()
    {
        GetTree().ChangeSceneToFile("res://scene/run_setup.tscn");
    }

    private void OnNoBtnPressed()
    {
        GetTree().ChangeSceneToFile("res://scene/title_screen.tscn");
    }

    private void OnRankBtnPressed()
    {
        GetTree().ChangeSceneToFile("res://scene/ranking.tscn");
    }

    private void OnCreditsBtnPressed()
    {
        GetTree().ChangeSceneToFile("res://scene/creditos.tscn");
    }

    private void OnQuitBtnPressed()
    {
        // Quit encerra o jogo equivale ao get_tree().quit()
        GetTree().Quit();
    }

    // conectado ao sinal mouse_entered de cada botão via editor
    private void OnButtonMouseEntered()
    {
        _audio.Stream = GD.Load<AudioStream>("res://audio_assets/btn_hover.mp3");
        _audio.Play();
    }
}