using Microsoft.Xna.Framework.Media;

namespace bash_royale.Music;

public static class AudioManager
{
    private static Song? _menuMusic;
    private static Song? _battleMusic;
    private static Song? _overtimeMusic;
    private static bool _loaded;
    
    

    public static bool IsMuted { get; private set; }


    public static void LoadAll()
    {
        if (_loaded) return;

        string audioRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music", "Content");

        _menuMusic = LoadSong("mainMenu", Path.Combine(audioRoot, "Bash Royale Track.ogg"));
        _battleMusic = LoadSong("battleMusic", Path.Combine(audioRoot, "Bash Royale Battle Theme.ogg"));
        _overtimeMusic = LoadSong("overtimeMusic", Path.Combine(audioRoot, "Bash Royale Battle Theme.ogg"));

        _loaded = true;
    }

    public static void PlayMenuMusic()
    {
        StopMusic();

        if (_menuMusic == null || IsMuted) return;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = GameSettings.MUSIC_VOLUME;
        MediaPlayer.Play(_menuMusic);
    }

    public static void PlayBattleMusic()
    {
        StopMusic();

        if (_battleMusic == null || IsMuted) return;   // BUG FIX: was checking _menuMusic

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = GameSettings.MUSIC_VOLUME;
        MediaPlayer.Play(_battleMusic);
    }
    public static void PlayOvertimeMusic()
    {
        StopMusic();

        if (_overtimeMusic == null || IsMuted) return;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = GameSettings.MUSIC_VOLUME;
        MediaPlayer.Play(_overtimeMusic);
    }

    public static void StopMusic()
    {
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }
    }
    
    public static bool ToggleMute()
    {
        IsMuted = !IsMuted;

        if (IsMuted)
        {
            StopMusic();
        }

        return IsMuted;
    }

    private static Song? LoadSong(string name, string path)
    {
        if (!File.Exists(path))
        {
            System.Console.WriteLine($"[AudioManager] File not found: {path}");
            return null;
        }

        return Song.FromUri(name, new Uri(path, UriKind.Absolute));
    }
}