namespace bash_royale;

public struct UnitState
{
    public UnitType Type;
    public PlayerId Owner;
    public Vector2Int Position;
    public int Health;
    public int MoveProgress;
    public IUnitBehaviour UnitBehaviour;
    // increase by 1 each frame / iteration of unit sim
    // if we have attack 1 per second then we want to do 
    public int Ticks;
    public UnitState(UnitType type, PlayerId owner, Vector2Int position)
    {
        Type = type;
        Owner = owner;
        Position = position;
        Health = UnitInfos.GetUnitInfo(type).MaxHealth;
    }
}

