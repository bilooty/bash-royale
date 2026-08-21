namespace bash_royale;

public struct UnitState
{
    public UnitType Type;
    public PlayerId Owner;
    public Vector2Int Position;
    public int Health;
    public float AttackCooldown;
    public float MoveProgress;

    public UnitState(UnitType type, PlayerId owner, Vector2Int position)
    {
        Type = type;
        Owner = owner;
        Position = position;
        Health = UnitInfos.GetUnitInfo(type).MaxHealth;
        AttackCooldown = 0f;
        MoveProgress = 0f;
    }
}

public static class UnitSim
{
    // Advances a unit by one grid cell in `direction` once enough movement has
    // accumulated, so units with different Speed values still move at the right pace.
    public static UnitState Step(UnitState unit, Vector2Int direction, float deltaSeconds)
    {
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        unit.MoveProgress += info.Speed * deltaSeconds;

        while (unit.MoveProgress >= 1f)
        {
            unit.MoveProgress -= 1f;
            unit.Position += direction;
        }

        return unit;
    }
}