using Godot;
using System.Threading.Tasks;

public partial class FallingPlatform : AnimatableBody2D
{
    [Export] public float ShakeTime = 0.5f;
    [Export] public float FallTime = 2.0f;

    // referências aos nós filhos
    // CollisionShape2D e Sprite2D tipos explícitos obrigatórios em C#
    private CollisionShape2D _collision;
    private Sprite2D _sprite;

    // impede que o trigger rode várias vezes ao mesmo tempo
    private bool _triggered = false;

    public override void _Ready()
    {
        // busca os nós filhos pelo nome exato da cena
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");
        _sprite    = GetNode<Sprite2D>("Sprite2D");
    }

    // conectado ao sinal body_entered da Area2D via editor
    // Node body qualquer nó que entrar na área
    private async void OnArea2DBodyEntered(Node body)
    {
        // "is Player" verifica o tipo só continua se for o player
        // !_triggered  ! é o NOT do C#, equivale ao "not triggered"
        // se não for p;layer OU já estiver triggered, sai imediatamente
        if (body is not Player || _triggered)
            return;

        // marca como ativo pra evitar execução dupla
        _triggered = true;

        // await espera cada etapa terminar antes de continuar a próxima
        // sem await as três rodariam ao mesmo tempo
        await Shake();
        await Fall();
        await Respawn();
    }
    private async Task Shake()
    {
        // guarda a posição original pra restaurar depois
        // Position é Vector2 estrutura copiada por valor, não por referência
        // isso significa que originalPos é uma cópia independente
        Vector2 originalPos = Position;

        // acumula o tempo passado
        float elapsed = 0.0f;

        while (elapsed < ShakeTime)
        {
            // GD.RandRange gera número aleatório entre os dois valores
            // equivale ao randf_range() do GDScript
            // (float) converte double pra float RandRange retorna double
            float offsetX = (float)GD.RandRange(-2.0, 2.0);

            // Vector2 é imutável em C# não pode fazer Position.X = valor
            // precisa criar um novo Vector2 com o valor alterado
            Position = new Vector2(originalPos.X + offsetX, originalPos.Y);

            // GetPhysicsProcessDeltaTime retorna o delta da física
            // equivale ao get_physics_process_delta_time() do GDScript
            elapsed += (float)GetPhysicsProcessDeltaTime();

            // espera 1 frame visual antes de continuar
            // ProcessFrame dispara uma vez por frame de renderização
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        // restaura posição exata após o shake
        Position = originalPos;
    }

    private async Task Fall()
    {
        // esconde o sprite plataforma some visualmente
        _sprite.Visible = false;

        // desativa colisão player cai através dela
        _collision.Disabled = true;

        // espera fall_time segundos antes de continuar
        await ToSignal(
            GetTree().CreateTimer(FallTime),
            SceneTreeTimer.SignalName.Timeout
        );
    }

    private async Task Respawn()
    {
        // restaura tudo ao estado inicial
        _sprite.Visible    = true;
        _collision.Disabled = false;
        _triggered          = false;

        // retorna uma Task já finalizada
        // necessário porque o método tem retorno Task
        // mas não tem nenhum await pra fazer
        // sem isso o compilador reclama que um método Task não tem await
        await Task.CompletedTask;
    }
}