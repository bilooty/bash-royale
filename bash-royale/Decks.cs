using System.IO;

namespace bash_royale;

/// <summary>
/// The eight cards a player brings into battle, plus the on-disk storage of the
/// local player's choice so it survives between runs.
/// </summary>
public static class Decks
{
    public const int DECK_SIZE = 8;

    // Deterministic shuffle seed. Both machines simulate both players, so the draw
    // order has to come out identical on each of them - never use an unseeded Random here.
    private const int SHUFFLE_SEED = 20250822;

    private static readonly string SavePath =
        Path.Combine(AppContext.BaseDirectory, "deck.txt");

    private static List<CardId>? _current;

    /// <summary>The local player's deck. Loaded from disk on first use.</summary>
    public static List<CardId> Current
    {
        get
        {
            _current ??= Load();
            return _current;
        }
    }

    public static List<CardId> CreateDefault() => new()
    {
        CardId.Knight,
        CardId.Archer,
        CardId.Goblin,
        CardId.Giant,
        CardId.Wizard,
        CardId.Hog,
        CardId.FireBall,
        CardId.Musketeer,
    };

    /// <summary>Replaces the local deck and writes it to disk. Invalid decks are rejected.</summary>
    public static bool Save(List<CardId> deck)
    {
        if (!IsValid(deck)) return false;

        _current = new List<CardId>(deck);
        try
        {
            File.WriteAllLines(SavePath, _current.Select(c => c.ToString()));
        }
        catch (IOException e)
        {
            System.Console.WriteLine("Could not save deck: " + e.Message);
        }
        return true;
    }

    /// <summary>A deck is legal when it holds exactly DECK_SIZE different, known cards.</summary>
    public static bool IsValid(List<CardId>? deck) =>
        deck is { Count: DECK_SIZE }
        && deck.Distinct().Count() == DECK_SIZE
        && deck.All(c => Enum.IsDefined(c));

    private static List<CardId> Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                List<CardId> loaded = new();
                foreach (string line in File.ReadAllLines(SavePath))
                {
                    if (Enum.TryParse(line.Trim(), out CardId card))
                        loaded.Add(card);
                }

                if (IsValid(loaded)) return loaded;
            }
        }
        catch (IOException e)
        {
            System.Console.WriteLine("Could not load deck: " + e.Message);
        }

        return CreateDefault();
    }

    /// <summary>Converts a deck to the wire format (one byte per card).</summary>
    public static byte[] ToBytes(List<CardId> deck) => deck.Select(c => (byte)c).ToArray();

    /// <summary>Reads a deck off the wire, falling back to the default if it is malformed.</summary>
    public static List<CardId> FromBytes(byte[]? bytes)
    {
        if (bytes is null) return CreateDefault();

        List<CardId> deck = bytes.Select(b => (CardId)b).ToList();
        return IsValid(deck) ? deck : CreateDefault();
    }

    /// <summary>
    /// Shuffles a copy of the deck with a fixed seed per player, so host and client
    /// draw the same cards in the same order.
    /// </summary>
    public static List<CardId> Shuffled(List<CardId> deck, PlayerId playerId)
    {
        List<CardId> cards = new(deck);
        Random random = new(SHUFFLE_SEED + (int)playerId);
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
        return cards;
    }
}
