using Godot;

public partial class Attack : Area2D
{
    // velocidade do projétil ajustável no inspetor
    [Export] public float Speed = 100.0f;

    // SetDirection é chamado de fora pelo Player
    public int Direction = 1;

    public override void _Process(double delta)
    {
        // move o ataque horizontalmente a cada frame
        // Area2D não tem MoveAndSlide ent move direto pela posição
        Position = new Vector2(
            Position.X + Speed * Direction * (float)delta,
            Position.Y
        );
    }

    // chamado pelo Player logo após instanciar o ataque
    // define pra qual lado o ataque vai se mover
    public void SetDirection(int direction)
    {
        // this.Direction — diferencia a variável da classe do parâmetro
        this.Direction = direction;
    }
}