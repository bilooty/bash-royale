namespace bash_royale;

public class Occupancy
{
    private readonly bool[,] _ground = new bool[ArenaMap.Width, ArenaMap.Height];
    private readonly bool[,] _air = new bool[ArenaMap.Width, ArenaMap.Height];

    private bool[,] GridFor(MovementLayer layer) => layer == MovementLayer.Air ? _air : _ground;

    public bool IsFree(Vector2Int position, MovementLayer layer) => !GridFor(layer)[position.X, position.Y];
    public void Occupy(Vector2Int position, MovementLayer layer) => GridFor(layer)[position.X, position.Y] = true;
    public void Vacate(Vector2Int position, MovementLayer layer) => GridFor(layer)[position.X, position.Y] = false;

    public static Occupancy Build(GameState state)
    {
        Occupancy occupancy = new();
        occupancy.Add(state.PlayerOne.Units);
        occupancy.Add(state.PlayerTwo.Units);
        return occupancy;
    }

    private void Add(List<UnitState> units)
    {
        foreach (UnitState unit in units)
        {
            Occupy(unit.Position, UnitInfos.GetUnitInfo(unit.Type).Layer);
        }
    }
}