using Godot;

public partial class Player : CharacterBody2D
{
    //conjunto de constantes de estado
    public enum PlayerState
    {
        Idle,
        Walk,
        Jump,
        Fall,
        Dead,
        Attack,
        Hurt,
        Parry
    }

    //constantes que nunca mudam
    private const float JumpVelocity = -250.0f;
    private const float InvincibilityTime = 1.0f;

    //variaveis editaveis no inspetor
    [Export] public float MaxSpeed = 180.0f;
    [Export] public float Acceleration = 500.0f;
    [Export] public float Deceleration = 400.0f;
    [Export] public int MaxHits = 3;

    //variaveis equivalentes ao @ONREADY
    private AudioStreamPlayer _attackSound;
    private AudioStreamPlayer _walkSound;
    private AudioStreamPlayer _jumpSound;
    private AudioStreamPlayer _deathSound;
    private AnimatedSprite2D _anim;
    private AnimatedSprite2D _attackBoxSprite;
    private Timer _reloadTimer;
    private Area2D _attackBox;
    private Area2D _hitbox;
    private Sprite2D _sprite;
    private CollisionShape2D _attackCollision;

    // VARIÁVEIS DE ESTADO
    private Hud _hud;
    private int _facingDir = 1;
    private float _attackBoxBaseX;
    private PlayerState _status = PlayerState.Idle;
    private int _jumpCount = 0;
    private int _maxJumpCount = 2;
    private float _direction = 0.0f;
    private bool _attackInProgress = false;
    private int _hitCount = 0;
    private bool _isInvincible = false;
    //retorna estado de invencibilidade e altera ele sem necessidade de acesso direto
    public bool IsInvincible { get => _isInvincible; set => _isInvincible = value; }

    public override void _Ready()
    {
        _attackSound = GetNode<AudioStreamPlayer>("attack_sound");
        _walkSound = GetNode<AudioStreamPlayer>("walk_sound");
        _jumpSound = GetNode<AudioStreamPlayer>("jump_sound");
        _deathSound = GetNode<AudioStreamPlayer>("death_sound");
        _anim = GetNode<AnimatedSprite2D>("anim");
        _attackBoxSprite = GetNode<AnimatedSprite2D>("attack_box/AnimatedSprite2D");
        _reloadTimer = GetNode<Timer>("ReloadTimer");
        _attackBox = GetNode<Area2D>("attack_box");
        _hitbox = GetNode<Area2D>("Hitbox");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _attackCollision = GetNode<CollisionShape2D>("attack_box/attack_collision");

        _hud = GetNode<Hud>("/root/Hud");

        //deixa o valor positivo
        _attackBoxBaseX = Mathf.Abs(_attackBox.Position.X);

        _attackBoxSprite.Visible = false;

        _attackCollision.Disabled = true;
        UpdateAttackBoxPosition();
        GoToIdleState();
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateAttackBoxPosition();

        if (!IsOnFloor())
        {
            var vel = Velocity;
            vel += GetGravity() * (float)delta;   // aplica gravidade
            Velocity = vel;
        }

        switch (_status)
        {
            case PlayerState.Idle: IdleState((float)delta); break;
            case PlayerState.Walk: WalkState((float)delta); break;
            case PlayerState.Jump: JumpState((float)delta); break;
            case PlayerState.Fall: FallState((float)delta); break;
            case PlayerState.Attack: AttackState((float)delta); break;
            case PlayerState.Dead: DeadState((float)delta); break;
        }

        MoveAndSlide();

        // Caiu fora da tela
        if (Position.Y > 600 && _status != PlayerState.Dead)
            GoToDeadState();

        if (_status != PlayerState.Dead)
            CheckDamageTile();
    }

    // público porque inimigos chamam de fora
    public void TakeHit()
    {
        if (_status == PlayerState.Dead || _isInvincible)
            return;

        _hitCount++;
        _hud.UpdateHearts(_hitCount);

        if (_hitCount >= MaxHits)
            GoToDeadState();
        else
            GoToHurtState();
    }

    public void ZeroHit()
    {
        _hitCount = 0;
        // Diz ao HUD para atualizar o display mostrando 3 corações novamente (case 0)
        _hud.UpdateHearts(_hitCount);
    }

    // a função precisa ser "async void" para poder usar await
    private async void GoToHurtState()
    {
        // evita que o player tome múltiplos hits em sequência
        _isInvincible = true;

        BlinkSprite();

        await ToSignal(
            GetTree().CreateTimer(InvincibilityTime),
            SceneTreeTimer.SignalName.Timeout
        );

        if (_anim.Animation == "tampar")
            return; // sai sem desligar _isInvincible o Spawn vai desligar no fim

        // Só chega aqui se o player n ta tampando
        _isInvincible = false;

        if (_status != PlayerState.Dead)
        {
            if (IsOnFloor())
            {
                if (Input.GetAxis("left", "right") != 0)
                    GoToWalkState();
                else
                    GoToIdleState();
            }
            else
                GoToFallState();
        }
    }

    private async void BlinkSprite()
    {
        for (int i = 0; i < 5; i++)
        {
            var c = _anim.Modulate;
            c.A = 0.3f; //a pega opacidade
            _anim.Modulate = c;

            await ToSignal(
                GetTree().CreateTimer(InvincibilityTime / 10.0f),
                SceneTreeTimer.SignalName.Timeout
            );

            c.A = 1.0f;
            _anim.Modulate = c;

            await ToSignal(
                GetTree().CreateTimer(InvincibilityTime / 10.0f),
                SceneTreeTimer.SignalName.Timeout
            );
        }
    }

    private void Move(float delta)
    {
        // pega o valor do input
        _direction = Input.GetAxis("left", "right");

        // se estiver atacando, anda mais lento 
        float speedMultiplier;
        if (_status == PlayerState.Attack)
        {
            speedMultiplier = 0.5f;
        }
        else
        {
            speedMultiplier = 1.0f;
        }

        // calcula a velocidade que o player está tentando alcançar
        float targetVelocity = _direction * MaxSpeed * speedMultiplier;

        // define se usa aceleração ou desaceleração

        float currentStep;
        if (_direction != 0)
        {
            currentStep = Acceleration;
        }
        else
        {
            currentStep = Deceleration;
        }

        Vector2 vel = Velocity;

        // move suavemente o X atual em direção ao X desejado
        vel.X = Mathf.MoveToward(vel.X, targetVelocity, currentStep * delta);

        // aplica a velocidade modificada de volta
        Velocity = vel;

        // se está se movendo, atualiza a direção que o player está olhando
        if (_direction != 0)
        {
            if (_direction > 0)
            {
                _facingDir = 1;
            }
            else
            {
                _facingDir = -1;
            }

            // inverte o sprite horizontalmente quando olha pra esquerda
            if (_facingDir == -1)
            {
                _anim.FlipH = true;
            }
            else
            {
                _anim.FlipH = false;
            }

            UpdateAttackBoxPosition();
        }
    }

    private void UpdateAttackBoxPosition()
    {
        var pos = _attackBox.Position;
        pos.X = _attackBoxBaseX * _facingDir;
        _attackBox.Position = pos;

        var scale = _attackBox.Scale;
        scale.X = _facingDir;
        _attackBox.Scale = scale;
    }


    //estados de transição
    public void GoToIdleState()
    {
        _status = PlayerState.Idle;
        _anim.Play("idle");
    }

    public void GoToWalkState()
    {
        _status = PlayerState.Walk;
        _anim.Play("walk");
    }

    private void GoToFallState()
    {
        _status = PlayerState.Fall;
        _anim.Play("fall");
    }

    private void GoToJumpState()
    {
        _walkSound.Stop();
        _status = PlayerState.Jump;
        _anim.Play("jump");
        var vel = Velocity;
        vel.Y = JumpVelocity;
        Velocity = vel;
        _jumpCount++;
    }

    private async void GoToAttackState()
    {
        _walkSound.Stop();
        if (_attackInProgress || _status == PlayerState.Dead) return;

        _attackInProgress = true;
        _status = PlayerState.Attack;
        _attackCollision.Disabled = false;
        _anim.Play("attack");

        // Carrega e reproduz o som de ataque uma única vez ao entrar no estado
        _attackSound.Stream = GD.Load<AudioStream>("res://audio_assets/attack.mp3");
        _attackSound.Play();

        float fps = (float)_anim.SpriteFrames.GetAnimationSpeed("attack");
        int frameCount = _anim.SpriteFrames.GetFrameCount("attack");
        float duration = frameCount / fps;

        //revisar dps
        await ToSignal(
            GetTree().CreateTimer(duration),
            SceneTreeTimer.SignalName.Timeout
        );

        _attackCollision.Disabled = true;
        _attackInProgress = false;

        if (_status == PlayerState.Dead) return;

        if (IsOnFloor())
        {
            if (Input.GetAxis("left", "right") != 0) GoToWalkState();
            else GoToIdleState();
        }
        else
            GoToFallState();
    }

    // Mesma estrutura do ataque, só muda o estado interno .
    private async void GoToParryState()
    {
        if (_attackInProgress || _status == PlayerState.Dead) return;

        _attackInProgress = true;
        _status = PlayerState.Parry;
        _attackCollision.Disabled = false;
        _anim.Play("attack");

        float fps = (float)_anim.SpriteFrames.GetAnimationSpeed("attack");
        int frameCount = _anim.SpriteFrames.GetFrameCount("attack");
        float duration = frameCount / fps;

        await ToSignal(
            GetTree().CreateTimer(duration),
            SceneTreeTimer.SignalName.Timeout
        );

        _attackCollision.Disabled = true;
        _attackInProgress = false;
        _attackBoxSprite.Visible = false;

        if (_status == PlayerState.Dead) return;

        if (IsOnFloor())
        {
            if (Input.GetAxis("left", "right") != 0) GoToWalkState();
            else GoToIdleState();
        }
        else
            GoToFallState();
    }

    public async void GoToDeadState(bool fast = false)
    {
        _walkSound.Stop();

        //verificar modulate dps
        var animModulate = _anim.Modulate;
        animModulate.A = 1.0f;
        _anim.Modulate = animModulate;

        _status = PlayerState.Dead;
        Velocity = Vector2.Zero;
        _attackCollision.Disabled = true;
        _attackInProgress = false;
        _isInvincible = false;

        var spriteModulate = _sprite.Modulate;
        spriteModulate.A = 1.0f;
        _sprite.Modulate = spriteModulate;

        _anim.Play("dead");
        
        // Carrega e reproduz o som correto de morte
        _deathSound.Stream = GD.Load<AudioStream>("res://audio_assets/death.mp3");
        _deathSound.Play();

        await ToSignal(_anim, AnimationPlayer.SignalName.AnimationFinished);

        //verificar dps
        if (fast)
            _reloadTimer.WaitTime = 0.5;

        _reloadTimer.Start();
    }

    // FUNÇÕES DE ESTADO (chamadas a cada frame no _PhysicsProcess)
    private void IdleState(float delta)
    {
        var vel = Velocity;
        vel.X = 0;
        Velocity = vel;

        if (Input.IsActionJustPressed("parry")) GoToParryState();
        if (Input.IsActionJustPressed("attack")) GoToAttackState();
        else if (Input.GetAxis("left", "right") != 0) GoToWalkState();
        else if (Input.IsActionJustPressed("jump") && CanJump()) GoToJumpState();

        _walkSound.Stop();
    }

    private void WalkState(float delta)
    {
        Move(delta);

        if (Input.IsActionJustPressed("parry")) GoToParryState();
        if (Input.IsActionJustPressed("attack")) GoToAttackState();
        else if (_direction == 0)
        {
            var vel = Velocity;
            vel.X = 0;
            Velocity = vel;
            GoToIdleState();
        }
        else if (Input.IsActionJustPressed("jump")) GoToJumpState();
        else if (!IsOnFloor()) GoToFallState();

        if (!_walkSound.Playing)
        {
            _walkSound.Stream = GD.Load<AudioStream>("res://audio_assets/passos.mp3");
            _walkSound.Play();
        }
    }

    private void JumpState(float delta)
    {
        Move(delta);

        if (Input.IsActionJustPressed("parry")) GoToParryState();
        if (Input.IsActionJustPressed("attack")) GoToAttackState();
        else if (Input.IsActionJustPressed("jump") && CanJump()) GoToJumpState();
        else if (Velocity.Y > 0) GoToFallState();

        if (!_jumpSound.Playing)
        {
            _jumpSound.Stream = GD.Load<AudioStream>("res://audio_assets/jump.mp3");
            _jumpSound.Play();
        }
    }

    private void FallState(float delta)
    {
        Move(delta);

        if (Input.IsActionJustPressed("parry")) GoToParryState();
        if (Input.IsActionJustPressed("attack")) GoToAttackState();
        else if (Input.IsActionJustPressed("jump") && CanJump()) GoToJumpState();
        else if (IsOnFloor())
        {
            _jumpCount = 0;
            if (Input.GetAxis("left", "right") != 0) GoToWalkState();
            else GoToIdleState();
        }
    }

    private void AttackState(float delta)
    {
        Move(delta);
        if (Input.IsActionJustPressed("jump") && CanJump()) GoToJumpState();
    }

    private void DeadState(float delta) { }   // pass, corpo vazio

    private bool CanJump() => _jumpCount < _maxJumpCount;

    //   REVISAR
    private Node GetEnemyFromCollider(Node collider)
    {
        if (collider.IsInGroup("Enemies")) return collider;

        var parent = collider.GetParent();
        if (parent != null && parent.IsInGroup("Enemies")) return parent;

        return null;
    }

    private void OnHitboxAreaEntered(Area2D area)
    {
        var enemy = GetEnemyFromCollider(area);

        if (enemy != null)
        {
            if (Velocity.Y > 0)   // caindo pisa no inimigo
            {
                if (enemy.HasMethod("TakeDamage"))
                    enemy.Call("TakeDamage");
                else
                    enemy.QueueFree();

                GoToJumpState();
            }
            else
                TakeHit();
        }

        if (area.IsInGroup("LethalArea"))
            GoToDeadState();
    }

    // Se está em Parry, tenta chamar "reverse" no inimigo (inverte o boss)
    private void OnAttackBoxAreaEntered(Area2D area)
    {
        var enemy = GetEnemyFromCollider(area);

        if (enemy != null)
        {
            if (_status == PlayerState.Parry)
            {   
                _attackBoxSprite.Visible = true;
                _attackBoxSprite.Play("default");
                if (enemy.HasMethod("Reverse"))
                    enemy.Call("Reverse");

            }
            else
            {
                if (enemy.HasMethod("TakeDamage"))
                    enemy.Call("TakeDamage");
                else
                    enemy.QueueFree();
            }
        }
    }

    //resetar corações quando player morre
    private void OnReloadTimerTimeout()
    {
        // reseta o HUD de corações pra 0
        _hud.Call("UpdateHearts", 0);

        // remove uma vida do GameManager
        // LoseLife retorna true se ainda restam vidas, false se chegou a 0
        bool hasLives = GameManager.LoseLife();

        GetNode("/root/LivesDisplay").Call("UpdateDisplay");

        // se ainda tem vidas apenas reinicia a fase atual
        if (hasLives)
        {
            // ReloadCurrentScene recarrega a cena do zero
            GetTree().ReloadCurrentScene();
        }
        else
        {
            GameManager.ResetLives();

            // vai pro game over
            GetTree().ChangeSceneToFile("res://scene/game_over.tscn");
        }
    }

    private void CheckDamageTile()
    {
        var feetPos = GlobalPosition + new Vector2(0, 8);

        foreach (Node tilemap in GetTree().GetNodesInGroup("damage_tile"))
        {
            if (tilemap.HasMethod("GetDamageAt"))
            {
                bool hasDamage = (bool)tilemap.Call("GetDamageAt", feetPos);
                if (hasDamage)
                {
                    TakeHit();
                    return;   // evita hit múltiplo no mesmo frame
                }
            }
        }
    }
}