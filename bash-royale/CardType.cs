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
    Cannon,
    Pekka,
    ThreeM,
    Skarmy,
    EBarbs,
    Berserker,
    Balloon,
    Princess,
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
    Swarm,
}

public record UnitCard(CardId Id, int Cost, UnitType UnitType, string DeploySound = null)
    : CardInfo(Id, CardType.Unit, Cost, ValidLocation.YourSide, DeploySound);
public record SpellCard(CardId Id, int Cost, ProjectileType ProjectileType, Vector2Int Offset, Vector2Int Size, string DeploySound = null) 
    : CardInfo(Id, CardType.Spell, Cost, ValidLocation.BothSides, DeploySound);
public record SwarmCard(CardId Id, int Cost, UnitType UnitType, List<Vector2Int> Offsets, string DeploySound = null) 
    :  CardInfo(Id, CardType.Swarm, Cost, ValidLocation.YourSide, DeploySound);
public record CardInfo(
    CardId Id,
    CardType Type,
    int Cost,
    ValidLocation ValidLocation,
    string DeploySound = null);


public static class CardSim
{
    // How far from the requested cell a spawn will look for space before giving up.
    private const int MaxPlacementSearch = 3;

    // A single unit is just a swarm with one member at no offset, so both card types
    // go through the same placement code and can't drift apart.
    private static readonly List<Vector2Int> SingleUnitOffsets = [new Vector2Int(0, 0)];

    public static GameState PlayCard(CardId cardId, GameState gameState, PlayerId playerId, Vector2Int position)
    {
        CardInfo card = CardInfos.GetCardInfo(cardId);

        switch (card)
        {
            case UnitCard unitCard:
                gameState = SpawnGroup(gameState, playerId, unitCard.UnitType, position, SingleUnitOffsets);
                break;

            case SwarmCard swarmCard:
                gameState = SpawnGroup(gameState, playerId, swarmCard.UnitType, position, swarmCard.Offsets);
                break;

            case SpellCard spellCard:
                gameState.Projectiles.Add(
                    new ProjectileState(spellCard.ProjectileType, playerId, position + spellCard.Offset));
                break;
        }

        return gameState;
    }

    // Offsets are walked in list order and ids are handed out in that same order, so two
    // machines given the same card and cell produce identical units in identical slots.
    private static GameState SpawnGroup(GameState gameState, PlayerId playerId, UnitType unitType,
        Vector2Int origin, List<Vector2Int> offsets)
    {
        List<UnitState> units = GameState.GetPlayerState(gameState, playerId).Units;

        foreach (Vector2Int offset in offsets)
        {
            Vector2Int desired = origin + offset;
            
            Vector2Int? cell = FindFreeCell(gameState, desired, unitType);
            if (cell is null) continue;

            units.Add(new UnitState(unitType, playerId, cell.Value, gameState.NextID));
            gameState.NextID++;
        }

        return gameState;
    }
    
    private static Vector2Int? FindFreeCell(GameState state, Vector2Int desired, UnitType unitType)
    {
        Vector2Int size = UnitInfos.GetUnitInfo(unitType).Size;

        if (!UnitSim.FootprintBlocked(state, desired, size, unitType, UnitState.NoTarget)) return desired;

        for (int radius = 1; radius <= MaxPlacementSearch; radius++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius) continue;

                    Vector2Int cell = new(desired.X + dx, desired.Y + dy);
                    if (!UnitSim.FootprintBlocked(state, cell, size, unitType, UnitState.NoTarget)) return cell;
                }
            }
        }

        return null;
    }

    public static GameState PlayFromHand(GameState gameState, PlayerId playerId, int handIndex, Vector2Int position)
    {
        PlayerState player = playerId == PlayerId.One ? gameState.PlayerOne : gameState.PlayerTwo;

        if (handIndex < 0 || handIndex >= player.Hand.Count) return gameState;

        CardId cardId = player.Hand[handIndex];
        CardInfo card = CardInfos.GetCardInfo(cardId);

        if (player.Elixir < card.Cost) return gameState;

        player.Elixir -= card.Cost;

        player.Hand.RemoveAt(handIndex);
        player.Deck.Add(cardId);
        if (player.Deck.Count > 0)
        {
            CardId nextCard = player.Deck[0];
            player.Deck.RemoveAt(0);
            player.Hand.Insert(handIndex, nextCard);
        }

        if (playerId == PlayerId.One) gameState.PlayerOne = player;
        else gameState.PlayerTwo = player;

        // Last, so nothing here overwrites the state PlayCard returns.
        return PlayCard(cardId, gameState, playerId, position);
    }
}

public static class SwarmFormations
{
    public static readonly List<Vector2Int> ThreeRing =
    [
        new Vector2Int(0, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
    ];
    
    public static readonly List<Vector2Int> Pair =
    [
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
    ];

    public static readonly List<Vector2Int> FourSquare =
    [
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
    ];
    
    public static readonly List<Vector2Int> EightBlock =
    [
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(2, -1),
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1),  new Vector2Int(2, 1),
    ];
}
public static class CardInfos
{
    public static CardInfo GetCardInfo(CardId id) => id switch
    {
        CardId.Knight => new UnitCard(id, 3, UnitType.Knight, DeploySound: "Knight_placed"),
        CardId.Zap => new SpellCard(id, 2, ProjectileType.Zap, new Vector2Int(-1, -1), new Vector2Int(3, 3), DeploySound: "zap_placed"),
        CardId.FireBall => new SpellCard(id, 4, ProjectileType.FireBallSummon, new Vector2Int(-2, -1), new Vector2Int(5, 3), DeploySound: "fireball_placed"),
        CardId.Giant => new UnitCard(id, 5, UnitType.Giant, DeploySound: "Giant_placed"),
        CardId.Archer => new SwarmCard(id, 2, UnitType.Archer, SwarmFormations.Pair, DeploySound: "archer_placed"),
        CardId.Goblin => new SwarmCard(id, 2, UnitType.Goblin, SwarmFormations.ThreeRing, DeploySound: "goblin_placed"),
        CardId.Wizard => new UnitCard(id, 5, UnitType.Wizard, DeploySound: "Wizard_placed"),
        CardId.Hog => new UnitCard(id, 4, UnitType.HogRider, DeploySound: "Hogrider_placed"),
        CardId.Barbarian => new SwarmCard(id, 5, UnitType.Barbarian, SwarmFormations.FourSquare, DeploySound:"EBard_placed"),
        CardId.Musketeer => new UnitCard(id, 4, UnitType.Musketeer, DeploySound: "Musketeer_placed"),
        CardId.MiniPekka => new UnitCard(id, 4, UnitType.MiniPekka, DeploySound: "Pekka_placed"),
        CardId.Pekka => new UnitCard(id, 7, UnitType.Pekka, DeploySound: "Pekka_placed"),
        CardId.Valkyrie => new UnitCard(id, 4, UnitType.Valkyrie, DeploySound: "Valkyrie_placed"),
        CardId.Skeleton => new SwarmCard(id, 1, UnitType.Skeleton, SwarmFormations.ThreeRing, DeploySound: "Skeleton_placed"),
        CardId.Dragon => new UnitCard(id, 5, UnitType.Dragon, DeploySound: "Dragon_placed"),
        CardId.Cannon => new UnitCard(id, 4, UnitType.Cannon, DeploySound: "Cannon_placed"),
        CardId.ThreeM => new SwarmCard(id, 9, UnitType.Musketeer, SwarmFormations.ThreeRing, DeploySound: "Musketeer_placed"),
        CardId.Skarmy => new SwarmCard(id, 3, UnitType.Skeleton, SwarmFormations.EightBlock, DeploySound: "Skarmy_placed"),
        CardId.EBarbs => new SwarmCard(id, 6,  UnitType.EBarbs, SwarmFormations.Pair, DeploySound: "EBarb_placed"),
        CardId.Berserker => new UnitCard(id, 2, UnitType.Berserker,"Berserker_placed"),
        CardId.Princess => new UnitCard(id, 2, UnitType.Princess, "Princess_placed"),
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
        CardId.Hog       => "HOG.R",
        CardId.FireBall  => "FRBLL",
        CardId.Barbarian => "BARB",
        CardId.Musketeer => "MUSKT",
        CardId.MiniPekka => "M.PKA",
        CardId.Pekka => "PEKKA",
        CardId.Valkyrie  => "VALK",
        CardId.Skeleton  => "SKLTN",
        CardId.Skarmy => "SKRMY",
        CardId.ThreeM => "3MUSK",
        CardId.Dragon    => "DRAGN",
        CardId.Cannon    => "CNNON", 
        CardId.EBarbs => "EBARB",
        CardId.Zap => "ZAP",
        CardId.Balloon => "BLOON",
        CardId.Berserker => "BSERK",
        CardId.Princess => "PRNCS",
        _ => id.ToString().PadRight(5)[..5].ToUpper(),
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
    public static ColoredGlyph? GetDisplayGlyph(CardId id) => GetCardInfo(id) switch
    {
        UnitCard c  => Rendering.EntityDisplay.Displays.GetValueOrDefault(c.UnitType)?.Glyphs[0][0],
        SwarmCard c => Rendering.EntityDisplay.Displays.GetValueOrDefault(c.UnitType)?.Glyphs[0][0],
        SpellCard c => Rendering.EntityDisplay.Projectiles.GetValueOrDefault(c.ProjectileType)?.Glyphs[0][0],
        _ => null,
    };

    public static char GetGlyph(CardId id) => GetDisplayGlyph(id)?.GlyphCharacter ?? '*';
}
