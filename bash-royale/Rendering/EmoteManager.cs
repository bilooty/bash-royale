using bash_royale.Emotes;
using SadConsole.Input;

namespace bash_royale.Rendering;

public class EmoteManager
{
    private readonly List<EmoteDisplay> _activeEmotes = new();
    private int _cooldownTicks = 0;
    
    private const int COOLDOWN_TICKS = 60;

    private static readonly Keys[] EmoteKeys =
    {
        Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, Keys.NumPad0
    };

    public EmoteId? ProcessInput(Keyboard keyboard)
    {
        if (_cooldownTicks > 0 ) return null;

        for (int i = 0; i < EmoteKeys.Length; i++)
        {
            if (!keyboard.IsKeyPressed(EmoteKeys[i])) continue;
            _cooldownTicks = COOLDOWN_TICKS;
            return (EmoteId)i;
            
        }

        return null;
    }

    public void Show(EmoteId emote, PlayerId owner)
    {
        _activeEmotes.RemoveAll (e => e.Owner == owner);
        _activeEmotes.Add(new EmoteDisplay(emote, owner));
    }
    public void Update(ScreenSurface surface, bool isHost){
        if (_cooldownTicks > 0) _cooldownTicks--;
        for (int i = _activeEmotes.Count - 1; i >= 0; i--)
        {
            _activeEmotes[i].Tick();
            if (_activeEmotes[i].TicksUp)
                _activeEmotes.RemoveAt(i);
            else
                _activeEmotes[i].Draw(surface, isHost);
        }
}
}