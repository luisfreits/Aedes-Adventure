using Godot;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public partial class ApiManager : Node
{
    private const string ApiUrl = "https://api-ranking-aedes.vercel.app/api/ranking";

    private static readonly System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();

    public async Task SaveRun(string playerName, float time)
    {
        // InvariantCulture: Garante que o float use ponto e nunca vírgula
        // Monta o JSON exatamente como o Express espera receber na rota POST
        var json = $"{{\"player_name\":\"{playerName}\",\"time\":{time.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}";
        
        // Envia os dados no formato JSON puro
        var conteudo = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            // Faz a requisição POST para salvar no banco do Aiven
            var response = await Http.PostAsync(ApiUrl, conteudo);
            GD.Print($"[API POST] Código de Status: {response.StatusCode}");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[API POST] Falha ao conectar: {e.Message}");
        }
    }

    public async Task<string> GetTopRuns()
    {
        try
        {
            // Faz requisição GET simples
            var response = await Http.GetAsync(ApiUrl);
            
            // Lê o texto puro (JSON) retornado pelo servidor
            var body = await response.Content.ReadAsStringAsync();
            GD.Print($"[API GET] Dados recebidos: {body}");
            return body;
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[API GET] Falha ao buscar ranking: {e.Message}");
            return "[]"; // Retorna uma lista vazia em formato texto caso dê erro
        }
    }
}