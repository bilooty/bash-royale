namespace bash_royale;

public enum UnitType
{
    Knight,
    Giant,
    Archer,
    Goblin,
    Wizard,
    Horde
}
public record UnitInfo(string Name, int hp, bool flying, string attackType, bool siege, int damage, Vector2Int Size);

public static class UnitInfos
{
    public static UnitInfo GetUnitInfo(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Knight => new UnitInfo("Knight", 10, false, "Melee", false, 2, new Vector2Int(1,1)),
            UnitType.Giant => new UnitInfo("Giant", 40, false, "Melee", true, 5, new Vector2Int(2,2)),
            UnitType.Archer => new UnitInfo("Archer", 4, false, "Ranged", false,2, new Vector2Int(1,1)),
            UnitType.Goblin => new UnitInfo("Goblin", 3, false, "Melee", false, 1, new Vector2Int(1,1)),
            UnitType.Wizard => new UnitInfo("Wizard", 8, false, "Ranged", false, 4, new Vector2Int(1,1)),
            UnitType.Horde => new UnitInfo("Horde", 20, true, "Melee", false, 2, new Vector2Int(1,1))
        };
    }
}