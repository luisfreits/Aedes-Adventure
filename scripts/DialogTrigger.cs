using Godot;
using System.Collections.Generic;

public partial class DialogTrigger : Area2D
{
    [Export] public Godot.Collections.Array<string> DialogTexts = new();

    // offset da posição onde o dialog_box vai aparecer
    // new Vector2(0, -20) cria Vector2 com valores iniciais
    [Export] public Vector2 OffsetPosition = new Vector2(0, -20);

    // controla se o player está dentro da área
    private bool _playerInside = false;

    // referência ao DialogManager filho
    private DialogManager _dialogManager;

    public override void _Ready()
    {
        // busca o DialogManager filho pelo tipo
        // GetNode<DialogManager> — busca pelo nome exato do nó
        _dialogManager = GetNode<DialogManager>("DialogManager");

        // conecta os sinais de entrada e saída da área
        // BodyEntered equivale ao body_entered do GDScript
        BodyEntered += OnBodyEntered;
        BodyExited  += OnBodyExited;
    }

    // chamado quando um corpo entra na área
    private void OnBodyEntered(Node2D body)
    {
        // "is Player" verifica se o corpo é um Player
        if (body is Player)
            _playerInside = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
            _playerInside = false;
    }

    // chamado quando um input não foi consumido por nenhum outro nó
    // InputEvent event contém informações sobre o input que aconteceu
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_playerInside) return;

        // IsActionPressed verifica se a ação foi pressionada nesse frame
        // equivale ao event.is_action_pressed() do GDScript
        if (!@event.IsActionPressed("interact")) return;

        // verifica se já tem diálogo ativo antes de iniciar outro
        if (_dialogManager.IsShowingDialog) return;

        // converte Godot.Collections.Array<string> pra List<string>
        // necessário porque StartDialog espera List<string>
        var textList = new List<string>(DialogTexts);

        // inicia o diálogo na posição do nó mais o offset
        _dialogManager.StartDialog(textList, GlobalPosition + OffsetPosition);
    }
}