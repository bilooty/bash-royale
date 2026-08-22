using bash_royale.Networking;

namespace bash_royale;
using System;
public struct GameState
{
    public PlayerState PlayerOne;
    public PlayerState PlayerTwo;
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
    private static PlayerResult UpdatePlayer(PlayerState playerState, GameState gameState)
    {
        List<UnitState> units = playerState.Units;
        List<ActionResult> results = new(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            ActionResult result = UnitSim.Update(units[i], gameState);
            //Console.WriteLine(result.unit.Position.X + " " + result.unit.Position.Y);
            units[i] = result.unit;
            results.Add(result);
        }
        playerState.Units = units;
        return new PlayerResult(playerState, results);
    }
    public static GameState Update(GameState state, NetworkAction p1Action, NetworkAction p2Action)
    {
        if (p1Action.Action == ActionType.DeployCard)
        {
            state = CardSim.PlayFromHand(state, PlayerId.One, p1Action.CardIdx, new Vector2Int(p1Action.X, p1Action.Y));
        }
        if (p2Action.Action == ActionType.DeployCard)
        {
            state = CardSim.PlayFromHand(state, PlayerId.Two, p2Action.CardIdx, new Vector2Int(p2Action.X, p2Action.Y));
        }
        if (state.Tick % 20 == 0)
        {
            state.PlayerOne.Elixir += 1;
            state.PlayerTwo.Elixir += 1;
        }
        var p1Result = UpdatePlayer(state.PlayerOne, state);
        state.PlayerOne = p1Result.playerState;
        var p2Result = UpdatePlayer(state.PlayerTwo, state);
        state.PlayerTwo = p2Result.playerState;


        ApplyDamage(p1Result.results, state.PlayerTwo.Units);
        ApplyDamage(p2Result.results, state.PlayerOne.Units);
        
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
            //Console.WriteLine(target.Type + " " + target.Health);
            enemies[result.targetIdx] = target;
        }
    } 
}