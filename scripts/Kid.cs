using Godot;

public partial class Kid : Area2D
{
    // Resource tipo base do Dialogic, funciona independente do plugin
    [Export] public Resource Timeline;

    private bool _playerPresent = false;

    // referência ao AutoLoad do Dialogic
    // guardamos no _Ready pra não buscar toda vez que precisar
    private Node _dialogic;

    public override void _Ready()
    {
        // conecta os sinais de corpo
        BodyEntered += OnBodyEntered;
        BodyExited  += OnBodyExited;

        // busca o AutoLoad do Dialogic pelo caminho
        _dialogic = GetNode("/root/Dialogic");

        // conecta o sinal timeline_ended do Dialogic
        // quando o diálogo terminar, OnDialogEnded é chamado automaticamente
        // Connect com StringName forma segura de conectar sinais por string
        _dialogic.Connect("timeline_ended", Callable.From(OnDialogEnded));
    }

    // pausa todos os nós do grupo Pausable
    private void PauseAll()
    {
        // GetNodesInGroup retorna todos os nós do grupo
        var nodes = GetTree().GetNodesInGroup("Pausable");

        GD.Print($"Nós no grupo Pausable: {nodes.Count}");

        // foreach percorre cada nó da lista um por um
        foreach (var node in nodes)
        {
            GD.Print($"Pausando: {node.Name}");

            // desativa os três tipos de processamento do nó
            // SetPhysicsProcess  para _PhysicsProcess
            // SetProcess  para _Process
            // SetProcessUnhandledInput para _UnhandledInput
            node.SetPhysicsProcess(false);
            node.SetProcess(false);
            node.SetProcessUnhandledInput(false);
        }
    }

    // reativa todos os nós do grupo Pausable
    private void UnpauseAll()
    {
        foreach (var node in GetTree().GetNodesInGroup("Pausable"))
        {
            node.SetPhysicsProcess(true);
            node.SetProcess(true);
            node.SetProcessUnhandledInput(true);
        }
    }

    // chamado quando o sinal timeline_ended do Dialogic dispara
    private void OnDialogEnded()
    {
        UnpauseAll();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_playerPresent) return;
        if (!@event.IsActionPressed("interact")) return;

        if (Timeline == null)
        {
            GD.PrintErr($"[Erro] Nenhuma timeline foi arrastada para o Inspector deste nó: {Name}");
            return;
        }

        // pausa ANTES de iniciar o diálogo
        // evita que o player se mova durante a conversa
        PauseAll();

        // inicia a timeline do Dialogic
        _dialogic.Call("start", Timeline);

        // consome o input pra outros nós não reagirem ao mesmo clique
        GetViewport().SetInputAsHandled();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
            _playerPresent = true;
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("Player"))
            _playerPresent = false;
    }
}