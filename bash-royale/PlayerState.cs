using System.Numerics;

namespace bash_royale;


public struct PlayerState
{
    public PlayerId Id;
    public List<UnitState> Units;
    public int Elixir;
    public List<CardId> Hand;
    public int NextUnitId; 
    public List<CardId> Deck;
    
    


    private const int HAND_SIZE = 4;

    public static PlayerState CreateNew(PlayerId id) => CreateNew(id, Decks.CreateDefault());

    public static PlayerState CreateNew(PlayerId id, List<CardId> chosenDeck)
    {
        // Same seed on both machines, so the lockstep sim stays in sync.
        List<CardId> deck = Decks.Shuffled(chosenDeck, id);

        List<CardId> hand = deck.Take(HAND_SIZE).ToList();
        deck.RemoveRange(0, hand.Count);

        return new PlayerState
        {
            Id = id,
            Units = new List<UnitState>(),
            Elixir = GameSettings.STARTING_ELIXIR,
            Hand = hand,
            Deck = deck,
            NextUnitId = 0,
        };
    }

}
