using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

public partial class Ranking : Control
{
    // Armazena as referências dos 5 labels visuais da tela
    private List<Label> _labels = new List<Label>();

    public override async void _Ready()
    {
        // Encontra o container vertical que segura os textos do placar
        var container = GetNode<VBoxContainer>("VBoxContainer");
        GD.Print($"Container encontrado: {container != null}");

        // Loop para achar as Label1, Label2, Label3, Label4 e Label5
        for (int i = 1; i <= 5; i++)
        {
            if (!container.HasNode($"Label{i}"))
            {
                GD.PrintErr($"Label{i} não encontrado na árvore de nós!");
                continue;
            }

            var label = container.GetNode<Label>($"Label{i}");
            _labels.Add(label);

            // Texto provisório enquanto a Vercel responde
            label.Text = $"{i}. Carregando...";
        }

        // Dispara a busca dos dados reais na nuvem
        await LoadRanking();
    }

    private async Task LoadRanking()
    {
        // Puxa a instância global do nosso novo script de API configurado no Autoload
        // NOTA: Se você não mudou o nome no Autoload, pode continuar usando "/root/SupabaseManager"
        var apiManager = GetNode<ApiManager>("/root/ApiManager");

        // Solicita o texto JSON para o nosso servidor
        string json = await apiManager.GetTopRuns();

        // Converte a string de texto puro em um objeto JSON manipulável
        var doc = JsonDocument.Parse(json);
        var runs = doc.RootElement.EnumerateArray();

        int position = 1;

        // Passa por cada registro retornado pelo banco do Aiven
        foreach (var run in runs)
        {
            // Proteção para não estourar o limite de labels na tela
            if (position > _labels.Count) break;

            // Extrai as propriedades do JSON usando os mesmos nomes da tabela do banco
            string name = run.GetProperty("player_name").GetString();
            float time = (float)run.GetProperty("time").GetDouble();

            // Atualiza o texto da tela formatando o tempo com 2 casas decimais (:F2)
            _labels[position - 1].Text = $"{position}. {name} - {time:F2}s";

            position++;
        }

        // Caso o banco tenha menos de 5 registros, preenche o restante com traços "---"
        for (int i = position - 1; i < _labels.Count; i++)
        {
            _labels[i].Text = $"{i + 1}. ---";
        }
    }

    // Botão configurado para voltar à tela inicial
    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://scene/title_screen.tscn");
    }
}