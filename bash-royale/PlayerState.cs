using System.Numerics;

namespace bash_royale;

public struct PlayerState
{
    public PlayerId Id;
    public List<UnitState> Units;
    public int Elixir;
    public List<CardId> Hand;
    public List<CardId> Deck;
    


    private const int HAND_SIZE = 4;

    public static PlayerState CreateNew(PlayerId id)
    {
        List<CardId> deck = Enum.GetValues<CardId>().ToList();
        //Shuffle(deck);

        List<CardId> hand = deck.Take(HAND_SIZE).ToList();
        deck.RemoveRange(0, hand.Count);

        return new PlayerState
        {
            Id = id,
            Units = new List<UnitState>(),
            Elixir = GameSettings.STARTING_ELIXIR,
            Hand = hand,
            Deck = deck,
        };
    }

    private static void Shuffle(List<CardId> cards)
    {
        Random random = new Random();
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }
}
