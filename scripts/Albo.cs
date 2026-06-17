using Godot;
using System.Threading.Tasks;

public partial class Albo : CharacterBody2D
{
    // ==================== TIPOS ====================
    public enum AlboState { Walk, Dash, Hit, Dead }

    // ==================== CONSTANTES ====================
    private const float Speed = 60.0f;
    private const float DashSpeed = 200.0f;
    private const float DashDuration = 0.50f;
    private const float DashCooldown = 1.2f;
    private const int MaxLives = 2;

    // ==================== REFERÊNCIAS AOS NÓS ====================
    private AnimatedSprite2D _anim;
    private Area2D _hitbox;
    private CollisionShape2D _collisionShape;
    private RayCast2D _wallDetector;
    private RayCast2D _groundDetector;
    private RayCast2D _playerDetector;

    // ==================== ESTADO ====================
    private AlboState _status;
    private int _direction = -1;
    private int _lives = MaxLives;

    // controla se pode fazer dash, false durante cooldown
    private bool _canDash = true;

    // guarda se estava bloqueado no frame anterior
    // evita flip repetido enquanto continua encostado na parede
    private bool _wasBlocked = false;

    // ==================== CICLO DE VIDA (GODOT) ====================

    public override void _Ready()
    {
        // busca todos os nós filhos pelo nome exato
        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hitbox = GetNode<Area2D>("Hitbox");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _wallDetector = GetNode<RayCast2D>("WallDetector");
        _groundDetector = GetNode<RayCast2D>("GroundDetector");
        _playerDetector = GetNode<RayCast2D>("PlayerDetector");

        GoToWalkState();
    }

    public override void _PhysicsProcess(double delta)
    {
        // se está em hit ou morrendo, para tudo e sai
        if (_status == AlboState.Hit || _status == AlboState.Dead)
        {
            Velocity = Vector2.Zero;
            return;
        }

        // aplica gravidade quando está no ar
        // GetGravity retorna o vetor de gravidade configurado no projeto
        if (!IsOnFloor())
            Velocity += GetGravity() * (float)delta;

        // executa o estado atual
        switch (_status)
        {
            case AlboState.Walk:
                WalkState((float)delta);
                break;

            case AlboState.Dash:
                break;
        }

        // aplica o movimento calculado
        MoveAndSlide();
    }

    // ==================== API PÚBLICA ====================

    // chamado de fora pelo sistema de ataque do player
    public void TakeDamage()
    {
        if (_status == AlboState.Dead) return;

        _lives--;

        if (_lives <= 0)
            GoToDeadState();
        else
            GoToHitState();
    }

    // ==================== TRANSIÇÕES DE ESTADO ====================

    private void GoToWalkState()
    {
        _status = AlboState.Walk;
        _anim.Play("walk");
    }

    // async void, tem await mas não precisa ser aguardado
    private async void GoToDashState(Node2D target)
    {
        if (!_canDash) return;

        _canDash = false;
        _status = AlboState.Dash;

        // calcula direção do dash em relação ao player
        // Mathf.Sign retorna -1 ou 1 dependendo do sinal
        float dashDir = Mathf.Sign(target.GlobalPosition.X - GlobalPosition.X);

        // vira pro lado do player se necessário
        if (dashDir != _direction)
            Flip();

        // aplica velocidade do dash
        Velocity = new Vector2(DashSpeed * dashDir, Velocity.Y);

        if (_anim.SpriteFrames.HasAnimation("dash")) _anim.Play("dash");

        // espera o dash terminar
        await ToSignal(
            GetTree().CreateTimer(DashDuration),
            SceneTreeTimer.SignalName.Timeout
        );

        // para o movimento horizontal após o dash
        Velocity = new Vector2(0, Velocity.Y);

        // volta ao walk se não morreu durante o dash
        if (_status != AlboState.Dead)
            GoToWalkState();

        // espera o cooldown antes de poder dashar de novo
        await ToSignal(
            GetTree().CreateTimer(DashCooldown),
            SceneTreeTimer.SignalName.Timeout
        );

        // só reativa se não está morrendo
        if (_status != AlboState.Dead)
            _canDash = true;
    }

    private async void GoToHitState()
    {
        _status = AlboState.Hit;
        if (_anim.SpriteFrames.HasAnimation("hit"))
            _anim.Play("hit");

        await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);

        // após o hit volta a andar
        GoToWalkState();
    }

    private async void GoToDeadState()
    {
        _status = AlboState.Dead;
        Velocity = Vector2.Zero;

        // desativa física, para de processar movimento
        SetPhysicsProcess(false);

        // SetDeferred executa no próximo frame seguro
        // necessário pra não causar erro durante o processamento de física
        _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        _hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);

        if (_anim.SpriteFrames.HasAnimation("dead"))
            _anim.Play("dead");

        await ToSignal(
            GetTree().CreateTimer(0.5f),
            SceneTreeTimer.SignalName.Timeout
        );

        // remove o nó da cena após a animação
        QueueFree();
    }

    // ==================== LÓGICA DE ESTADO ====================

    private void WalkState(float delta)
    {
        // move na direção atual com velocidade constante
        Velocity = new Vector2(Speed * _direction, Velocity.Y);

        // verifica se está bloqueado, parede na frente ou borda
        // IsColliding verifica se o RayCast está tocando algo
        bool blocked = _wallDetector.IsColliding() || !_groundDetector.IsColliding();

        // só vira se acabou de ficar bloqueado evita flip repetido
        if (blocked && !_wasBlocked)
            Flip();

        // atualiza pra comparar no próximo frame
        _wasBlocked = blocked;

        CheckPlayerDetection();
    }

    private void CheckPlayerDetection()
    {
        // se está em cooldown, não verifica
        if (!_canDash) return;

        // verifica se o RayCast está tocando algo
        if (_playerDetector.IsColliding())
        {
            // GetCollider retorna o objeto que o RayCast está tocando
            var hit = _playerDetector.GetCollider();

            // verifica se o que foi atingido é um Player
            if (hit is Player player)
                GoToDashState(player);
        }
    }

    // ==================== HELPERS ====================

    // inverte a direção e o sprite visualmente
    private void Flip()
    {
        _direction *= -1;

        // Scale é uma propriedade Vector2 não pode modificar X diretamente
        // precisa criar um novo Vector2 com X invertido
        Scale = new Vector2(Scale.X * -1, Scale.Y);
    }
}