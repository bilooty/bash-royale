namespace bash_royale.Emotes;

public enum EmoteId
{
    GoodGame,
    Thanks,
    Laugh,
    Wow,
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
            EmoteId.Wow => "WOW",
            EmoteId.Angry => ">:(",
            EmoteId.Cry => "T_T",
            EmoteId.Laugh => "LOL",
            _ => "???",
        };
    
    public static Color GetColor(EmoteId id) => id switch
    {
        EmoteId.GoodGame => Color.LimeGreen,
        EmoteId.Thanks   => Color.Cyan,
        EmoteId.Wow      => Color.Yellow,
        EmoteId.Angry    => Color.Red,
        EmoteId.Cry      => Color.LightBlue,
        EmoteId.Laugh    => Color.Orange,
        _                => Color.White,
    };
}