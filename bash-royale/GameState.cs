namespace bash_royale;

public struct GameState
{
    public PlayerState PlayerOne;
    public PlayerState PlayerTwo;
    
}

public static class GameSim
{
    public static GameState Update(GameState state)
    {
        state.PlayerOne = PlayerSim.Update(state.PlayerOne);
        state.PlayerTwo = PlayerSim.Update(state.PlayerTwo);
        return state;
    }
}