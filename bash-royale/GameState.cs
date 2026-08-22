using bash_royale.Networking;

namespace bash_royale;
using System;
public struct GameState
{
    public PlayerState PlayerOne;
    public PlayerState PlayerTwo;
    public bool IsDraw;
    public bool IsGameOver;
    public PlayerId? Winner;
    public int Tick;
    public bool IsOvertime => Tick > GameSettings.REGULATION_END_TICK;
    public static GameState CreateNew()
    {
        return new GameState
        {
            PlayerOne = PlayerState.CreateNew(PlayerId.One),
            PlayerTwo = PlayerState.CreateNew(PlayerId.Two),
            IsDraw = false,
            IsGameOver = false,
            Winner = null,
        };
    }

    public static PlayerState GetPlayerState(GameState state, PlayerId playerId)
    {
        return playerId == PlayerId.One ?  state.PlayerOne : state.PlayerTwo;
    }
}

public record PlayerResult(PlayerState playerState, List<ActionResult> results);
public static class GameSim
{   
    private static bool HasCastle(List<UnitState> units)
    {
        foreach (UnitState unit in units)
        {
            if (unit.Type == UnitType.Castle) return true;
        }

        return false;
    }

    private static int BuildingCount(List<UnitState> units)
    {
        int count = 0;

        foreach (UnitState unit in units)
        {
            if (!UnitInfos.GetUnitInfo(unit.Type).IsBuilding) continue;
            count++;
        }

        return count;
    }
    
    private static int LowestBuildingHealth(List<UnitState> units)
    {
        int lowest = int.MaxValue;

        foreach (UnitState unit in units)
        {
            if (!UnitInfos.GetUnitInfo(unit.Type).IsBuilding) continue;
            if (unit.Health >= lowest) continue;

            lowest = unit.Health;
        }

        return lowest;
    }
    
        private static GameState CheckGameOver(GameState state)
        {
            bool p1Alive = HasCastle(state.PlayerOne.Units);
            bool p2Alive = HasCastle(state.PlayerTwo.Units);
    
            if (!p1Alive || !p2Alive)
            {
                state.IsGameOver = true;
                state.IsDraw = !p1Alive && !p2Alive;
                state.Winner = state.IsDraw ? null : (p1Alive ? PlayerId.One : PlayerId.Two);
                return state;
            }
    
            if (state.Tick < GameSettings.OVERTIME_END_TICK) return state;
    
            // Time's up. More buildings standing wins; otherwise the weakest building loses.
            int p1Count = BuildingCount(state.PlayerOne.Units);
            int p2Count = BuildingCount(state.PlayerTwo.Units);
    
            state.IsGameOver = true;
    
            if (p1Count != p2Count)
            {
                state.Winner = p1Count > p2Count ? PlayerId.One : PlayerId.Two;
                return state;
            }
    
            int p1Lowest = LowestBuildingHealth(state.PlayerOne.Units);
            int p2Lowest = LowestBuildingHealth(state.PlayerTwo.Units);
    
            state.IsDraw = p1Lowest == p2Lowest;
            state.Winner = state.IsDraw ? null : (p1Lowest > p2Lowest ? PlayerId.One : PlayerId.Two);
            return state;
        }
    private static PlayerResult UpdatePlayer(PlayerState playerState, GameState gameState)
    {
        List<UnitState> units = playerState.Units;
        List<ActionResult> results = new(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            ActionResult result = UnitSim.Update(units[i], gameState);
            
            units[i] = result.unit;
            results.Add(result);
        }
        playerState.Units = units;
        return new PlayerResult(playerState, results);
    }
    public static GameState Update(GameState state, NetworkAction p1Action, NetworkAction p2Action)
    {   
        if (state.IsGameOver) return state;
        if (p1Action.Action == ActionType.DeployCard)
        {
            state = CardSim.PlayFromHand(state, PlayerId.One, p1Action.CardIdx, new Vector2Int(p1Action.X, p1Action.Y));
        }
        if (p2Action.Action == ActionType.DeployCard)
        {
            state = CardSim.PlayFromHand(state, PlayerId.Two, p2Action.CardIdx, new Vector2Int(p2Action.X, p2Action.Y));
        }
        if (state.Tick % GameSettings.ELIXIR_TICK_INTERVAL == 0)
        {
            state.PlayerOne.Elixir = Math.Min(state.PlayerOne.Elixir + 1, GameSettings.MAX_ELIXIR);
            state.PlayerTwo.Elixir = Math.Min(state.PlayerTwo.Elixir + 1, GameSettings.MAX_ELIXIR);
        }
        var p1Result = UpdatePlayer(state.PlayerOne, state);
        var p2Result = UpdatePlayer(state.PlayerTwo, state);

        state.PlayerOne = p1Result.playerState;
        state.PlayerTwo = p2Result.playerState;
        ApplyDamage(p1Result.results, state.PlayerTwo.Units);
        ApplyDamage(p2Result.results, state.PlayerOne.Units);
        
        state.PlayerOne.Units.RemoveAll(u => u.Health <= 0);
        state.PlayerTwo.Units.RemoveAll(u => u.Health <= 0);
        state = CheckGameOver(state);
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
            target.LastDamageTick = target.Ticks;
            //Console.WriteLine(target.Type + " " + target.Health);
            enemies[result.targetIdx] = target;
        }
    } 
}