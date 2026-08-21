namespace bash_royale;

public struct PlayerState
{
    public PlayerId Id;
    public List<UnitState> Units;
    public float Elixir;
    public List<UnitType> Hand;
    public int KingTowerHealth;
    public int LeftTowerHealth;
    public int RightTowerHealth;

    public static PlayerState CreateNew(PlayerId id, List<UnitType> startingHand)
    {
        return new PlayerState
        {
            Id = id,
            Units = new List<UnitState>(),
            Elixir = GameSettings.STARTING_ELIXIR,
            Hand = startingHand,
            KingTowerHealth = GameSettings.KING_TOWER_HEALTH,
            LeftTowerHealth = GameSettings.PRINCESS_TOWER_HEALTH,
            RightTowerHealth = GameSettings.PRINCESS_TOWER_HEALTH,
        };
    }
}

public static class PlayerSim
{
    public static float RegenerateElixir(float elixir, float deltaSeconds)
    {
        float regenerated = elixir + GameSettings.ELIXIR_REGEN_PER_SECOND * deltaSeconds;
        return Math.Min(regenerated, GameSettings.MAX_ELIXIR);
    }
}