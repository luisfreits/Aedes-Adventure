using Godot;
using System.Threading.Tasks;

public partial class Pernilongo : CharacterBody2D
{
    // ==================== TIPOS ====================
    // enum, lista de estados possíveis
    public enum PernilongoState { Walk, Dead }

    // ==================== CONSTANTES ====================
    private const float Speed = 60.0f;

    // ==================== REFERÊNCIAS AOS NÓS ====================
    private AnimatedSprite2D _anim;
    private Area2D _hitbox;
    private CollisionShape2D _collisionShape;
    private RayCast2D _wallDetector;
    private RayCast2D _groundDetector;

    // ==================== ESTADO ====================
    // estado atual
    private PernilongoState _status;

    // direção atual, começa indo pra esquerda
    private int _direction = -1;

    // ==================== CICLO DE VIDA (GODOT) ====================

    public override void _Ready()
    {
        // busca todos os nós filhos pelo nome exato
        _anim           = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hitbox         = GetNode<Area2D>("Hitbox");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _wallDetector   = GetNode<RayCast2D>("WallDetector");
        _groundDetector = GetNode<RayCast2D>("GroundDetector");

        GoToWalkState();
    }

    public override void _PhysicsProcess(double delta)
    {
        // se está morrendo, para tudo e sai
        if (_status == PernilongoState.Dead)
        {
            Velocity = Vector2.Zero;
            return;
        }

        // aplica gravidade quando está no ar
        if (!IsOnFloor())
            Velocity += GetGravity() * (float)delta;

        // executa o estado atual
        if (_status == PernilongoState.Walk) WalkState((float)delta);

        MoveAndSlide();
    }

    // ==================== API PÚBLICA ====================

    // public, chamado de fora pelo sistema de ataque do player
    public void TakeDamage()
    {
        if (_status == PernilongoState.Dead) return;

        GoToDeadState();
    }

    // ==================== TRANSIÇÕES DE ESTADO ====================

    private void GoToWalkState()
    {
        _status = PernilongoState.Walk;
        _anim.Play("walk");
    }

    private async void GoToDeadState()
    {
        _status  = PernilongoState.Dead;

        // para o processamento de física
        SetPhysicsProcess(false);

        // desativa colisões no próximo frame seguro
        // SetDeferred, evita erro ao mudar física durante o processamento
        _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        _hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);

        if (_anim.SpriteFrames.HasAnimation("dead")) _anim.Play("dead");

        // espera a animação de morte terminar
        await ToSignal(
            GetTree().CreateTimer(0.5f),
            SceneTreeTimer.SignalName.Timeout
        );

        // remove o nó da cena
        QueueFree();
    }

    // ==================== LÓGICA DE ESTADO ====================

    private void WalkState(float delta)
    {
        // move na direção atual
        Velocity = new Vector2(Speed * _direction, Velocity.Y);

        // vira quando bate na parede ou chega na borda
        if (_wallDetector.IsColliding() || !_groundDetector.IsColliding())
            Flip();
    }

    // ==================== HELPERS ====================

    // inverte a direção e o sprite visualmente
    public void Flip()
    {
        _direction *= -1;

        // Scale é uma propriedade Vector2 não pode modificar X diretamente
        // precisa criar um novo Vector2 com X invertido
        Scale = new Vector2(Scale.X * -1, Scale.Y);
    }
}