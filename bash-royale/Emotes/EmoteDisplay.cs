namespace bash_royale.Emotes;

public class EmoteDisplay
{
    public EmoteId Emote { get; }
    public PlayerId Owner { get; }
    public int TicksLeft { get; private set; }
    
    private const int DisplayDuration = 200; 
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
        bool isLocalPlayer = (Owner == PlayerId.One) == isHost;
        string label = EmoteInfo.GetLabel(Emote);
        Color color = EmoteInfo.GetColor(Emote);

        int elapsed = DisplayDuration - TicksLeft;
        
        int baseY = isLocalPlayer ? ArenaMap.Height - 2 : 2;
        int y = isLocalPlayer? baseY - (elapsed / 10) : baseY + (elapsed / 10);

        int x = isLocalPlayer ? 1 : ArenaMap.Width - label.Length - 1;

        int FADE_TICKS = 10;
        if (TicksLeft < FADE_TICKS)
        {
            float alpha = (float)TicksLeft / FADE_TICKS;
            color = color * alpha;
        }
        int[][] emoteBox = new int[][]
        {
            // Row 1:  [Space] ┌  ─  ─  ─  ─  ─  ─  ─  ┐ [Space]
            new int[] { 218, 196, 196, 196, 196, 196, 196, 196, 191,  },
            // Row 2:  [Space] │ [Sp] ╥ [Sp][Sp][Sp] ╥ [Sp] │ [Space]
            new int[] { 179,  32, 210,  32,  32,  32, 210, 32, 179,  },
            // Row 3:  [Space] │ [Sp] ╚  ═  ═  ═  ╝ [Sp] │ [Space]
            new int[] { 179,  32, 200, 205, 205, 205, 188,  32, 179,  },
            // Row 4:  [Space] └  ─  ─  ─  ─  ─  ─  ─  ┘ [Space]
            new int[] { 192, 196, 196, 196, 196, 196, 196, 196, 217,  }
        };

        if (label == "GG!")
        {
            for (int row = 0; row < emoteBox.Length; row++)
            {
                for (int col = 0; col < emoteBox[row].Length; col++)
                {
                    surface.SetGlyph(x + col, y + row, emoteBox[row][col], color, Color.White);
                }
            }
        }
        else
        {
            surface.Print(x, y, label, color, Color.Black);
        }
    }
    
    
}

