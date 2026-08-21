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
            behaviour = inAttackRange ? info.AttackBehaviour : info.ChaseBehaviour;
        }

        curUnit.Ticks++;
        return behaviour.Update(curUnit, gameState, target, targetIdx ?? -1);
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

    private static long DistanceSquared(Vector2Int a, Vector2Int b)
    {
        long dx = (long)b.X - a.X;
        long dy = (long)b.Y - a.Y;
        return dx * dx + dy * dy;
    }

    private static bool InRange(Vector2Int curPosition, Vector2Int target, int range)
    {
        return DistanceSquared(curPosition, target) <= (long)range * range;
    }

    private static List<UnitState> GetEnemyUnits(UnitState curUnit, GameState gameState)
    {
        return curUnit.Owner switch
        {
            PlayerId.One => gameState.PlayerTwo.Units,
            PlayerId.Two => gameState.PlayerOne.Units,
            _ => throw new ArgumentOutOfRangeException(nameof(curUnit), curUnit.Owner, null)
        };
    }
}

   
