namespace bash_royale;
using System;
public static class Movement
{
    public const int MOVE_THRESHOLD = 50;   // 100 progress = one cell

    public static UnitState StepToward(UnitState unit, Vector2Int destination, int speed)
    {
        //Console.WriteLine("OldPos: " + unit.Position + " " + unit.MoveProgress);

        unit.MoveProgress += speed;
        if (unit.MoveProgress < MOVE_THRESHOLD) return unit;

        unit.MoveProgress -= MOVE_THRESHOLD;

        int dx = Math.Sign(destination.X - unit.Position.X);
        int dy = Math.Sign(destination.Y - unit.Position.Y);

        unit.Position = new Vector2Int(unit.Position.X + dx, unit.Position.Y + dy);
        //Console.WriteLine("NewPos: " + unit.Position);
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
        //Console.WriteLine("We are moving forward!");
        List<UnitState> enemies = UnitSim.GetEnemyUnits(unit, state);

        Vector2Int? destination = null;
        long best = long.MaxValue;

        foreach (UnitState enemy in enemies)
        {
            if (!UnitInfos.GetUnitInfo(enemy.Type).IsBuilding) continue;

            long d = UnitSim.DistanceSquared(unit.Position, enemy.Position);
            if (d >= best) continue;

            best = d;
            destination = enemy.Position;
        }

        if (destination is null) return ActionResult.NoAttack(unit);
        return ActionResult.NoAttack(Movement.StepToward(unit, destination.Value, speed));
    }
}
public class ChaseBehaviour(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        if (target is null) return ActionResult.NoAttack(unit);
        return ActionResult.NoAttack(Movement.StepToward(unit, target.Value.Position, speed));
    }
}

public class AttackBehaviour(int damage) : IUnitBehaviour
{
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        
        if (target is null) return ActionResult.NoAttack(unit);
        
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        if (unit.Ticks % info.TicksPerAttack != 0) return ActionResult.NoAttack(unit);
        
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

