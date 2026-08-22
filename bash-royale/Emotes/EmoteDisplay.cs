namespace bash_royale.Emotes;

public class EmoteDisplay
{
    public EmoteId Emote { get; }
    public PlayerId Owner { get; }
    public int TicksLeft { get; private set; }

    private const int DisplayDuration = 200;
    public bool TicksUp => TicksLeft <= 0;

    public EmoteDisplay(EmoteId emote, PlayerId owner)
    {
        Emote = emote;
        Owner = owner;
        TicksLeft = DisplayDuration;
    }

    public void Tick() => TicksLeft--;

    public void Draw(ScreenSurface surface, bool isHost)
    {
        bool isLocalPlayer = (Owner == PlayerId.One) == isHost;
        string label = EmoteInfo.GetLabel(Emote);
        Color color = EmoteInfo.GetColor(Emote);

        int elapsed = DisplayDuration - TicksLeft;
        int[][] art = EmoteArt.GetArt(Emote);
        int FADE_TICKS = 10;

        
        int baseY = isLocalPlayer ? ArenaMap.Height - 2 : 2;
        int y = isLocalPlayer ? baseY - (elapsed / 10) : baseY + (elapsed / 10);
        int x = isLocalPlayer ? 1 : ArenaMap.Width - 10;

        if (TicksLeft < FADE_TICKS)
        {
            float alpha = (float)TicksLeft / FADE_TICKS;
            color = color * alpha;
        }

        for (int row = 0; row < art.Length; row++)
        {
            for (int col = 0; col < art[row].Length; col++)
            {
                surface.SetGlyph(x + col, y + row, art[row][col], color, Color.Black);
            }
        }
    }
}

