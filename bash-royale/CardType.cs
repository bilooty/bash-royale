namespace bash_royale;

public enum CardID
{
    Knight,
    Giant,
    Archer,
    Goblin,
    Wizard,
    Horde
}

public enum CardType
{
    Unit,
    Spell,
    Building
}

public record struct CardInfo(
    CardID ID,
    CardType Type,
    UnitType UnitType,
    int Cost
    );

public static class CardInfos
{
    public static CardInfo GetCardInfo(CardID id) => id switch
    {
        CardID.Knight => new(id, CardType.Unit, UnitType.Knight, 3),
        CardID.Giant => new(id, CardType.Unit, UnitType.Giant, 6),
        CardID.Archer => new(id, CardType.Unit, UnitType.Archer, 3),
        CardID.Horde => new(id, CardType.Unit, UnitType.Horde, 5),
        CardID.Goblin => new(id, CardType.Unit, UnitType.Goblin, 2),
        CardID.Wizard => new(id, CardType.Unit, UnitType.Wizard, 5),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };
}

