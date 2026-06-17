using Godot;
using System.Threading.Tasks;

public partial class Boss : CharacterBody2D
{
    // ==================== TIPOS ====================
    public enum BossState { Idle, Stun, Dead }

    // ==================== CONSTANTES ====================
    private const int MaxLives = 8;
    private const float StunDuration = 5.0f;

    // ==================== CONFIGURAÇÃO (EXPORTS) ====================
    // expõe no inspetor
    [Export] public PackedScene MosquitoScene;
    [Export] public float FlyHeight = 150.0f;
    [Export] public float FlySpeed = 80.0f;
    [Export] public float SpawnInterval = 1.0f;
    [Export] public int SpawnsBeforeStun = 10;

    // ==================== REFERÊNCIAS AOS NÓS ====================
    private AnimatedSprite2D _anim;
    private Area2D _hitbox;
    private CollisionShape2D _collisionShape;

    // ==================== ESTADO ====================
    // estado atual do boss
    private BossState _status;

    // vidas restantes começa no máximo
    private int _lives = MaxLives;

    // posição Y do chão salva no _Ready pra saber onde é o chão
    private float _groundY;

    // contador de mosquitos spawnados nesse ciclo
    private int _spawnCount = 0;

    // indica se está subindo ou descendo usado pra saber direção
    private bool _goingUp = true;

    // ==================== CICLO DE VIDA (GODOT) ====================

    public override void _Ready()
    {
        // busca os nós filhos
        _anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hitbox = GetNode<Area2D>("Hitbox");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        // conecta o sinal AreaEntered do hitbox
        // quando algo entrar no hitbox, OnHitboxAreaEntered é chamado
        _hitbox.AreaEntered += OnHitboxAreaEntered;

        // salva a posição Y inicial como "chão" do boss
        _groundY = GlobalPosition.Y;

        // inicia no estado idle
        GoToIdleState();

        // inicia os dois loops independentes
        // como são async void, rodam em paralelo sem bloquear o _Ready
        SpawnTimerLoop();
        MoveLoop();
    }

    // o movimento é feito manualmente no FlyTo
    public override void _PhysicsProcess(double delta) { }

    // ==================== API PÚBLICA ====================

    // public chamado de fora (pelo mosquito revertido)
    public void TakeDamage()
    {
        // ignora dano se já está morrendo
        if (_status == BossState.Dead) return;

        _lives--;
        GD.Print("tomou dano " + _lives);

        if (_lives <= 0)
            GoToDeadState();
        else
            GoToHitState();
    }

    // ==================== TRANSIÇÕES DE ESTADO ====================

    private void GoToIdleState()
    {
        _status = BossState.Idle;
        if (_anim.SpriteFrames.HasAnimation("idle"))
            _anim.Play("idle");
    }

    private async Task DoStun()
    {
        _status = BossState.Stun;

        // HasAnimation verifica se a animação existe antes de tocar
        // evita erro se a animação não foi criada ainda
        if (_anim.SpriteFrames.HasAnimation("stun"))
            _anim.Play("stun");

        await ToSignal(
            GetTree().CreateTimer(StunDuration),
            SceneTreeTimer.SignalName.Timeout
        );

        // após o stun, volta pro idle se ainda estiver vivo
        if (IsInstanceValid(this) && _status != BossState.Dead)
            GoToIdleState();
    }

    // async void tem await mas não precisa ser aguardado por ninguém
    private async void GoToHitState()
    {
        // guarda o estado anterior pra restaurar depois da animação
        var prevStatus = _status;

        if (_anim.SpriteFrames.HasAnimation("hit"))
            _anim.Play("hit");

        await ToSignal(
            GetTree().CreateTimer(0.4f),
            SceneTreeTimer.SignalName.Timeout
        );

        if (!IsInstanceValid(this)) return;

        // restaura o estado que tinha antes do hit
        if (prevStatus == BossState.Idle)
            GoToIdleState();
        else if (prevStatus == BossState.Stun)
            _anim.Play("stun"); // volta a animação de stun sem mudar o status
    }

    private async void GoToDeadState()
    {
        GameManager.StopTimer(); // para e salva o tempo
        GameManager.HasCompleted = true;
        GD.Print($"Run completada em: {GameManager.RunTime:F2}s");

        _status = BossState.Dead;

        // SetDeferred executa a mudança de propriedade no próximo frame seguro
        // necessário durante física, mudar colisão diretamente pode causar erros

        _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        _hitbox.SetDeferred(Area2D.PropertyName.Monitorable, false);

        if (_anim.SpriteFrames.HasAnimation("dead"))
            _anim.Play("dead");

        var api = GetTree().Root.GetNode<ApiManager>("ApiManager");
        if (api != null)
            await api.SaveRun(GameManager.PlayerName, GameManager.RunTime);

        await ToSignal(
            GetTree().CreateTimer(0.5f),
            SceneTreeTimer.SignalName.Timeout
        );

        GetTree().ChangeSceneToFile("res://scene/game_over.tscn");

        // remove o nó da cena
        QueueFree();
    }

    // ==================== LÓGICA DE ESTADO ====================

    // async void roda em paralelo, não bloqueia quem chamou
    // loop infinito que spawna mosquitos a cada SpawnInterval segundos
    private async void SpawnTimerLoop()
    {
        // IsInstanceValid verifica se o nó ainda existe na cena
        while (IsInstanceValid(this) && _status != BossState.Dead)
        {
            // espera SpawnInterval segundos antes de spawnar
            await ToSignal(
                GetTree().CreateTimer(SpawnInterval),
                SceneTreeTimer.SignalName.Timeout
            );

            // verifica novamente após o timer status pode ter mudado
            if (_status == BossState.Dead) break;

            // durante stun não spawna pula pro próximo ciclo
            if (_status == BossState.Stun) continue;

            // spawna o mosquito e incrementa contador
            SpawnMosquito();
            _spawnCount++;
        }
    }

    private async void MoveLoop()
    {
        while (IsInstanceValid(this) && _status != BossState.Dead)
        {
            // se estiver em stun, espera um pouco e tenta de novo
            // sem esse await o while  buga
            if (_status == BossState.Stun)
            {
                await ToSignal(
                    GetTree().CreateTimer(0.1f),
                    SceneTreeTimer.SignalName.Timeout
                );
                continue; // volta pro início do while
            }

            // sobe até fly_height acima do chão
            _goingUp = true;
            await FlyTo(_groundY - FlyHeight);

            // desce de volta ao chão
            _goingUp = false;
            await FlyTo(_groundY);

            // chegou no chão verifica se deve entrar em stun
            if (_spawnCount >= SpawnsBeforeStun)
            {
                _spawnCount = 0; // reseta contador pro próximo ciclo
                await DoStun();
            }
        }
    }

    // ==================== HELPERS ====================

    // Task retorna Task pra poder ser aguardado com await
    // move o boss gradualmente até target_y
    private async Task FlyTo(float targetY)
    {
        // continua movendo enquanto não chegou perto o suficiente
        // Mathf.Abs equivale ao abs() do GDScript
        while (IsInstanceValid(this) && Mathf.Abs(GlobalPosition.Y - targetY) > 2.0f)
        {
            // se morreu ou stunnou, para o movimento imediatamente
            if (_status == BossState.Dead || _status == BossState.Stun)
                return;

            // Mathf.Sign retorna -1, 0 ou 1 dependendo do sinal do número
            float dir = Mathf.Sign(targetY - GlobalPosition.Y);

            // move a posição Y diretamente — sem move_and_slide
            // GetPhysicsProcessDeltaTime equivale ao get_physics_process_delta_time()
            GlobalPosition = new Vector2(
                GlobalPosition.X,
                GlobalPosition.Y + dir * FlySpeed * (float)GetPhysicsProcessDeltaTime()
            );

            // espera 1 frame de física antes de continuar
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }

        // garante que chegou exatamente no destino
        if (IsInstanceValid(this))
            GlobalPosition = new Vector2(GlobalPosition.X, targetY);
    }

    private void SpawnMosquito()
    {
        if (MosquitoScene == null) return;

        // Instantiate cria uma instância da cena
        // equivale ao mosquito_scene.instantiate() do GDScript
        var mosquito = MosquitoScene.Instantiate();

        // adiciona o mosquito como filho da cena atual
        GetTree().CurrentScene.AddChild(mosquito);

        // posiciona no mesmo lugar do boss
        // como não sabemos o tipo exato, usamos Node2D via cast
        ((Node2D)mosquito).GlobalPosition = GlobalPosition;

        // seta a direção do mosquito via Call
        // usado porque não temos acesso direto ao tipo MosquitoBoss aqui
        mosquito.Set("direction", 1);
    }

    private void OnHitboxAreaEntered(Area2D area)
    {
        var mosquito = area.GetParent();

        // CORREÇÃO: "IsReversed" com as maiúsculas corretas do C#
        var isReversed = mosquito.Get("IsReversed");

        if (isReversed.AsBool())
        {
            TakeDamage();
            mosquito.QueueFree();
        }
    }
}