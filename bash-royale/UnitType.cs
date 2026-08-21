using System.Drawing;
using System.Numerics;

namespace bash_royale;

public enum UnitType
{
    Knight,
    Giant
}
public record UnitInfo(
    UnitType Type,
    string Name,
    int Cost,
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
                5, 
                700,
                1, 
                1, 
                1,
                1,
                new WalkForwards(5),
                new ChaseBehaviour(5), 
                new AttackBehaviour(75, false)),
                new Vector2Int(1,1),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
}