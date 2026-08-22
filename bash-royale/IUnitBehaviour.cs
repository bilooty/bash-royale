namespace bash_royale;
using System;

public static class Movement
{
    public const int MOVE_THRESHOLD = 100;   // 100 progress = one cell

    public static UnitState StepTo(UnitState unit, Vector2Int destination, int speed, GameState state, MovementLayer layer)
    {
        // Clamp so a blocked unit doesn't bank progress and teleport when freed.
        unit.MoveProgress = Math.Min(unit.MoveProgress + speed, MOVE_THRESHOLD);
        if (unit.MoveProgress < MOVE_THRESHOLD) return unit;

        Vector2Int? next = Pathfinder.NextStep(unit.Position, destination, layer);
        if (next is null) return unit;
        if (UnitSim.IsOccupied(state, next.Value, layer)) return unit;

        unit.MoveProgress -= MOVE_THRESHOLD;
        unit.Position = next.Value;
        return unit;
    }
    
}
public interface IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx);
}
// FOR ALL NON ATTACKS ENSURE ACTIONRESULT.ISATTACK IS FALSE
public class WalkForwards(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        Vector2Int? destination = NearestBuilding(unit, state);
        if (destination is null) return ActionResult.NoAttack(unit);

        MovementLayer layer = UnitInfos.GetUnitInfo(unit.Type).Layer;
        return ActionResult.NoAttack(Movement.StepTo(unit, destination.Value, speed, state, layer));
    }

    private static Vector2Int? NearestBuilding(UnitState unit, GameState state)
    {
        Vector2Int? destination = null;
        long best = long.MaxValue;

        foreach (UnitState enemy in UnitSim.GetEnemyUnits(unit, state))
        {
            if (!UnitInfos.GetUnitInfo(enemy.Type).IsBuilding) continue;

            long d = UnitSim.DistanceSquared(unit.Position, enemy.Position);
            if (d >= best) continue;

            best = d;
            destination = enemy.Position;
        }

        return destination;
    }
}

public class ChaseBehaviour(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        if (target is null) return ActionResult.NoAttack(unit);

        MovementLayer layer = UnitInfos.GetUnitInfo(unit.Type).Layer;
        Vector2Int approach = Pathfinder.NearestFreeApproach(unit.Position, target.Value.Position, layer, state);

        return ActionResult.NoAttack(Movement.StepTo(unit, approach, speed, state, layer));
    }
}

public class AttackBehaviour(int damage) : IUnitBehaviour
{
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        System.Console.WriteLine("bleehhh");
        if (target is null) return ActionResult.NoAttack(unit);
        
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        System.Console.WriteLine("bleehhh");
        if (unit.Ticks % info.TicksPerAttack != 0) return ActionResult.NoAttack(unit);
        System.Console.Write("Last attack tick was: " + unit.LastAttackTick + "is now");
        unit.LastAttackTick = unit.Ticks;
        System.Console.WriteLine(unit.LastAttackTick);
        return new ActionResult(unit, targetIdx, damage, true);
    }
}

public class DoNothing : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        return ActionResult.NoAttack(unit);
    }
}

