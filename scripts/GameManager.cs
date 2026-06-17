using Godot;

public partial class GameManager : Node
{
    // nome do jogador pra speedrun
public static string PlayerName = "";
    public static bool HasCompleted = false;
    public static int Lives = 3;
    public const int MaxLives = 3;

    // guarda o tempo total da run em segundos
    // static persiste entre cenas
    public static float RunTime = 0.0f;

    // controla se o cronômetro está rodando
    public static bool TimerRunning = false;

    public static void ResetLives()
    {
        Lives = MaxLives;
    }

    public static bool LoseLife()
    {
        Lives--;
        return Lives > 0;
    }

    // inicia o cronômetro, chamado ao começar novo jogo
    public static void StartTimer()
    {
        RunTime = 0.0f;
        TimerRunning = true;
    }

    // para o cronômetro, chamado quando o boss morre
    public static void StopTimer()
    {
        TimerRunning = false;
        GD.Print($"Tempo final da run: {RunTime:F2} segundos");
    }

    // _Process roda todo frame, atualiza o cronômetro
    // como GameManager é AutoLoad, _Process sempre roda
    public override void _Process(double delta)
    {
        if (!TimerRunning) return;

        // acumula o tempo passado
        // (float)delta converte double pra float
        RunTime += (float)delta;
    }
}