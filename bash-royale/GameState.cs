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

public record PlayerResult(PlayerState playerState, List<ActionResult> results);
public static class GameSim
{
    private static PlayerResult UpdatePlayer(PlayerState playerState, GameState gameState)
    {
        List<UnitState> units = playerState.Units;
        List<ActionResult> results = new List<ActionResult>();
        for (int i = 0; i < units.Count; i++)
        {
            ActionResult result = UnitSim.Update(units[i], gameState);
            units[i] = result.unit;
            results.Add(result);
        }
        return new PlayerResult(playerState, results);
    }
    public static GameState Update(GameState state, float deltaSeconds)
    {

        List<ActionResult> results;
        var P1Result = UpdatePlayer(state.PlayerOne, state);
        state.PlayerOne = P1Result.playerState;
        foreach (ActionResult result in P1Result.results)
        {
            if (result.didDamage == false) continue;
            UnitState target = state.PlayerTwo.Units[result.targetIdx];
            target.Health = result.targetNewHP;
            PlayerState p2 = state.PlayerTwo;
            p2.Units[result.targetIdx] = target;
            state.PlayerTwo = p2;
        }
        
        var P2Result = UpdatePlayer( state.PlayerTwo, state);
        state.PlayerTwo = P2Result.playerState;
        foreach (ActionResult result in P2Result.results)
        {
            if (result.didDamage == false) continue;
            UnitState target = state.PlayerOne.Units[result.targetIdx];
            target.Health = result.targetNewHP;
            PlayerState p1 = state.PlayerOne;
            p1.Units[result.targetIdx] = target;
            state.PlayerTwo = p1;
        }
        
        return state;
    }
}