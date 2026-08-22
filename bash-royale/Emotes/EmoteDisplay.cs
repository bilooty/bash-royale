namespace bash_royale.Emotes;

public class EmoteDisplay
{
    public EmoteId Emote { get; }
    public PlayerId Owner { get; }
    public int TicksLeft { get; private set; }
    
    private const int DisplayDuration = 40; // 2secs
    public bool TicksUp =>  TicksLeft <= 0;

    public EmoteDisplay(EmoteId emote, PlayerId owner)
    {
        Emote = emote;
        Owner = owner;
        TicksLeft = DisplayDuration;
    }
    
    public void Tick() =>  TicksLeft--;
    
    public void Draw(ScreenSurface surface, bool isHost)
    {
        // Show emote on the sender's half of the arena
        bool isLocalPlayer = (Owner == PlayerId.One) == isHost;
        int y = isLocalPlayer ? ArenaMap.Height - 2 : 1;
        int x = ArenaMap.Width / 2 - 1;
        string label = EmoteInfo.GetLabel(Emote);
        Color  color = EmoteInfo.GetColor(Emote);
        
        // Fade effect: dim the colour in the last 0.5s
        int FADE_TICKS = 10;
        if (TicksLeft < FADE_TICKS)
        {
            float alpha = (float)TicksLeft / FADE_TICKS;
            color = color * alpha;
        }
        surface.Print(x, y, label, color, Color.Transparent);
    }
    
    
}

