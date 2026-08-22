namespace bash_royale;

public enum CardId
{
    Zap,
    Knight,
    Giant,
    Archer,
    Goblin,
    Wizard,
    Hog,

    FireBall,
    Barbarian,
    Musketeer,
    MiniPekka,
    Valkyrie,
    Skeleton,
    Dragon,
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

public record UnitCard(CardId Id, int Cost, UnitType UnitType)
    : CardInfo(Id, CardType.Unit, Cost, ValidLocation.YourSide);

public record SpellCard(CardId Id, int Cost, ProjectileType ProjectileType, Vector2Int Offset, Vector2Int Size) : CardInfo(Id, CardType.Spell, Cost, ValidLocation.BothSides);

public record CardInfo(
    CardId Id,
    CardType Type,
    int Cost,
    ValidLocation ValidLocation);

public static class CardSim
{
    public static GameState PlayCard(CardId cardId, GameState gameState, PlayerId playerId, Vector2Int position)
    {
        var card = CardInfos.GetCardInfo(cardId);
        PlayerState player = playerId == PlayerId.One ? gameState.PlayerOne : gameState.PlayerTwo;
  

       
        if (card is UnitCard unitCard)
        {
            player.Units.Add(new UnitState(
                unitCard.UnitType, playerId, position));

        }

        if (card is SpellCard spellCard)
        {
            gameState.Projectiles.Add(new ProjectileState(spellCard.ProjectileType, playerId, position));
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

    public static GameState PlayFromHand(GameState gameState, PlayerId playerId, int handIndex, Vector2Int position)
    {
        
        PlayerState player = playerId == PlayerId.One ? gameState.PlayerOne : gameState.PlayerTwo;

        if (handIndex < 0 || handIndex >= player.Hand.Count)
        {
            return gameState;
        }

        CardId cardId = player.Hand[handIndex];
        CardInfo card = CardInfos.GetCardInfo(cardId);
        if (player.Elixir - card.Cost < 0)
        {
            return gameState;
        }
        player.Elixir -= card.Cost;
        System.Console.WriteLine("Card cost was +" + card.Cost);
      
        gameState = PlayCard(cardId, gameState, playerId, position);

       
        player.Hand.RemoveAt(handIndex);
        player.Deck.Add(cardId);
        if (player.Deck.Count > 0)
        {
            CardId nextCard = player.Deck[0];
            player.Deck.RemoveAt(0);
            player.Hand.Insert(handIndex, nextCard);
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
        CardId.Zap => new SpellCard(id, 4, ProjectileType.Zap, new Vector2Int(-1, -1), new Vector2Int(3, 3)),
        CardId.Giant => new UnitCard(id, 5, UnitType.Giant),
        CardId.Archer => new UnitCard(id, 3, UnitType.Archer),
        CardId.Goblin => new UnitCard(id, 2, UnitType.Goblin),
        CardId.Wizard => new UnitCard(id, 6, UnitType.Wizard),
        CardId.Hog => new UnitCard(id, 8, UnitType.HogRider),
        CardId.Barbarian => new UnitCard(id, 4, UnitType.Barbarian),
        CardId.Musketeer => new UnitCard(id, 4, UnitType.Musketeer),
        CardId.MiniPekka => new UnitCard(id, 4, UnitType.MiniPekka),
        CardId.Valkyrie => new UnitCard(id, 4, UnitType.Valkyrie),
        CardId.Skeleton => new UnitCard(id, 1, UnitType.Skeleton),
        CardId.Dragon => new UnitCard(id, 5, UnitType.Dragon),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };

    // Every card that can be put in a deck, in a fixed order so the deck screen and
    // the network protocol (which sends card ids as bytes) always agree.
    public static readonly IReadOnlyList<CardId> AllCards = Enum.GetValues<CardId>().ToList();

    // Five character label used by the tiny card boxes in the battle HUD.
    public static string GetShortLabel(CardId id) => id switch
    {
        CardId.Knight    => "KNGHT",
        CardId.Giant     => "GIANT",
        CardId.Archer    => "ARCHR",
        CardId.Goblin    => "GOBLN",
        CardId.Wizard    => "WIZRD",
        CardId.Hog       => "RIDER",
        CardId.FireBall  => "FRBAL",
        CardId.Barbarian => "BARBR",
        CardId.Musketeer => "MUSKT",
        CardId.MiniPekka => "PEKKA",
        CardId.Valkyrie  => "VALKR",
        CardId.Skeleton  => "SKELE",
        CardId.Dragon    => "DRAGN",
        _ => id.ToString().PadRight(5)[..5],
    };

    // Full name for the roomier deck builder boxes.
    public static string GetName(CardId id) => id switch
    {
        CardId.Hog      => "Hog Rider",
        CardId.FireBall => "Fireball",
        CardId.MiniPekka => "Mini Pekka",
        CardId.Dragon   => "Baby Dragon",
        _ => id.ToString(),
    };

    // The glyph the card is drawn with, so the deck builder looks like the arena.
    public static char GetGlyph(CardId id) =>
        GetCardInfo(id) is UnitCard unitCard && Scenes.EntityDisplay.Displays.TryGetValue(unitCard.UnitType, out var display)
            ? display.Glyphs[0][0].GlyphCharacter
            : '*';
}
