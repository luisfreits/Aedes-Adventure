using Godot;
using System;

public partial class FollowCamera : Camera2D
{
    //? significa que pode ser null
    private Node2D? _target = null;

    // override sobrescreve o _Ready que já existe no Camera2D
    public override void _Ready()
    {
        GetTarget();
    }


    public override void _Process(double _delta)
    {
        //verifica se achou jogador
        if(_target == null)
        {
            return;
        }
        //atualiza posicao da camera pra se igualar a do player a cada frame
        Position = _target.Position;
    }

    public void GetTarget()
    {
        //busca todos os nós que pertencem ao grupo "Player"
        //retorna uma lista de nós
        var nodes = GetTree().GetNodesInGroup("Player");

        if (nodes.Count == 0)
        {
            //mostra erro no output sem crashar
            GD.PushError("Player não encontrado no grupo 'Player'");
            return;
        }

        //GetNodesInGroup retorna array generico, por isso:
        _target = (Node2D)nodes[0];
    }
}