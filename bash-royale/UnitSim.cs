namespace bash_royale;

// Handles targeting, movement and attacking across both armies. This lives apart from
// UnitSim/PlayerSim because resolving an attack needs to mutate the *enemy's* state.

public record ActionResult(UnitState unit, int targetIdx, int targetNewHP, bool didDamage);

public static class UnitSim
{
    public static ActionResult Update(UnitState curUnit, GameState gameState)
    {
        UnitInfo info = UnitInfos.GetUnitInfo(curUnit.Type);
        UnitState? target = FindNearestEnemy(curUnit, gameState, info.AggroRange);

        IUnitBehaviour behaviour;
        if (target is null)
        {
            behaviour = info.NeutralBehaviour;
        }
        else
        {
            bool inAttackRange = InRange(curUnit.Position, target.Value.Position, info.AttackRange);
            behaviour = inAttackRange ? info.AttackBehaviour : info.ChaseBehaviour;
        }

        return behaviour.Update(curUnit, gameState, target);
    }




    private static UnitState? FindNearestEnemy(UnitState curUnit, GameState gameState, int aggroRange)
    {
        Vector2Int curPosition = curUnit.Position;
        long rangeSquared = (long)aggroRange * aggroRange;

        List<UnitState> enemies = GetEnemyUnits(curUnit, gameState);
        UnitState? closest = null;
        long closestDistanceSquared = long.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            long distanceSquared = DistanceSquared(curPosition, enemies[i].Position);

            if (distanceSquared > rangeSquared) continue;
            if (distanceSquared >= closestDistanceSquared) continue;

            closestDistanceSquared = distanceSquared;
            closest = enemies[i];
        }

        return closest;
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

   
