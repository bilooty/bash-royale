namespace bash_royale;

// Handles targeting, movement and attacking across both armies. This lives apart from
// UnitSim/PlayerSim because resolving an attack needs to mutate the *enemy's* state.
public static class UnitSim
{
    public static void Update(UnitState curUnit, GameState gameState)
    {
        // find if unit in attack range and if it is then we 
        if (FindEnemyInAttackRange(curUnit, gameState) is not null)
        {
            curUnit = curUnit;
        }


    }

    private static UnitState? FindEnemyInAttackRange(UnitState curUnit, GameState gameState)
    {
        int range = UnitInfos.GetUnitInfo(curUnit.Type).Range;
        Vector2Int curPosition = curUnit.Position;
        long rangeSquared = (long)range * range;

        UnitState? closest = null;
        long closestDistanceSquared = long.MaxValue;

        foreach (UnitState enemy in GetEnemyUnits(curUnit, gameState))
        {
            long distanceSquared = DistanceSquared(curPosition, enemy.Position);

            if (distanceSquared > rangeSquared) continue;
            if (distanceSquared >= closestDistanceSquared) continue;

            closestDistanceSquared = distanceSquared;
            closest = enemy;
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
