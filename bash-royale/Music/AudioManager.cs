using Microsoft.Xna.Framework.Media;

namespace bash_royale.Music;

public static class AudioManager
{
    private static Song? _menuMusic;
    private static bool _loaded;

    /// <summary>
    /// Loads all audio assets from disk. Safe to call multiple times — only loads once.
    /// </summary>
    public static void LoadAll()
    {
        if (_loaded) return;

        string audioRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music", "Content");

        _menuMusic = LoadSong("mainMenu", Path.Combine(audioRoot, "mainMenu.ogg"));

        _loaded = true;
    }

    public static void PlayMenuMusic()
    {
        StopMusic();

        if (_menuMusic == null) return;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = GameSettings.MUSIC_VOLUME;
        MediaPlayer.Play(_menuMusic);
    }

    public static void StopMusic()
    {
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }
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