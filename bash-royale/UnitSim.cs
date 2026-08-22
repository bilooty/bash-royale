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
            bool inAttackRange = InRange(curUnit, target.Value, info.AttackRange);
            if (inAttackRange)
            {
                behaviour = info.AttackBehaviour;

            }
            else
            {
                behaviour = info.ChaseBehaviour;
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
            // Skip the unit doing the moving; its own body isn't an obstacle.
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
        bool targetsBuildingOnly = UnitInfos.GetUnitInfo(curUnit.Type).targetsBuildingsOnly;
        bool ranged = UnitInfos.GetUnitInfo(curUnit.Type).ranged;
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
            // if targets layer is air and unit is not ranged, continue
            if (UnitInfos.GetUnitInfo(enemies[i].Type).Layer == MovementLayer.Air && ranged == false) continue;
            // Buildings-only units ignore troops and only target towers/castles.
            if (targetsBuildingOnly && !UnitInfos.GetUnitInfo(enemies[i].Type).IsBuilding) continue;
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

    // Chebyshev gap between two footprints. 0 means touching or overlapping, so a
    // range-1 melee unit connects from any edge or corner regardless of unit size.
    private static int FootprintDistance(UnitState a, UnitState b)
    {
        Vector2Int sizeA = UnitInfos.GetUnitInfo(a.Type).Size;
        Vector2Int sizeB = UnitInfos.GetUnitInfo(b.Type).Size;

        int aMaxX = a.Position.X + sizeA.X - 1;
        int aMaxY = a.Position.Y + sizeA.Y - 1;
        int bMaxX = b.Position.X + sizeB.X - 1;
        int bMaxY = b.Position.Y + sizeB.Y - 1;

        int dx = Math.Max(0, Math.Max(b.Position.X - aMaxX, a.Position.X - bMaxX));
        int dy = Math.Max(0, Math.Max(b.Position.Y - aMaxY, a.Position.Y - bMaxY));

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
    internal static bool FootprintBlocked(GameState state, Vector2Int topLeft,
        Vector2Int size, UnitType unitType, Vector2Int ignore)
    {
        MovementLayer layer = UnitInfos.GetUnitInfo(unitType).Layer;

        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                Vector2Int cell = new(topLeft.X + x, topLeft.Y + y);

                if (!ArenaMap.IsPassable(cell, unitType)) return true;
                if (IsOccupied(state, cell, layer, ignore)) return true;
            }
        }

        return false;
    }
    internal static bool FootprintBlocked(GameState state, Vector2Int topLeft, Vector2Int size,
        MovementLayer layer, Vector2Int ignore, Vector2Int goal, Vector2Int goalSize)
    {
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                Vector2Int cell = new(topLeft.X + x, topLeft.Y + y);

                if (!ArenaMap.IsPassable(cell, layer)) return true;

                // The goal's own cells don't block; that's the thing we're walking at.
                if (cell.X >= goal.X && cell.X < goal.X + goalSize.X
                                     && cell.Y >= goal.Y && cell.Y < goal.Y + goalSize.Y) continue;

                if (IsOccupied(state, cell, layer, ignore)) return true;
            }
        }

        return false;
    }
    internal static bool FootprintBlocked(GameState state, Vector2Int topLeft, Vector2Int size,
        UnitType unitType, Vector2Int ignore, Vector2Int goal, Vector2Int goalSize)
    {
        MovementLayer layer = UnitInfos.GetUnitInfo(unitType).Layer;

        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                Vector2Int cell = new(topLeft.X + x, topLeft.Y + y);

                if (!ArenaMap.IsPassable(cell, unitType)) return true;

                // The goal's own cells don't block - that's the thing we're walking at.
                if (cell.X >= goal.X && cell.X < goal.X + goalSize.X
                                     && cell.Y >= goal.Y && cell.Y < goal.Y + goalSize.Y) continue;

                if (IsOccupied(state, cell, layer, ignore)) return true;
            }
        }

        return false;
    }
}
    



   
