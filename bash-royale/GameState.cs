namespace bash_royale;

public struct GameState
{
    public PlayerState PlayerOne;
    public PlayerState PlayerTwo;
    public Occupancy Occupancy;
    public bool IsGameOver;
    public PlayerId? Winner;
    public int Tick;
    public static GameState CreateNew()
    {
        return new GameState
        {
            PlayerOne = PlayerState.CreateNew(PlayerId.One),
            PlayerTwo = PlayerState.CreateNew(PlayerId.Two),
            
            IsGameOver = false,
            Winner = null,
        };
    }
}

public record PlayerResult(PlayerState playerState, List<ActionResult> results);
public static class GameSim
{
    private static List<ActionResult> UpdatePlayer(PlayerState playerState, GameState gameState)
    {
        List<UnitState> units = playerState.Units;
        List<ActionResult> results = new(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            ActionResult result = UnitSim.Update(units[i], gameState);
            units[i] = result.unit;
            results.Add(result);
        }
        return results;
    }
    public static GameState Update(GameState state)
    {
        state.Occupancy = Occupancy.Build(state);
        
        var p1Result = UpdatePlayer(state.PlayerOne, state);
        var p2Result = UpdatePlayer(state.PlayerTwo, state);

        ApplyDamage(p1Result, state.PlayerTwo.Units);
        ApplyDamage(p2Result, state.PlayerOne.Units);

        state.PlayerOne.Units.RemoveAll(u => u.Health <= 0);
        state.PlayerTwo.Units.RemoveAll(u => u.Health <= 0);

        state.Tick++;
        return state;
    }
        
    private static void ApplyDamage(List<ActionResult> results, List<UnitState> enemies)
    {
        foreach (ActionResult result in results)
        {
            if (!result.didDamage) continue;
            UnitState target = enemies[result.targetIdx];
            target.Health -= result.damage;
            enemies[result.targetIdx] = target;
        }
    } 
}