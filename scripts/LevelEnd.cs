using Godot;

public partial class LevelEnd : Node2D
{
    // const string valor fixo, caminho do arquivo de save
    private const string SavePath = "user://savegame.save";

    // nome da próxima cena preenchido no inspetor
    [Export] public string NextLevel = "";

    // quando true, salva o progresso ao passar por esse ponto
    // equivale ao is_checkpoint do GDScript
    [Export] public bool IsCheckpoint = false;

    // conectado ao sinal body_entered via editor
    private void OnBodyEntered(Node2D body)
    {
        // Verifica a quantidade de nós restantes nos grupos "enemies" e "need"
        int remainingEnemies = GetTree().GetNodesInGroup("enemies").Count;
        int remainingNeeds = GetTree().GetNodesInGroup("Need").Count;

        // Se ainda houver inimigos ou focos de lixo/água não limpos, bloqueia o avanço
        if (remainingEnemies > 0 || remainingNeeds >0)
        {
            TriggerBlockDialog();
        }
        else
        {
            // CallDeferred executa o método no próximo frame seguro
            // evita bugs de física ao trocar de cena durante o processamento
            // equivale ao call_deferred("load_next_scene") do GDScript
            CallDeferred(MethodName.LoadNextScene);
        }
    }

    // Método que chama o Dialogic para exibir o aviso de bloqueio
    private void TriggerBlockDialog()
    {
        // Acessa o nó Autoload global do Dialogic (/root/Dialogic)
        var dialogic = GetNodeOrNull("/root/Dialogic");

        if (dialogic != null)
        {
            dialogic.Call("start", "timeline-restart");
        }
        else
        {
            GD.PrintErr("Erro: O Autoload do Dialogic não foi encontrado!");
        }
    }

    // MethodName.LoadNextScene forma segura de referenciar o nome do método
    // equivale a passar "LoadNextScene" como string mas com verificação do compilador
    // se errar o nome o compilador avisa diferente de uma string que só erraria em execução
    public void LoadNextScene()
    {
        // salva o progresso se for um checkpoint
        if (IsCheckpoint)
            Save(NextLevel);

        // monta o caminho completo da cena e troca
        // $ — string interpolada, equivale a "res://scene/" + NextLevel + ".tscn"
        GetTree().ChangeSceneToFile($"res://scene/{NextLevel}.tscn");
    }

    // private só chamado internamente por LoadNextScene
    private void Save(string sceneName)
    {
        // FileAccess.Open abre o arquivo pra escrita
        // ModeFlags.Write cria o arquivo se não existir, sobrescreve se existir
        // equivale ao FileAccess.open(SAVE_PATH, FileAccess.WRITE) do GDScript
        var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);

        // StoreLine escreve uma linha de texto no arquivo
        // equivale ao file.store_line() do GDScript
        file.StoreLine($"res://scene/{sceneName}.tscn");

        // fecha o arquivo libera o recurso
        // importante sempre fechar após usar
        file.Close();
    }
}