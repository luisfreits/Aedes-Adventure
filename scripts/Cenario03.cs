using Godot;

public partial class Cenario03 : Node2D
{
    private AudioStreamPlayer _audio;
    private Area2D _zonaNovaMusica;
    private bool _musicaMudou = false; // Evita que a música fique reiniciando se o player entrar e sair da área

    public override void _Ready()
    {
        _audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _zonaNovaMusica = GetNode<Area2D>("MusicZone");

        _zonaNovaMusica.BodyEntered += OnZonaMusicaBodyEntered;

        // Carrega a música inicial do cenário
        AudioStream musica = GD.Load<AudioStream>("res://audio_assets/cenario_02.mp3");

        // Verifica se a música foi realmente encontrada
        if (musica == null)
        {
            GD.PrintErr("ERRO: cenario_02.mp3 não encontrado! Verifique o nome e a pasta.");
        }
        else
        {
            if (musica is AudioStreamMP3 mp3Musica)
            {
                mp3Musica.Loop = true; 
            }

            _audio.Stream = musica;
            _audio.Play();
        }

        GetNode("/root/Hud").Call("MostrarHud");
        GetNode("/root/LivesDisplay").Call("MostrarLivesDisplay");
    }

    private void OnZonaMusicaBodyEntered(Node2D body)
    {
        // CORREÇÃO: Usar o nome do nó ou grupo é muito mais seguro no Godot do que 'body is Player'
        if ((body.Name == "Player" || body.IsInGroup("Player")) && !_musicaMudou)
        {
            TrocarMusicaAmbiente("res://audio_assets/boss_song.mp3");
            _musicaMudou = true; // Bloqueia para não reiniciar a música do zero caso ele saia e entre de novo
        }
    }

    private void TrocarMusicaAmbiente(string caminhoDoAudio)
    {
        // Para a música antiga para não dar estalo no áudio
        _audio.Stop();

        // Carrega o novo arquivo enviado por parâmetro
        AudioStream novaMusica = GD.Load<AudioStream>(caminhoDoAudio);

        if (novaMusica == null)
        {
            GD.PrintErr($"ERRO: Música do boss não encontrada no caminho: {caminhoDoAudio}");
            return;
        }

        // Aplica o loop no novo arquivo também
        if (novaMusica is AudioStreamMP3 mp3Musica)
        {
            mp3Musica.Loop = true;
        }

        // Alimenta o player com a nova música e dá o Play
        _audio.Stream = novaMusica;
        _audio.Play();
    }
}