using Godot;

public partial class Cenario02 : Node2D
{
    private AudioStreamPlayer _audio;

    public override void _Ready()
    {
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        // 1. Carrega o recurso de áudio normalmente
        AudioStream musica = GD.Load<AudioStream>("res://audio_assets/cenario_02.mp3");

        // 2. Verifica se o áudio é realmente um MP3 e ativa o Loop de forma segura
        if (musica is AudioStreamMP3 mp3Musica)
        {
            mp3Musica.Loop = true; // Ativa o loop contínuo
        }

        // 3. Atribui ao player e inicia a reprodução
        _audio.Stream = musica;
        _audio.Play();

        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
    }
}