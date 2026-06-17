using Godot;

public partial class Cenario01 : Node2D
{
    private AudioStreamPlayer _audio;

    public override void _Ready()
    {
        //GetNode busca filho pelo nome, chamado AudioStreamPlayer
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        _audio.Stream = GD.Load<AudioStream>("res://audio_assets/cenario_02.mp3");

        _audio.Play();

        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
    }
}