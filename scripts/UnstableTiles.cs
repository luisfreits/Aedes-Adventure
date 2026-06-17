using Godot;
using System.Threading.Tasks;

public partial class UnstableTiles : AnimatableBody2D
{
    [Export] public float ShakeTime = 1.5f;
    [Export] public float FallTime = 1.0f;

    private AudioStreamPlayer _shakeSound;
    private AnimatedSprite2D _sprite;
    private CollisionShape2D _collision;

    private bool _triggered = false;

    public override void _Ready()
    {
        _shakeSound = GetNode<AudioStreamPlayer>("ShakeSound");
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        _sprite.Play("idle");
    }

    // Node body é o nó que entrou na área
    private async void OnArea2DBodyEntered(Node body)
    {
        // IsInGroup verifica se o nó pertence ao grupo
        if (!body.IsInGroup("Player") || _triggered)
            return;

        // marca como triggered pra não rodar de novo enquanto está em execução
        _triggered = true;

        // await em C# funciona igual ao GDScript espera o método terminar
        // mas o método precisa retornar Task pra poder ser aguardado
        await Shake();
        await Fall();
        await Respawn();
    }

    private async Task Shake()
    {
        _shakeSound.Stream = GD.Load<AudioStream>("audio_assets/shake.mp3");
        _shakeSound.Play();

        // toca animação de shake
        _sprite.Play("shake");

        //guarda posição original pra restaurar dps
        float originalX = Position.X;

        //guarda quanto tempo passou
        float elapsed = 0.0f;

        while (elapsed < ShakeTime)
        {
            //RangeRange gera numero aleatorio entre dois valores
            Position = new Vector2(
                originalX + (float)GD.RandRange(-0.3, 0.3), Position.Y
            );

            //retorna o delta do physics process
            elapsed += (float)GetPhysicsProcessDeltaTime();

            // ToSignal espera um sinal disparar, revisar essa linha
            // ProcessFrame é o sinal que dispara a cada frame visual
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        Position = new Vector2(originalX, Position.Y);
    }

    private async System.Threading.Tasks.Task Fall()
    {
        _sprite.Play("fall");

        // desativa a colisão
        _collision.Disabled = true;

        // cria um timer de fall_time seg e espera terminar
        await ToSignal(
            GetTree().CreateTimer(FallTime), // cria o timer
            Timer.SignalName.Timeout          // espera o sinal Timeout
        );

        // esconde o sprite depois da queda
        _sprite.Visible = false;
    }

    private async Task Respawn()
    {
        // restaura tudo ao estado inicial
        _sprite.Visible    = true;
        _collision.Disabled = false;
        _triggered          = false;
        _sprite.Play("idle");

        // Task.CompletedTask retorna uma Task já finalizada
        // necessário porque o método tem retorno Task mas não tem await
        // sem isso o compilador reclama, estudar mais sobre dps
        await Task.CompletedTask;
    }
}