namespace bash_royale;

public static class ArenaMap
{
    // Automatically calculate dimensions so they never go out of sync with the template!
    public static readonly int Height;
    public static readonly int Width;

    // The visual template of the Clash Royale arena (24x32)
    // . = Grass | ~ = Water | = = Bridge
    public static readonly string[] MapTemplate = new string[]
    {
        "............................",
        "............................",
        "............................", // Enemy King Tower
        "............................",
        "............................",
        "............................", // Enemy Princess Towers
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "~~~~====~~~~~~~~~~~~====~~~~", // The River & Bridges (4 wide)
        "~~~~====~~~~~~~~~~~~====~~~~",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................",
        "............................", // Player Princess Towers
        "............................",
        "............................",
        "............................", // Player King Tower
        "............................",
        "............................", 
        // 32nd row
    };

    // The actual deterministic data structure used by the engine
    public static readonly TileType[,] Grid;

    static ArenaMap()
    {
        // Calculate based on the string array
        Height = MapTemplate.Length;
        Width = MapTemplate[0].Length;

        Grid = new TileType[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char tileChar = MapTemplate[y][x];
                
                Grid[x, y] = tileChar switch
                {
                    '.' => TileType.Grass,
                    '~' => TileType.Water,
                    '=' => TileType.Bridge,
                    _   => TileType.Grass 
                };
            }
        }
    }

    public static bool IsPassable(Vector2Int position, MovementLayer layer)
    {
        if (position.X < 0 || position.X >= Width) return false;
        if (position.Y < 0 || position.Y >= Height) return false;
        if (layer == MovementLayer.Air) return true;
        return Grid[position.X, position.Y] != TileType.Water;
    }
}