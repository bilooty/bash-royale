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
    HogRider,

}
public record UnitInfo(
    UnitType Type,
    string Name,
    
    int MaxHealth,
    int AttackRange,
    int AggroRange,
    int TicksPerAttack,
    bool ranged,
    IUnitBehaviour NeutralBehaviour,
    IUnitBehaviour ChaseBehaviour,
    IUnitBehaviour AttackBehaviour,
    Vector2Int Size,
    MovementLayer Layer,
    bool IsBuilding = false,
    bool targetsBuildingsOnly = false);

public static class UnitInfos
{
    public static UnitInfo GetUnitInfo(UnitType unitType)
    {
        return unitType switch
        {
                        UnitType.Knight => new UnitInfo(
                UnitType.Knight,
                "Knight",
                1400,
                1,
                6,
                24,                        // 1.2s hit speed
                false,
                new WalkForwards(5),
                new ChaseBehaviour(5),
                new AttackBehaviour(160),
                new Vector2Int(1,1),
                MovementLayer.Ground),
            UnitType.Tower => new UnitInfo(
                UnitType.Tower,
                "Tower",
                1400,
                7,
                7,
                16,                        // 0.8s
                true,
                new DoNothing(),
                new DoNothing(),
                new AttackBehaviour(90),
                new Vector2Int(1,1),
                MovementLayer.Ground,
                true),
            UnitType.Castle => new UnitInfo(
                UnitType.Castle,
                "Castle",
                2400,
                7,
                7,
                20,                        // 1.0s
                true,
                new DoNothing(),
                new DoNothing(),
                new AttackBehaviour(110),
                new Vector2Int(1,1),
                MovementLayer.Ground,
                true),
            UnitType.Giant => new UnitInfo(
                UnitType.Giant,
                "Giant",
                2500,
                1,
                5,
                30,                        // 1.5s
                false,
                new WalkForwards(3),       // slow
                new ChaseBehaviour(3),
                new AttackBehaviour(210),
                new Vector2Int(2,2),
                MovementLayer.Ground,
                false,
                true),
            UnitType.Archer => new UnitInfo(
                UnitType.Archer,
                "Archer",
                250,
                7,
                9,
                24,                        // 1.2s
                true,
                new WalkForwards(5),
                new ChaseBehaviour(5),
                new AttackBehaviour(85),
                new Vector2Int(1,1),
                MovementLayer.Ground),
            UnitType.Goblin => new UnitInfo(
                UnitType.Goblin,
                "Goblin",
                170,
                1,
                8,
                22,                        // 1.1s
                false,
                new WalkForwards(8),       // very fast
                new ChaseBehaviour(8),
                new AttackBehaviour(110),
                new Vector2Int(1,1),
                MovementLayer.Ground),
            UnitType.Wizard => new UnitInfo(
                UnitType.Wizard,
                "Wizard",
                450,
                8,
                10,
                28,                        // 1.4s
                true,
                new WalkForwards(5),
                new ChaseBehaviour(5),
                new AttackBehaviour(220),
                new Vector2Int(1,1),
                MovementLayer.Ground),
            UnitType.HogRider => new UnitInfo(
                UnitType.HogRider,
                "Hog Rider",
                1200,
                1,
                5,
                32,                        // 1.6s
                false,
                new WalkForwards(9),       // very fast
                new ChaseBehaviour(9),
                new AttackBehaviour(240),
                new Vector2Int(1,1),
                MovementLayer.Ground,
                false,
                true),                     // buildings only
                
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
}