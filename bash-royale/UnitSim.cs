namespace bash_royale;

// Handles targeting, movement and attacking across both armies. This lives apart from
// UnitSim/PlayerSim because resolving an attack needs to mutate the *enemy's* state.

public record ActionResult(UnitState unit, int targetIdx, int damage, bool didDamage)
{
    public static ActionResult NoAttack(UnitState unit) => new(unit, -1, 0, false);
}

public static class UnitSim
{
    public static ActionResult Update(UnitState curUnit, GameState gameState)
    {
        UnitInfo info = UnitInfos.GetUnitInfo(curUnit.Type);
        int? targetIdx = FindNearestEnemy(curUnit, gameState, info.AggroRange);

        IUnitBehaviour behaviour;
        UnitState? target = null;

        if (targetIdx is null)
        {
            behaviour = info.NeutralBehaviour;
        }
        else
        {
            target = GetEnemyUnits(curUnit, gameState)[targetIdx.Value];
            bool inAttackRange = InRange(curUnit.Position, target.Value.Position, info.AttackRange);
            if (inAttackRange)
            {
                behaviour = info.AttackBehaviour;
            
            }
            else
            {
               behaviour =  info.ChaseBehaviour;
            }
         
            
        }

        curUnit.Ticks++;
        return behaviour.Update(curUnit, gameState, target, targetIdx ?? -1);
    }
    internal static bool IsOccupied(GameState state, Vector2Int position, MovementLayer layer, Vector2Int ignore)
    {
        return HasUnitAt(state.PlayerOne.Units, position, layer, ignore)
               || HasUnitAt(state.PlayerTwo.Units, position, layer, ignore);
    }

    private static bool HasUnitAt(List<UnitState> units, Vector2Int position, MovementLayer layer, Vector2Int ignore)
    {
        foreach (UnitState unit in units)
        {
            // Skip the unit doing the moving — its own body isn't an obstacle.
            if (unit.Position == ignore) continue;

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
    private static int? FindNearestEnemy(UnitState curUnit, GameState gameState, int aggroRange)
    {
        Vector2Int curPosition = curUnit.Position;
        long rangeSquared = (long)aggroRange * aggroRange;
        List<UnitState> enemies = GetEnemyUnits(curUnit, gameState);

        int? closestIndex = null;
        long closestDistanceSquared = long.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            long distanceSquared = DistanceSquared(curPosition, enemies[i].Position);

            if (distanceSquared > rangeSquared) continue;
            if (distanceSquared >= closestDistanceSquared) continue;

            closestDistanceSquared = distanceSquared;
            closestIndex = i;
        }

        return closestIndex;
    }

    internal static long DistanceSquared(Vector2Int a, Vector2Int b)
    {
        long dx = (long)b.X - a.X;
        long dy = (long)b.Y - a.Y;
        return dx * dx + dy * dy;
    }

    private static bool InRange(Vector2Int curPosition, Vector2Int target, int range)
    {
        int dx = Math.Abs(target.X - curPosition.X);
        int dy = Math.Abs(target.Y - curPosition.Y);
        return Math.Max(dx, dy) <= range;
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
        Vector2Int size, MovementLayer layer, Vector2Int ignore)
    {
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                Vector2Int cell = new(topLeft.X + x, topLeft.Y + y);

                if (!ArenaMap.IsPassable(cell, layer)) return true;
                if (IsOccupied(state, cell, layer, ignore)) return true;
            }
        }

        return false;
    }
}

   
