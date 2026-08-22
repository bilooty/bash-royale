namespace bash_royale;

public enum UnitType
{
    Knight,
    Tower,
    Castle,
    Giant,
    Archer,
    Goblin,
    Wizard,
    Horde,

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
                new ChaseBehaviour(10), 
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
                new Vector2Int(1,1),
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
                new Vector2Int(1,1),
                MovementLayer.Ground,
                true),
                UnitType.Giant => new UnitInfo(
                UnitType.Giant,
                "Giant",
                1500,
                1,
                5,
                20,
                new WalkForwards(5),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75),
                new Vector2Int(2,2),
                MovementLayer.Ground),
                UnitType.Archer => new UnitInfo(
                UnitType.Archer,
                "Archer",
                500,
                5,
                5,
                10,
                new WalkForwards(5),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75),
                new Vector2Int(1,1),
                MovementLayer.Ground),
                UnitType.Goblin => new UnitInfo(
                UnitType.Goblin,
                "Goblin",
                200,
                1,
                3,
                5,
                new WalkForwards(8),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75),
                new Vector2Int(1,1),
                MovementLayer.Ground),
                UnitType.Wizard => new UnitInfo(
                UnitType.Wizard,
                "Wizard",
                600,
                5,
                5,
                10,
                new WalkForwards(5),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75),
                new Vector2Int(1,1),
                MovementLayer.Ground),
                UnitType.Horde => new UnitInfo(
                UnitType.Horde,
                "Horde",
                800,
                1,
                3,
                10,
                new WalkForwards(5),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75),
                new Vector2Int(1,1),
                MovementLayer.Ground),
                
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
}