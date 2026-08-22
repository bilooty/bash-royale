namespace bash_royale;
using System;

public static class Movement
{
    public const int MOVE_THRESHOLD = 100; // 100 progress = one cell

    public static UnitState StepTo(UnitState unit, Vector2Int destination, Vector2Int destinationSize,
        int speed, int stopDistance, GameState state)
    {
       
        unit.MoveProgress += speed;
        if (unit.MoveProgress < MOVE_THRESHOLD) return unit;

        Vector2Int size = UnitInfos.GetUnitInfo(unit.Type).Size;

        Vector2Int? next = Pathfinder.NextStep(
            from: unit.Position,
            to: destination,
            size: size,
            goalSize: destinationSize,
            unitType: unit.Type,
            ignoreUnitId: unit.Id,
            stopDistance: stopDistance,
            state: state);

        if (next is null) return Stall(unit);

        if (UnitSim.FootprintBlocked(state, next.Value, size, unit.Type, unit.Id)) return Stall(unit);

        unit.MoveProgress -= MOVE_THRESHOLD;
        unit.Position = next.Value;
        return unit;
    }

    // Blocked or no route: hold progress at a full cell so the unit steps the instant the
    // way clears, but don't let it bank several cells' worth while it waits.
    private static UnitState Stall(UnitState unit)
    {
        unit.MoveProgress = MOVE_THRESHOLD;
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

        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        Vector2Int destinationSize = UnitInfos.GetUnitInfo(destination.Value.Type).Size;

        return ActionResult.NoAttack(Movement.StepTo(
            unit, destination.Value.Position, destinationSize, speed, info.AttackRange, state));
    }

    private static UnitState? NearestBuilding(UnitState unit, GameState state)
    {
        List<UnitState> enemies = UnitSim.GetEnemyUnits(unit, state);

        // Same rule as targeting: the castle is off-limits until a tower falls.
        bool castleLocked = UnitSim.CountTowers(enemies) >= 2;

        UnitState? destination = null;
        int best = int.MaxValue;

        foreach (UnitState enemy in enemies)
        {
            if (enemy.Health <= 0) continue;
            if (!UnitInfos.GetUnitInfo(enemy.Type).IsBuilding) continue;
            if (enemy.Type == UnitType.Castle && castleLocked) continue;

            int d = UnitSim.FootprintDistance(unit, enemy);
            if (d > best) continue;

            // Same deterministic tie-break as targeting: lowest Id, never list order.
            if (d == best && destination is not null && enemy.Id >= destination.Value.Id) continue;

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

        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        Vector2Int targetSize = UnitInfos.GetUnitInfo(target.Value.Type).Size;

        return ActionResult.NoAttack(Movement.StepTo(
            unit, target.Value.Position, targetSize, speed, info.AttackRange, state));
    }
}

internal static class AttackTiming
{
    public static bool Ready(UnitState unit, UnitInfo info)
    {
        return unit.Ticks - unit.LastAttackTick >= Math.Max(1, info.TicksPerAttack);
    }
}

public class AttackBehaviour(int damage) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    {
        if (target is null) return ActionResult.NoAttack(unit);

        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        if (!AttackTiming.Ready(unit, info)) return ActionResult.NoAttack(unit);

        unit.LastAttackTick = unit.Ticks;
        return new ActionResult(unit, targetId, damage, true);
    }
}

public class RangedAttack(ProjectileType projectileType) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetId)
    {
        if (target is null) return ActionResult.NoAttack(unit);

        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        if (!AttackTiming.Ready(unit, info)) return ActionResult.NoAttack(unit);

        unit.LastAttackTick = unit.Ticks;

        ProjectileState newProj = new ProjectileState(projectileType, unit.Owner, unit.Position)
        {
            TargetId = targetId
        };

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