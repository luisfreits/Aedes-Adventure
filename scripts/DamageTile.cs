using Godot;

public partial class DamageTile : TileMapLayer
{
    //recebe posição global para verificar
    public bool GetDamageAt(Vector2 pos)
    {
        // converte posição global pra local
        //tilemap trabalha com coord locais
        Vector2 localPos = ToLocal(pos);

        //LocalToMap converte posição local (pixels) pra coordenada de célula do tilemap
        //ex: pixel (64, 32) vira célula (2, 1)
        //tiles usam coordenadas inteiras, por isso Vector2I
        Vector2I cell = LocalToMap(localPos);

        //GetCellTileData busca os dados do tile nessa célula
        TileData? data = GetCellTileData(cell);

        // verifica se encontrou um tile nessa célula
        if(data == null)
        {
            return false;
        }

        // GetCustomData busca o valor do dado customizado chamado "damage"
        // retorna um Variant, tipo genérico do Godot
        Variant damageVariant = data.GetCustomData("damage");

        // AsBool() converte o Variant pra bool
        return damageVariant.AsBool();
    }
}