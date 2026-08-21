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
public record UnitInfo(string Name, int hp, bool flying, string attackType, bool siege, int damage);

public static class UnitInfos
{
    public static UnitInfo GetUnitInfo(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Knight => new UnitInfo("Knight", 10, false, "Melee", false, 2),
            UnitType.Giant => new UnitInfo("Giant", 40, false, "Melee", true, 5),
            UnitType.Archer => new UnitInfo("Archer", 4, false, "Ranged", false,2),
            UnitType.Goblin => new UnitInfo("Goblin", 3, false, "Melee", false, 1),
            UnitType.Wizard => new UnitInfo("Wizard", 8, false, "Ranged", false, 4),
            UnitType.Horde => new UnitInfo("Horde", 20, true, "Melee", false, 2)
        };
    }
}