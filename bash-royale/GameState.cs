namespace bash_royale;

public struct GameState
{
    public PlayerState PlayerOne;
    public PlayerState PlayerTwo;
    public float ElapsedSeconds;
    public bool IsGameOver;
    public PlayerId? Winner;

    public static GameState CreateNew(List<UnitType> playerOneHand, List<UnitType> playerTwoHand)
    {
        return new GameState
        {
            PlayerOne = PlayerState.CreateNew(PlayerId.One, playerOneHand),
            PlayerTwo = PlayerState.CreateNew(PlayerId.Two, playerTwoHand),
            ElapsedSeconds = 0f,
            IsGameOver = false,
            Winner = null,
        };
    }
}

public static class GameSim
{
    public static GameState Update(GameState state, float deltaSeconds)
    {
        if (state.IsGameOver)
            return state;

        state.PlayerOne.Elixir = PlayerSim.RegenerateElixir(state.PlayerOne.Elixir, deltaSeconds);
        state.PlayerTwo.Elixir = PlayerSim.RegenerateElixir(state.PlayerTwo.Elixir, deltaSeconds);

        

        state.PlayerOne.Units.RemoveAll(unit => unit.Health <= 0);
        state.PlayerTwo.Units.RemoveAll(unit => unit.Health <= 0);

        state.ElapsedSeconds += deltaSeconds;
        UpdateWinCondition(ref state);

        return state;
    }

    private static void UpdateWinCondition(ref GameState state)
    {
        bool playerOneDefeated = state.PlayerOne.KingTowerHealth <= 0;
        bool playerTwoDefeated = state.PlayerTwo.KingTowerHealth <= 0;

        if (!playerOneDefeated && !playerTwoDefeated)
            return;

        state.IsGameOver = true;
        state.Winner = playerOneDefeated == playerTwoDefeated
            ? null
            : playerOneDefeated ? PlayerId.Two : PlayerId.One;
    }
}