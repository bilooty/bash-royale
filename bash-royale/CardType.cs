namespace bash_royale;

public enum CardId
{
    Knight,
    Giant,
    Archer,
    Goblin,
    Wizard,
    Horde,
    FireBall,
}

public enum ValidLocation
{
    YourSide,
    BothSides
}

public enum CardType
{
    Unit,
    Spell,
}

public record UnitCard(CardId Id, int Cost, UnitType UnitType) : CardInfo(Id, CardType.Unit, Cost, ValidLocation.YourSide);

public record SpellCard(CardId Id, int Cost, SpellType SpellType) : CardInfo(Id, CardType.Spell, Cost, ValidLocation.BothSides);

public record CardInfo(
    CardId Id,
    CardType Type,
    int Cost,
    ValidLocation ValidLocation
    );

public static class CardSim
{
    public static GameState PlayCard(CardInfo card, GameState gameState, PlayerId playerId, Vector2Int position)
    {
        PlayerState player = playerId == PlayerId.One ? gameState.PlayerOne : gameState.PlayerTwo;
        if (player.Elixir - card.Cost < 0)
        {
            return gameState;
        }

        player.Elixir -= card.Cost;
        if (card is UnitCard unitCard)
        {
            player.Units.Add(new UnitState(
                unitCard.UnitType, playerId, position));

        }

        if (card is SpellCard spellCard)
        {
            //
        }

        if (playerId == PlayerId.One)
        {
            gameState.PlayerOne = player;
        }
        else
        {
            gameState.PlayerTwo = player;
        }

        return gameState;
    }
}
public static class CardInfos
{
    public static CardInfo GetCardInfo(CardId id) => id switch
    {
        CardId.Knight => new UnitCard(id, 3, UnitType.Knight),
        CardId.FireBall => new SpellCard(id, 4, SpellType.FireBall),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };
}

