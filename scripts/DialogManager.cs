using Godot;
using System.Collections.Generic;

public partial class DialogManager : Node
{
    // usado pra instanciar a cena do dialog_box quando precisar
    [Export] public PackedScene DialogScene;

    // guardamos a referência pra poder deletar depois
    private Node _dialogBox = null;

    // bool controla se já tem um diálogo na tela
    // evita abrir dois diálogos ao mesmo tempo
    public bool IsShowingDialog = false;

    // public chamado de fora por qualquer nó que queira iniciar um diálogo
    // Vector2 dialogPosition onde na tela o dialog_box vai aparecer
    public void StartDialog(List<string> texts, Vector2 dialogPosition)
    {
        // se já tem diálogo ativo, ignora a chamada
        if (IsShowingDialog) return;

        // verifica se a cena foi atribuída no inspetor
        // null check sem isso tentaria instanciar null e crasharia
        if (DialogScene == null) return;

        // Instantiate cria uma instância da cena em memória
        // ainda não está na cena só existe em memória até AddChild
        _dialogBox = DialogScene.Instantiate();

        // AddChild adiciona o dialog_box como filho da cena atual
        GetTree().CurrentScene.AddChild(_dialogBox);

        // Set define uma propriedade pelo nome em string
        // usado porque não temos o tipo exato do dialog_box
        // equivale ao dialog_box.texts_to_display = texts do GDScript
        var godotArray = new Godot.Collections.Array();
        foreach (string text in texts)
        {
            // itera sobre cada item da lista um por um
            godotArray.Add(text); // Add adiciona um item ao array
        }
        _dialogBox.Set("texts_to_display", godotArray);

        // define a posição global do dialog_box
        // como é Node2D por baixo, tem GlobalPosition
        // cast pra Node2D pra acessar a propriedade
        ((Node2D)_dialogBox).GlobalPosition = dialogPosition;

        // Call chama um método pelo nome em string
        // usado quando não temos acesso direto ao tipo
        // equivale ao dialog_box.show_text() do GDScript
        _dialogBox.Call("show_text");

        // marca que há um diálogo ativo
        IsShowingDialog = true;

        // conecta o sinal dialog_finished ao método
        // Callable.From cria um Callable (referência a método) a partir de um método C#
        // equivale ao dialog_box.dialog_finished.connect(_on_dialog_finished)
        _dialogBox.Connect(
            "dialog_finished",           // nome do sinal como string
            Callable.From(OnDialogFinished) // método que vai ser chamado
        );
    }

    // chamado automaticamente quando o sinal dialog_finished dispara
    private void OnDialogFinished()
    {
        // marca que não há mais diálogo ativo
        IsShowingDialog = false;

        // verifica se o dialog_box ainda existe antes de deletar
        // IsInstanceValid mais seguro que checar null pois detecta nós já deletados
        // equivale ao if dialog_box: do GDScript
        if (_dialogBox != null && IsInstanceValid(_dialogBox))
        {
            // remove o nó da cena no próximo frame seguro
            _dialogBox.QueueFree();

            // limpa a referência boa prática pra evitar memory leak
            _dialogBox = null;
        }
    }
}