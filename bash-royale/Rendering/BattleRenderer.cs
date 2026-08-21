namespace bash_royale.Scenes;

// UI/BattleRenderer.cs
public class BattleRenderer : SadConsole.ScreenSurface
{
    private GameState _gameState;
    
    // Timer to handle the fixed timestep
    private double _timeSinceLastTick = 0;
    private const double TickRate = 1.0 / 10.0; // 10 ticks per second

    public BattleRenderer() : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
    {
        // 1. Initialize your deterministic engine
        _gameState = new GameState();
        // 3. Draw the static map onto the base surface once
        DrawArena();
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

    public override void Update(TimeSpan delta)
    {
        //
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
    private void DrawState()
    {
        Surface.Clear();

        // Draw Arena (River, Bridges, Towers)
        
        
    }
}