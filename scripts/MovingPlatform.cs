using Godot;
using System.Threading.Tasks;

public partial class MovingPlatform : AnimatableBody2D
{
    // offset do ponto B em relação à posição inicial
    [Export] public Vector2 PointBOffset = new Vector2(150.0f, 0.0f);

    // velocidade de movimento em pixels por segundo
    [Export] public float Speed = 60.0f;

    // tempo de pausa em cada ponto
    [Export] public float PauseTimeA = 1.0f;
    [Export] public float PauseTimeB = 1.0f;

    // pontos de início e fim do trajeto
    // calculados no _Ready baseado na posição inicial
    private Vector2 _pointA;
    private Vector2 _pointB;

    // referência ao sprite animado
    private AnimatedSprite2D _sprite;

    public override async void _Ready()
    {
        // busca o sprite filho
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // espera 1 frame antes de ler a posição
        // necessário pois no _Ready a posição global ainda pode não ter sido aplicada

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // salva os pontos APÓS o frame posição já está correta
        _pointA = GlobalPosition;
        _pointB = GlobalPosition + PointBOffset;

        // toca a animação padrão
        _sprite.Play("default");

        // inicia o loop de movimento
        MoveLoop();
    }

    // loop infinito que move a plataforma entre os dois pontos
    private async void MoveLoop()
    {
        while (true)
        {
            // move até o ponto B e espera chegar
            GD.Print($"[MOVIMENTO] Indos para o Ponto B: {_pointB}");
            await MoveTo(_pointB);
            GD.Print("[MOVIMENTO] Chegou no Ponto B. Iniciando Timer...");

            // pausa no ponto B
            await ToSignal(
                GetTree().CreateTimer(PauseTimeB),
                SceneTreeTimer.SignalName.Timeout
            );

            await MoveTo(_pointA);

            // pausa no ponto A
            await ToSignal(
                GetTree().CreateTimer(PauseTimeA),
                SceneTreeTimer.SignalName.Timeout
            );
        }
    }

    // move suavemente até o destino usando Tween
    // retorna Task pra poder ser aguardado com await
    private async Task MoveTo(Vector2 destination)
    {
        // calcula quanto tempo o movimento vai durar

        float duration = GlobalPosition.DistanceTo(destination) / Speed;


        var tween = CreateTween();

        tween.TweenProperty(this, "global_position", destination, duration)

             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);

        // espera o Tween terminar antes de continuar
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}