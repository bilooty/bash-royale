using System.Runtime.CompilerServices;
using SadConsole.Input;

namespace bash_royale.Scenes;

// UI/BattleRenderer.cs
public class BattleRenderer : SadConsole.ScreenSurface
{
    private static readonly Keys[] HandSlotKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };

    private GameState _gameState;
    private ScreenSurface _unitLayer;
    private ScreenSurface _guiLayer;
    private double _timer = 0.05f;
    private int tick = 0;
    public BattleRenderer() : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
    {
        // 1. Initialize your deterministic engine
        _gameState = GameState.CreateNew();
        _unitLayer = new ScreenSurface(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT);
        _unitLayer.Surface.DefaultBackground = Color.Transparent;
        _guiLayer = new ScreenSurface(28, 8);
        _guiLayer.Surface.DefaultBackground = Color.Transparent;
        _guiLayer.Position = new Point(0, ArenaMap.Height); 
        Children.Add(_guiLayer);
        _gameState = GameState.CreateNew();
        PlayerState p1 = _gameState.PlayerOne;
        p1.Units.Add(new UnitState(UnitType.Castle, PlayerId.Two, new Vector2Int(10, 4)));
        p1.Units.Add(new UnitState(UnitType.Knight, PlayerId.One, new Vector2Int(5, 5)));
        PlayerState p2 = _gameState.PlayerTwo;
        p2.Units.Add(new UnitState(UnitType.Knight, PlayerId.Two, new Vector2Int(10, 29)));
        p2.Units.Add(new UnitState(UnitType.Castle, PlayerId.Two, new Vector2Int(10, 30)));
        _gameState.PlayerOne = p1;
        Children.Add(_unitLayer);
        // 3. Draw the static map onto the base surface once
        DrawArena();

        UseKeyboard = true;
        IsFocused = true;
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        for (int i = 0; i < HandSlotKeys.Length; i++)
        {
            if (keyboard.IsKeyPressed(HandSlotKeys[i]))
            {
                Vector2Int deployPosition = new Vector2Int(ArenaMap.Width / 2, ArenaMap.Height - 5);
                _gameState = CardSim.PlayFromHand(_gameState, PlayerId.One, i, deployPosition);
                return true;
            }
        }

        return base.ProcessKeyboard(keyboard);
    }
    private bool ShouldDrawSprout(int x, int y)
    {
        // Use large prime numbers to create a chaotic but repeatable hash
        uint hash = (uint)x * 374761393u ^ (uint)y * 668265263u;
    
        // Mix the bits further to eliminate any remaining geometric patterns
        hash = (hash ^ (hash >> 13)) * 1274126177u;
    
        // Use modulo to set the density. 
        // Example: hash % 100 < 15 means a ~15% chance for a sprout.
        return (hash % 100) < 15; 
    }
    

    public void DrawUnits(PlayerState player)
    {
        foreach (UnitState unit in player.Units)
        {
            Vector2Int pos = unit.Position;
            UnitDisplay display = UnitDisplay.Displays[unit.Type];
            Vector2Int size = UnitInfos.GetUnitInfo(unit.Type).Size;
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    ColoredGlyph glyph = display.Glyphs[y][x];
                    _unitLayer.Surface[pos.X, pos.Y].Background = glyph.Background;
                    _unitLayer.Surface[pos.X, pos.Y].Foreground = glyph.Foreground;
                    _unitLayer.Surface[pos.X, pos.Y].GlyphCharacter = glyph.GlyphCharacter;
                }
            }
        }
    }
    public override void Update(TimeSpan delta)
    {
        _gameState.PlayerOne.Elixir = PlayerSim.RegenerateElixir(_gameState.PlayerOne.Elixir, (float)delta.TotalSeconds);
        _gameState.PlayerTwo.Elixir = PlayerSim.RegenerateElixir(_gameState.PlayerTwo.Elixir, (float)delta.TotalSeconds);

        _unitLayer.Surface.Clear();
        _guiLayer.Surface.Clear();
        DrawUnits(_gameState.PlayerOne);
        DrawUnits(_gameState.PlayerTwo);
        DrawGUI();
        _timer -= delta.TotalSeconds;
        if (_timer <= 0f)
        {
            _timer = 0.05;
            tick++;
            _gameState = GameSim.Update(_gameState);
        }
        DrawUnits(_gameState.PlayerOne);
        DrawUnits(_gameState.PlayerTwo);
        base.Update(delta);
    }
    private void DrawArena()
    {
        for (int y = 0; y < ArenaMap.Height; y++)
        {
            for (int x = 0; x < ArenaMap.Width; x++)
            {
                TileType tile = ArenaMap.Grid[x, y];

                ColoredGlyph cellAppearance;

                switch (tile)
                {
                    case TileType.Grass:
                        ColoredGlyph baseGrass = new ColoredGlyph(Color.LightGreen, Color.LightGreen, ' ');
                        ColoredGlyph grassSprout = new ColoredGlyph(Color.LightSeaGreen, Color.LightGreen, '"');
                        cellAppearance = baseGrass;
                        
                        // Your modulus logic for random sprouts
                        if (ShouldDrawSprout(x,y))
                        {
                            cellAppearance = grassSprout;
                        }
                        break;
                
                    case TileType.Water:
                        // You can use a similar trick here to make the water look animated or textured later!
                        cellAppearance = new ColoredGlyph(Color.Cyan, Color.Aquamarine, ' ');
                        break;
                
                    case TileType.Bridge:
                        cellAppearance = new ColoredGlyph(Color.BurlyWood, Color.BurlyWood, ' ');
                        break;
                        
                    default:
                        cellAppearance = new ColoredGlyph(Color.White, Color.Black, ' ');
                        break;
                }

                // Apply the visual state to the specific grid coordinate on the screen
                Surface.SetCellAppearance(x, y, cellAppearance);
            }
        }
    }

    private void DrawGUI()
    {
        _guiLayer.Surface.Clear();
        _guiLayer.Surface.DrawBox(
            new Rectangle(0, 0, _guiLayer.Surface.Width, _guiLayer.Surface.Height),
            ShapeParameters.CreateBorder(new ColoredGlyph(Color.Black, Color.Gray)));
        
        _guiLayer.Surface.Print(2, 1, "=== HAND ===", Color.Yellow);
        int cardWidth = 7;
        int cardHeight = 5;
        int spacing = 2;
        int startX = 2;
        int startY = 2;

        PlayerState player = _gameState.PlayerOne;
        for (int i = 0; i < player.Hand.Count; i++)
        {
            CardInfo card = CardInfos.GetCardInfo(player.Hand[i]);
            Color color = player.Elixir >= card.Cost ? Color.Cyan : Color.Gray;
            int cardX = startX + (i * (cardWidth + spacing));
            int cardY = startY;
            _guiLayer.Surface.DrawBox(
                new Rectangle(cardX, cardY, cardWidth, cardHeight),
                ShapeParameters.CreateBorder(new ColoredGlyph(card.Color, Color.Black, 0))
            );  
        }

        _guiLayer.Surface.Print(25, 2, $"Elixir: {player.Elixir:0.0} / {GameSettings.MAX_ELIXIR:0}", Color.Magenta);
    }
    private void DrawState()
    {
        Surface.Clear();

        // Draw Arena (River, Bridges, Towers)
        
        
    }
}