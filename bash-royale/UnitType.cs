namespace bash_royale;

public enum UnitType
{
    Knight,
    Giant
}
public record UnitInfo(string Name, int Cost, int MaxHealth, int Damage, float Range, float Speed, float AttacksPerSecond);

public static class UnitInfos
{
    public static UnitInfo GetUnitInfo(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Knight => new UnitInfo("Knight", Cost: 5, MaxHealth: 700, Damage: 75, Range: 1f, Speed: 1.5f, AttacksPerSecond: 1.2f),
            UnitType.Giant => new UnitInfo("Giant", Cost: 6, MaxHealth: 2000, Damage: 120, Range: 1f, Speed: 1f, AttacksPerSecond: 1f),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };
    }
}