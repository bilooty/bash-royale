namespace bash_royale;

public record struct UnitState
{
    public UnitType Type;
    public PlayerId Owner;
    public Vector2Int Position;
    public int Id;
    public int LastAttackTick = -10;
    public int LastDamageTick = -10;
    public IEnumerable<Vector2Int> Positions(){
        UnitInfo info = UnitInfos.GetUnitInfo(Type);
        for (int x = 0; x < info.Size.X; x++)
        {
            for (int y = 0; y < info.Size.Y; y++)
            {
                yield return new Vector2Int(Position.X + x , Position.Y + y);
            }
        }
    }
    public int Health;
    public int MoveProgress;
    // increase by 1 each frame / iteration of unit sim
    // if we have attack 1 per second then we want to do 
    public int Ticks;
    public UnitState(UnitType type, PlayerId owner, Vector2Int position, int id)
    {
        Id = id;
        Type = type;
        Owner = owner;
        Position = position;
        Health = UnitInfos.GetUnitInfo(type).MaxHealth;
    }
}

