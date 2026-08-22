using System;
using SadConsole.Input;

namespace bash_royale.Rendering;

/// <summary>
/// Clash Royale style deck builder: the eight cards you take into battle sit at the
/// top, the whole collection below. Clicking a collection card adds it (or takes it
/// back out), clicking a deck card removes it.
/// </summary>
public class DeckScreen : SadConsole.ScreenSurface
{
    private const int CardWidth = 6;
    private const int CardHeight = 3;
    private const int Columns = 4;

    private const int DeckStartX = 1;
    private const int DeckStartY = 4;
    private const int CollectionStartX = 1;
    private const int CollectionStartY = 16;

    private readonly List<CardId> _deck;
    private readonly IReadOnlyList<CardId> _collection = CardInfos.AllCards;

    private readonly Rectangle _saveButton = new(1, 34, 13, 3);
    private readonly Rectangle _backButton = new(14, 34, 13, 3);

    private Point? _hover;
    private string _message = "Click a card below to add it.";
    private Color _messageColor = Color.Gray;

    public DeckScreen() : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
    {
        // Work on a copy so cancelling leaves the saved deck untouched.
        _deck = new List<CardId>(Decks.Current);

        UseMouse = true;
        UseKeyboard = true;
        IsFocused = true;

        Redraw();
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            GoBack();
            return true;
        }

        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            SaveAndLeave();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        Point? previousHover = _hover;
        _hover = state.IsOnScreenObject ? state.CellPosition : null;
        if (_hover != previousHover) Redraw();

        if (!state.Mouse.LeftClicked || !state.IsOnScreenObject)
            return base.ProcessMouse(state);

        Point cell = state.CellPosition;

        if (_saveButton.Contains(cell))
        {
            SaveAndLeave();
            return true;
        }

        if (_backButton.Contains(cell))
        {
            GoBack();
            return true;
        }

        if (HitTest(cell, DeckStartX, DeckStartY, _deck.Count) is int deckIdx)
        {
            CardId removed = _deck[deckIdx];
            _deck.RemoveAt(deckIdx);
            SetMessage("Removed " + CardInfos.GetName(removed), Color.Orange);
            Redraw();
            return true;
        }

        if (HitTest(cell, CollectionStartX, CollectionStartY, _collection.Count) is int cardIdx)
        {
            ToggleCard(_collection[cardIdx]);
            Redraw();
            return true;
        }

        return base.ProcessMouse(state);
    }

    private void ToggleCard(CardId card)
    {
        if (_deck.Remove(card))
        {
            SetMessage("Removed " + CardInfos.GetName(card), Color.Orange);
            return;
        }

        if (_deck.Count >= Decks.DECK_SIZE)
        {
            SetMessage("Deck is full - remove a card first.", Color.Red);
            return;
        }

        _deck.Add(card);
        SetMessage("Added " + CardInfos.GetName(card), Color.LightGreen);
    }

    private void SaveAndLeave()
    {
        if (!Decks.Save(_deck))
        {
            SetMessage("Pick exactly " + Decks.DECK_SIZE + " cards to save.", Color.Red);
            Redraw();
            return;
        }

        GoBack();
    }

    private static void GoBack()
    {
        var startScreen = new StartScreen();
        SadConsole.Game.Instance.Screen = startScreen;
        startScreen.IsFocused = true;
    }

    private void SetMessage(string message, Color color)
    {
        _message = message;
        _messageColor = color;
    }

    /// <summary>Returns the index of the card slot under the given cell, or null.</summary>
    private static int? HitTest(Point cell, int startX, int startY, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (SlotBounds(startX, startY, i).Contains(cell)) return i;
        }
        return null;
    }

    private static Rectangle SlotBounds(int startX, int startY, int index)
    {
        int column = index % Columns;
        int row = index / Columns;
        return new Rectangle(
            startX + column * (CardWidth + 1),
            startY + row * (CardHeight + 1),
            CardWidth,
            CardHeight);
    }

    private void Redraw()
    {
        Surface.Clear();
        Surface.DefaultBackground = Color.Black;
        Surface.Fill(Color.White, Color.Black, ' ');

        Surface.Print(Center("DECK BUILDER"), 1, "DECK BUILDER", Color.Cyan);

        bool complete = _deck.Count == Decks.DECK_SIZE;
        string deckHeader = "YOUR DECK  " + _deck.Count + "/" + Decks.DECK_SIZE;
        Surface.Print(1, 3, deckHeader, complete ? Color.LightGreen : Color.Yellow);
        Surface.Print(1 + deckHeader.Length + 1, 3, complete ? "READY" : "INCOMPLETE",
            complete ? Color.LightGreen : Color.Red);

        for (int i = 0; i < Decks.DECK_SIZE; i++)
        {
            Rectangle bounds = SlotBounds(DeckStartX, DeckStartY, i);
            if (i < _deck.Count)
                DrawCard(bounds, _deck[i], Color.Cyan, inDeck: true);
            else
                DrawEmptySlot(bounds);
        }

        Surface.Print(1, 15, "COLLECTION", Color.Yellow);

        for (int i = 0; i < _collection.Count; i++)
        {
            CardId card = _collection[i];
            bool inDeck = _deck.Contains(card);
            DrawCard(SlotBounds(CollectionStartX, CollectionStartY, i), card,
                inDeck ? Color.DarkGreen : Color.White, inDeck);
        }

        Surface.Print(1, 32, _message.PadRight(36).Substring(0, 36), _messageColor);
        Surface.Print(1, 33, "Enter = save, Esc = back".PadRight(36), Color.Gray);

        DrawButton(_saveButton, "SAVE & BACK", complete ? Color.LightGreen : Color.Gray);
        DrawButton(_backButton, "CANCEL", Color.Orange);
    }

    private int Center(string text) => (Surface.Width - text.Length) / 2;

    private void DrawButton(Rectangle bounds, string text, Color color)
    {
        bool hovered = _hover is Point p && bounds.Contains(p);
        DrawBox(bounds, hovered ? Color.White : color);
        Surface.Print(bounds.X + (bounds.Width - text.Length) / 2, bounds.Y + 1, text,
            hovered ? Color.White : color);
    }

    private void DrawCard(Rectangle bounds, CardId card, Color color, bool inDeck)
    {
        bool hovered = _hover is Point p && bounds.Contains(p);
        Color border = hovered ? Color.White : color;

        DrawBox(bounds, border);

        CardInfo info = CardInfos.GetCardInfo(card);
        string name = CardInfos.GetName(card);
        if (name.Length > CardWidth - 2) name = name.Substring(0, CardWidth - 2);

        Surface.Print(bounds.X + 1, bounds.Y + 1, name, border);
        Surface.SetGlyph(bounds.X + 1, bounds.Y + 2, CardInfos.GetGlyph(card),
            inDeck ? Color.LightGreen : Color.LightGray);
        Surface.Print(bounds.X + CardWidth - 2, bounds.Y + 2, info.Cost.ToString(), Color.Magenta);

        // A tick in the corner: this card is already in the deck.
        if (inDeck) Surface.SetGlyph(bounds.X + CardWidth - 2, bounds.Y, 251, Color.LightGreen);
    }

    private void DrawBox(Rectangle bounds, Color color)
    {
        int right = bounds.X + bounds.Width - 1;
        int bottom = bounds.Y + bounds.Height - 1;

        Surface.SetGlyph(bounds.X, bounds.Y, 218, color);
        Surface.SetGlyph(right, bounds.Y, 191, color);
        Surface.SetGlyph(bounds.X, bottom, 192, color);
        Surface.SetGlyph(right, bottom, 217, color);

        for (int x = bounds.X + 1; x < right; x++)
        {
            Surface.SetGlyph(x, bounds.Y, 196, color);
            Surface.SetGlyph(x, bottom, 196, color);
        }

        for (int y = bounds.Y + 1; y < bottom; y++)
        {
            Surface.SetGlyph(bounds.X, y, 179, color);
            Surface.SetGlyph(right, y, 179, color);
        }
    }

    private void DrawEmptySlot(Rectangle bounds)
    {
        DrawBox(bounds, Color.DarkGray);
        Surface.Print(bounds.X + 2, bounds.Y + 2, "----", Color.DarkGray);
    }
}
