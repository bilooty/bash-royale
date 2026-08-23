namespace bash_royale.Emotes;

public static class EmoteArt
{
    // The top and bottom borders are the same for all boxed emotes
    // Top: [Space] ┌  ─  ─  ─  ─  ─  ─  ─  ┐ [Space]
    private static readonly int[] BoxTop = { 32, 218, 196, 196, 196, 196, 196, 196, 196, 191, 32 };
    
    // Bot: [Space] └  ─  ─  ─  ─  ─  ─  ─  ┘ [Space]
    private static readonly int[] BoxBot = { 32, 192, 196, 196, 196, 196, 196, 196, 196, 217, 32 };

    /// <summary>
    /// Returns a 4x11 grid of CP437 integer indices representing the emote's artwork.
    /// </summary>
    public static int[][] GetArt(EmoteId id)
    {
        return id switch
        {
            EmoteId.Laugh => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 210, 32,  32,  32, 210, 32, 179, 32 }, // │ ╥   ╥ │
                new[] { 32, 179, 32, 200, 205, 205, 205, 188, 32, 179, 32 }, // │ ╚═══╝ │
                BoxBot
            },
            EmoteId.Angry => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 62,  32,  32,  32, 60,  32, 179, 32 }, // │ >   < │
                new[] { 32, 179, 32, 32, 196, 196, 196, 32,  32, 179, 32 }, // │  ───  │
                BoxBot
            },
            EmoteId.Cry => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 210, 32,  32,  32, 210, 32, 179, 32 }, // │ ╥   ╥ │
                new[] { 32, 179, 32, 39,  32, 196,  32, 39,  32, 179, 32 }, // │ ' ─ ' │
                BoxBot
            },
            EmoteId.Usuck => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 79,  32,  32,  32, 79,  32, 179, 32 }, // │ O   O │
                new[] { 32, 179, 32, 32,  32,  79,  32, 32,  32, 179, 32 }, // │   O   │
                BoxBot
            },
            EmoteId.GoodGame => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 94,  32,  32,  32, 94,  32, 179, 32 }, // │ ^   ^ │
                new[] { 32, 179, 32, 32,  92,  32,  47, 32,  32, 179, 32 }, // │  \ /  │
                BoxBot
            },
            EmoteId.Thanks => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 94,  32,  32,  32, 94,  32, 179, 32 }, // │ ^   ^ │
                new[] { 32, 179, 32, 32, 196, 196, 196, 32,  32, 179, 32 }, // │  ───  │
                BoxBot
            },
            _ => new[]
            {
                BoxTop,
                new[] { 32, 179, 32, 63,  32,  32,  32, 63,  32, 179, 32 }, // │ ?   ? │
                new[] { 32, 179, 32, 32, 196, 196, 196, 32,  32, 179, 32 }, // │  ───  │
                BoxBot
            }
        };
    }
}
