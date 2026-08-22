using bash_royale.Emotes;
using SadConsole.Input;

namespace bash_royale.Rendering;

public class EmoteManager
{
    private readonly List<EmoteDisplay> _activeEmotes = new();
    private int _cooldownTicks = 0;
    public bool IsMenuOpen { get; private set; } = false;

    private const int COOLDOWN_TICKS = 60;

    // The 4 emotes available in the menu
    private static readonly EmoteId[] MenuEmotes =
    {
        EmoteId.GoodGame, EmoteId.Thanks, EmoteId.Wow, EmoteId.Laugh
    };

    private static readonly Keys[] SelectKeys =
    {
        Keys.D1, Keys.D2, Keys.D3, Keys.D4
    };

    /// <summary>
    /// Call first in ProcessKeyboard. Returns true if the emote system consumed the input.
    /// </summary>
    public bool HandleInput(Keyboard keyboard, out EmoteId? selectedEmote)
    {
        selectedEmote = null;

        if (keyboard.IsKeyPressed(Keys.E))
        {
            IsMenuOpen = !IsMenuOpen;
            return true;
        }
        if (IsMenuOpen)
        {
            for (int i = 0; i < SelectKeys.Length; i++)
            {
                if (!keyboard.IsKeyPressed(SelectKeys[i])) continue;

                IsMenuOpen = false;

                if (_cooldownTicks > 0) return true; 

                _cooldownTicks = COOLDOWN_TICKS;
                selectedEmote = MenuEmotes[i];
                return true;
            }

            return true;
        }

        return false;
    }

    public void Show(EmoteId emote, PlayerId owner)
    {
        _activeEmotes.Add(new EmoteDisplay(emote, owner));
    }

    public void Update(ScreenSurface surface, bool isHost)
    {
        if (_cooldownTicks > 0) _cooldownTicks--;

        for (int i = _activeEmotes.Count - 1; i >= 0; i--)
        {
            _activeEmotes[i].Tick();
            if (_activeEmotes[i].TicksUp)
                _activeEmotes.RemoveAt(i);
            else
                _activeEmotes[i].Draw(surface, isHost);
        }

        if (IsMenuOpen)
            DrawMenu(surface);
    }

    private void DrawMenu(ScreenSurface surface)
    {
        int menuX = 0;
        int menuY = ArenaMap.Height - 6;
        int menuWidth = 8;
        int menuHeight = 6;

        // Draw black box
        for (int y = menuY; y < menuY + menuHeight; y++)
        {
            for (int x = menuX; x < menuX + menuWidth; x++)
            {
                if (x >= 0 && x < surface.Surface.Width && y >= 0 && y < surface.Surface.Height)
                {
                    surface.Surface[x, y].Background = Color.Black;
                    surface.Surface[x, y].GlyphCharacter = ' ';
                }
            }
        }

        // Draw header
        surface.Print(menuX + 1, menuY, "EMOTE", Color.White, Color.Black);

        // Draw 4 options
        for (int i = 0; i < MenuEmotes.Length; i++)
        {
            string label = $"{i + 1}.{EmoteInfo.GetLabel(MenuEmotes[i])}";
            Color color = EmoteInfo.GetColor(MenuEmotes[i]);
            surface.Print(menuX + 1, menuY + 1 + i, label, color, Color.Black);
        }
    }
}