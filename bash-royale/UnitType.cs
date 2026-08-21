namespace bash_royale;

public enum UnitType
{
    Knight,
    Giant
}
public record UnitInfo(string Name, int cost);

public static class UnitInfos
{
    public static UnitInfo GetUnitInfo(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Knight => new UnitInfo("Knight", 5),
            UnitType.Giant => new UnitInfo("Giant", 6)
        };
    }
}