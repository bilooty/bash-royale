namespace bash_royale;



public record ActionResult(UnitState unit, int targetId, int damage, bool didDamage, ProjectileState? newProjectile = null)
{
    public static ActionResult NoAttack(UnitState unit) => new(unit, -1, 0, false);
}

public static class UnitSim
{
    private const int RetargetMargin = 1;

    public static ActionResult Update(UnitState curUnit, GameState gameState)
    {
        UnitInfo info = UnitInfos.GetUnitInfo(curUnit.Type);
        List<UnitState> enemies = GetEnemyUnits(curUnit, gameState);

        bool castleLocked = CountTowers(enemies) >= 2;

        UnitState? target = ResolveTarget(curUnit, info, enemies, castleLocked);

        IUnitBehaviour behaviour;

        if (target is null)
        {
            curUnit.TargetId = UnitState.NoTarget;
            curUnit.CurrentBehaviour = Behaviour.Neutral;
            behaviour = info.NeutralBehaviour;
        }
        else
        {
            curUnit.TargetId = target.Value.Id;

            bool inAttackRange = InRange(curUnit, target.Value, info.AttackRange);
            curUnit.CurrentBehaviour = inAttackRange ? Behaviour.Attack : Behaviour.Chase;
            behaviour = inAttackRange ? info.AttackBehaviour : info.ChaseBehaviour;
        }

        curUnit.Ticks++;
        return behaviour.Update(curUnit, gameState, target, target?.Id ?? -1);
    }
    
    private static UnitState? ResolveTarget(UnitState curUnit, UnitInfo curInfo,
        List<UnitState> enemies, bool castleLocked)
    {
        if (curUnit.TargetId != UnitState.NoTarget)
        {
            foreach (UnitState enemy in enemies)
            {
                if (enemy.Id != curUnit.TargetId) continue;
                
                if (!IsTargetable(curInfo, enemy, castleLocked)) break;
                if (FootprintDistance(curUnit, enemy) > curInfo.AggroRange + RetargetMargin) break;

                return enemy;
            }
        }

        return FindNearestEnemy(curUnit, curInfo, enemies, castleLocked);
    }
    
    private static UnitState? FindNearestEnemy(UnitState curUnit, UnitInfo curInfo,
        List<UnitState> enemies, bool castleLocked)
    {
        UnitState? closest = null;
        int closestDistance = int.MaxValue;

        foreach (UnitState enemy in enemies)
        {
            if (!IsTargetable(curInfo, enemy, castleLocked)) continue;

            int distance = FootprintDistance(curUnit, enemy);
            if (distance > curInfo.AggroRange) continue;
            if (distance > closestDistance) continue;
            
            if (distance == closestDistance && closest is not null
                                            && enemy.Id >= closest.Value.Id) continue;

            closestDistance = distance;
            closest = enemy;
        }

        return closest;
    }

    private static bool IsTargetable(UnitInfo curInfo, UnitState enemy, bool castleLocked)
    {
        if (enemy.Health <= 0) return false;

        UnitInfo enemyInfo = UnitInfos.GetUnitInfo(enemy.Type);

        if (enemy.Type == UnitType.Castle && castleLocked) return false;
        if (enemyInfo.Layer == MovementLayer.Air && !curInfo.ranged) return false;
        if (curInfo.targetsBuildingsOnly && !enemyInfo.IsBuilding) return false;

        return true;
    }

    internal static bool IsOccupied(GameState state, Vector2Int position, MovementLayer layer, int ignoreUnitId)
    {
        return HasUnitAt(state.PlayerOne.Units, position, layer, ignoreUnitId)
               || HasUnitAt(state.PlayerTwo.Units, position, layer, ignoreUnitId);
    }

    private static bool HasUnitAt(List<UnitState> units, Vector2Int position, MovementLayer layer, int ignoreUnitId)
    {
        foreach (UnitState unit in units)
        {
  
            if (unit.Id == ignoreUnitId) continue;

            UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
            if (info.Layer != layer) continue;

            if (position.X < unit.Position.X) continue;
            if (position.X >= unit.Position.X + info.Size.X) continue;
            if (position.Y < unit.Position.Y) continue;
            if (position.Y >= unit.Position.Y + info.Size.Y) continue;

            return true;
        }
        return false;
    }

    internal static int CountTowers(List<UnitState> units)
    {
        int count = 0;
        foreach (UnitState unit in units)
        {
            if (unit.Type == UnitType.Tower) count++;
        }
        return count;
    }

    internal static long DistanceSquared(Vector2Int a, Vector2Int b)
    {
        long dx = (long)b.X - a.X;
        long dy = (long)b.Y - a.Y;
        return dx * dx + dy * dy;
    }

    // Chebyshev gap between two footprints. 0 means touching or overlapping, so a
    // range-1 melee unit connects from any edge or corner regardless of unit size.
    internal static int FootprintDistance(UnitState a, UnitState b)
    {
        return FootprintDistance(
            a.Position, UnitInfos.GetUnitInfo(a.Type).Size,
            b.Position, UnitInfos.GetUnitInfo(b.Type).Size);
    }

    // Same metric against raw rectangles, for callers reasoning about hypothetical
    // positions (the pathfinder asking "would I be in range if I stood here?").
    // Targeting, attack range and pathfinding all agree because they all end up here.
    internal static int FootprintDistance(Vector2Int aPos, Vector2Int aSize, Vector2Int bPos, Vector2Int bSize)
    {
        int aMaxX = aPos.X + aSize.X - 1;
        int aMaxY = aPos.Y + aSize.Y - 1;
        int bMaxX = bPos.X + bSize.X - 1;
        int bMaxY = bPos.Y + bSize.Y - 1;

        int dx = Math.Max(0, Math.Max(bPos.X - aMaxX, aPos.X - bMaxX));
        int dy = Math.Max(0, Math.Max(bPos.Y - aMaxY, aPos.Y - bMaxY));

        return Math.Max(dx, dy);
    }

    private static bool InRange(UnitState attacker, UnitState target, int range)
    {
        return FootprintDistance(attacker, target) <= range;
    }

    internal static List<UnitState> GetEnemyUnits(UnitState curUnit, GameState gameState)
    {
        return curUnit.Owner switch
        {
            PlayerId.One => gameState.PlayerTwo.Units,
            PlayerId.Two => gameState.PlayerOne.Units,
            _ => throw new ArgumentOutOfRangeException(nameof(curUnit), curUnit.Owner, null)
        };
    }

    internal static bool FootprintBlocked(GameState state, Vector2Int topLeft,
        Vector2Int size, UnitType unitType, int ignoreUnitId)
    {
        MovementLayer layer = UnitInfos.GetUnitInfo(unitType).Layer;

        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                Vector2Int cell = new(topLeft.X + x, topLeft.Y + y);

                if (!ArenaMap.IsPassable(cell, unitType)) return true;
                if (IsOccupied(state, cell, layer, ignoreUnitId)) return true;
            }
        }

        return false;
    }

    internal static bool FootprintBlocked(GameState state, Vector2Int topLeft, Vector2Int size,
        UnitType unitType, int ignoreUnitId, Vector2Int goal, Vector2Int goalSize)
    {
        MovementLayer layer = UnitInfos.GetUnitInfo(unitType).Layer;

        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                Vector2Int cell = new(topLeft.X + x, topLeft.Y + y);

                if (!ArenaMap.IsPassable(cell, unitType)) return true;

                // The goal's own cells don't block; that's the thing we're walking at.
                if (cell.X >= goal.X && cell.X < goal.X + goalSize.X
                                     && cell.Y >= goal.Y && cell.Y < goal.Y + goalSize.Y) continue;

                if (IsOccupied(state, cell, layer, ignoreUnitId)) return true;
            }
        }

        return false;
    }
}