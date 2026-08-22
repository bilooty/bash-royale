namespace bash_royale;
using System;

public static class Movement
{
    public const int MOVE_THRESHOLD = 100; // 100 progress = one cell

    public static UnitState StepTo(UnitState unit, Vector2Int destination, Vector2Int destinationSize, int speed,
        GameState state, MovementLayer layer)
    {
        unit.MoveProgress = Math.Min(unit.MoveProgress + speed, MOVE_THRESHOLD);
        if (unit.MoveProgress < MOVE_THRESHOLD) return unit;

        Vector2Int size = UnitInfos.GetUnitInfo(unit.Type).Size;

        Vector2Int? next = Pathfinder.NextStep(unit.Position, destination, size, destinationSize, unit.Type, state);
        if (next is null) return unit;
        if (UnitSim.FootprintBlocked(state, next.Value, size, unit.Type, unit.Position)) return unit;

        unit.MoveProgress -= MOVE_THRESHOLD;
        unit.Position = next.Value;
        return unit;
    }
}

public interface IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId);
}

public class WalkForwards(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    {
        UnitState? destination = NearestBuilding(unit, state);
        if (destination is null) return ActionResult.NoAttack(unit);

        MovementLayer layer = UnitInfos.GetUnitInfo(unit.Type).Layer;
        Vector2Int destinationSize = UnitInfos.GetUnitInfo(destination.Value.Type).Size;

        return ActionResult.NoAttack(Movement.StepTo(unit, destination.Value.Position, destinationSize, speed, state, layer));
    }

    private static UnitState? NearestBuilding(UnitState unit, GameState state)
        {
            List<UnitState> enemies = UnitSim.GetEnemyUnits(unit, state);
    
            // Same rule as targeting: the castle is off-limits until a tower falls.
            bool castleLocked = UnitSim.CountTowers(enemies) >= 2;
    
            UnitState? destination = null;
            long best = long.MaxValue;
    
            foreach (UnitState enemy in enemies)
            {
                if (!UnitInfos.GetUnitInfo(enemy.Type).IsBuilding) continue;
                if (enemy.Type == UnitType.Castle && castleLocked) continue;
    
                long d = UnitSim.FootprintDistance(unit, enemy);
                if (d >= best) continue;
    
                best = d;
                destination = enemy;
            }
    
            return destination;
        }
}

public class ChaseBehaviour(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    {
        if (target is null) return ActionResult.NoAttack(unit);

        MovementLayer layer = UnitInfos.GetUnitInfo(unit.Type).Layer;
        Vector2Int targetSize = UnitInfos.GetUnitInfo(target.Value.Type).Size;

        return ActionResult.NoAttack(Movement.StepTo(unit, target.Value.Position, targetSize, speed, state, layer));
    }
}

public class AttackBehaviour(int damage) : IUnitBehaviour
{
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    { 
        if (target is null) return ActionResult.NoAttack(unit);
        
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type); 
        if (unit.Ticks % info.TicksPerAttack != 0) return ActionResult.NoAttack(unit);
        unit.LastAttackTick = unit.Ticks;
        return new ActionResult(unit, targetId, damage, true);
    }
}
public class RangedAttack(int damage, ProjectileType projectileType) : IUnitBehaviour
{
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    { 
        if (target is null) return ActionResult.NoAttack(unit);
        
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type); 
        if (unit.Ticks % info.TicksPerAttack != 0) return ActionResult.NoAttack(unit);
        unit.LastAttackTick = unit.Ticks;
        PlayerState enemy = unit.Owner == PlayerId.One ? state.PlayerTwo : state.PlayerOne;
        ProjectileState newProj = new ProjectileState(projectileType, unit.Owner, unit.Position);
        newProj.TargetId = targetId;
        return new ActionResult(unit, targetId, 0, false, newProj);
    }
}

public class DoNothing : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    {
        return ActionResult.NoAttack(unit);
    }
}

