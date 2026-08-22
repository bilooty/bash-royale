namespace bash_royale;

public enum UnitType
{
    Knight,
    Tower,
    Castle,
    Giant
}
public record UnitInfo(
    UnitType Type,
    string Name,
    
    int MaxHealth,
    int AttackRange,
    int AggroRange,
    int TicksPerAttack,
    IUnitBehaviour NeutralBehaviour,
    IUnitBehaviour ChaseBehaviour,
    IUnitBehaviour AttackBehaviour,
    Vector2Int Size,
    MovementLayer Layer,
    bool IsBuilding = false);
    

public static class UnitInfos
{
    public static UnitInfo GetUnitInfo(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Knight => new UnitInfo(
                UnitType.Knight,
                "Knight",
                700,
                1,
                3,
                20,
                new WalkForwards(5),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75),
                new Vector2Int(1,1),
                MovementLayer.Ground),
            UnitType.Tower => new UnitInfo(
                UnitType.Tower,
                "Tower",
                2000,
                5,
                5,
                10,
                new DoNothing(),
                new DoNothing(),
                new AttackBehaviour(5),
                new Vector2Int(3,3),
                MovementLayer.Ground,
                true),
            UnitType.Castle => new UnitInfo(
                UnitType.Castle,
                "Castle",
                3000,
                5,
                5,
                10,
                new DoNothing(),
                new DoNothing(),
                new AttackBehaviour(5),
                new Vector2Int(3,3),
                MovementLayer.Ground,
                true),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
}