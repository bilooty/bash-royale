namespace bash_royale.Emotes;

public enum EmoteId
{
    GoodGame,
    Thanks,
    Laugh,
    Usuck,
    Angry,
    Cry,
}

public static class EmoteInfo
{
    public static string GetLabel(EmoteId id) => id
        switch
        {
            EmoteId.GoodGame => "GG!",
            EmoteId.Thanks => "THX",
            EmoteId.Usuck => "U SUCK",
            EmoteId.Angry => ">:(",
            EmoteId.Cry => "T_T",
            EmoteId.Laugh => "HEHEHAHA",
            _ => "???",
        };
    
    public static Color GetColor(EmoteId id) => id switch
    {
        EmoteId.GoodGame => Color.LimeGreen,
        EmoteId.Thanks   => Color.Cyan,
        EmoteId.Usuck      => Color.Yellow,
        EmoteId.Angry    => Color.Red,
        EmoteId.Cry      => Color.LightBlue,
        EmoteId.Laugh    => Color.Orange,
        _                => Color.White,
    };

    public static string GetSound(EmoteId id) => id switch
    {
        EmoteId.GoodGame => "GG_placed",
        EmoteId.Thanks   => "Thanks_placed",
        EmoteId.Usuck      => "Usuck_placed",
        EmoteId.Angry    => "emote_angry",
        EmoteId.Cry      => "emote_cry",
        EmoteId.Laugh    => "Haha_placed",
        _                => "",
    };
}