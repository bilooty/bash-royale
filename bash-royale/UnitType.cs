using System.Drawing;
using System.Numerics;

namespace bash_royale;

public enum UnitType
{
    Knight,
    Giant,
    Archer,
    Goblin,
    Wizard
}
public record UnitInfo(
    UnitType Type,
    string Name,
    int MaxHealth,
    int Damage,
    int Range,
    int Speed,
    int AttacksPerTwentyTicks,
    IUnitBehaviour NeutralBehaviour,
    IUnitBehaviour ChaseBehaviour,
    IUnitBehaviour AttackBehaviour,
    Vector2Int Size,
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
                6, 
                2,
                1, 
                1, 
                1,
                new WalkForwards(60),
                new ChaseBehaviour(60),
                new AttackBehaviour(75, false),
                new Vector2Int(1,1)),
            
            UnitType.Giant => new UnitInfo(
                UnitType.Giant,
                "Giant",
                6,
                1,
                1,
                1,
                1,
                new WalkForwards(30),
                new ChaseBehaviour(30),
                new AttackBehaviour(100, false),
                new Vector2Int(2,2)
                
                
                ),
                
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
}