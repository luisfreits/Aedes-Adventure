using Godot;
using System.Threading.Tasks;

public partial class MovingPlatformBoss : AnimatableBody2D
{
    [Export] public Vector2 PointBOffset = new Vector2(150.0f, 0.0f);
    [Export] public float Speed = 60.0f;
    [Export] public float PauseTimeA = 1.0f;
    [Export] public float PauseTimeB = 1.0f;

    private Vector2 _pointA;
    private Vector2 _pointB;
    private AnimatedSprite2D _sprite;

    // referência à Area2D que detecta o player
    private Area2D _triggerArea;

    // controla se já foi ativado  evita ativar duas vezes
    private bool _triggered = false;

    public override async void _Ready()
    {
        _sprite      = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _triggerArea = GetNode<Area2D>("TriggerArea");

        // conecta o sinal BodyEntered da área de trigger
        _triggerArea.BodyEntered += OnTriggerBodyEntered;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _pointA = GlobalPosition;
        _pointB = GlobalPosition + PointBOffset;

        _sprite.Play("default");

        // nao inicia o loop aqui espera o player entrar
    }

    private void OnTriggerBodyEntered(Node2D body)
    {
        // ignora se não for player ou já foi ativado
        if (!body.IsInGroup("Player") || _triggered) return;

        // marca como ativado não vai rodar de novo
        _triggered = true;

        // inicia o trajeto uma única vez
        DoOneShot();
    }

    // async void roda o trajeto uma vez sem bloquear
    private async void DoOneShot()
    {
        _sprite.Play("default");

        // vai até o ponto B
        await MoveTo(_pointB);

        // pausa no ponto B
        await ToSignal(
            GetTree().CreateTimer(PauseTimeB),
            SceneTreeTimer.SignalName.Timeout
        );

        // volta ao ponto A
        await MoveTo(_pointA);

        // pausa no ponto A
        await ToSignal(
            GetTree().CreateTimer(PauseTimeA),
            SceneTreeTimer.SignalName.Timeout
        );

        // trajeto completo para aqui, não repete
    }

    private async Task MoveTo(Vector2 destination)
    {
        float duration = GlobalPosition.DistanceTo(destination) / Speed;

        var tween = CreateTween();
        tween.TweenProperty(this, "global_position", destination, duration)
             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, Tween.SignalName.Finished);
    }
}