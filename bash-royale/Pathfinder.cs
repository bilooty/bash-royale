namespace bash_royale;

public static class Pathfinder
{
    // Hard cap on expansions. Without it, an unreachable goal (a walled-off target, a
    // ground unit asked to path at a flyer) makes A* flood the entire arena every tick
    // for every such unit before returning null.
    private const int MaxExpansions = 4096;

    // Convenience overload: everything the search needs is already on the two units.
    public static Vector2Int? NextStep(UnitState unit, UnitState target, GameState state)
    {
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);

        return NextStep(
            from: unit.Position,
            to: target.Position,
            size: info.Size,
            goalSize: UnitInfos.GetUnitInfo(target.Type).Size,
            unitType: unit.Type,
            ignoreUnitId: unit.Id,
            stopDistance: info.AttackRange,
            state: state);
    }

    // A*
    //
    // `from` and `to` are top-left origins; `stopDistance` is a footprint gap measured
    // with UnitSim.FootprintDistance, so passing AttackRange makes the search terminate
    // exactly where the attack behaviour would take over. Previously the search only
    // stopped on reaching the target's origin cell, which meant walking *into* a large
    // target and expanding a pile of nodes past the point the unit would have stopped.
    public static Vector2Int? NextStep(Vector2Int from, Vector2Int to, Vector2Int size,
        Vector2Int goalSize, UnitType unitType, int ignoreUnitId, int stopDistance, GameState state)
    {
        if (AtGoal(from, size, to, goalSize, stopDistance)) return null;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> costSoFar = new();
        HashSet<Vector2Int> expanded = new();

        PriorityQueue<Vector2Int, (int, int, int)> frontier = new();

        cameFrom[from] = from;
        costSoFar[from] = 0;
        frontier.Enqueue(from, (Heuristic(from, size, to, goalSize, stopDistance), from.Y, from.X));

        int expansions = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            // There's no decrease-key, so the queue can hold stale copies of a cell that
            // was later reached more cheaply. Expanding each cell once is enough: the
            // heuristic is consistent, so the first pop of a cell is its best.
            if (!expanded.Add(current)) continue;

            if (AtGoal(current, size, to, goalSize, stopDistance)) return FirstStep(cameFrom, from, current);

            if (++expansions > MaxExpansions) return null;

            int nextCost = costSoFar[current] + 1;

            foreach (Vector2Int direction in Vector2Int.Cardinals)
            {
                Vector2Int neighbour = current + direction;

                if (expanded.Contains(neighbour)) continue;

                // Cheap check before the expensive one: the footprint scan touches
                // size.X * size.Y cells and both unit lists, so don't run it for a
                // neighbour we already have an equal-or-better route to.
                if (costSoFar.TryGetValue(neighbour, out int known) && known <= nextCost) continue;

                // The goal is exempt, it will always be occupied. Exempt its whole
                // footprint, or a big unit can never reach a big target.
                if (UnitSim.FootprintBlocked(state, neighbour, size, unitType, ignoreUnitId, to, goalSize)) continue;

                costSoFar[neighbour] = nextCost;
                cameFrom[neighbour] = current;
                frontier.Enqueue(neighbour,
                    (nextCost + Heuristic(neighbour, size, to, goalSize, stopDistance), neighbour.Y, neighbour.X));
            }
        }

        return null;
    }

    // Chebyshev footprint gap, shared with targeting and attack-range checks so the
    // pathfinder can't stop one tile short of, or one tile past, where the attack
    // behaviour expects to engage.
    private static bool AtGoal(Vector2Int cell, Vector2Int size, Vector2Int goal, Vector2Int goalSize, int stopDistance)
    {
        return UnitSim.FootprintDistance(cell, size, goal, goalSize) <= stopDistance;
    }

    // Manhattan gap between footprints, less the distance we're allowed to stop short.
    // Measuring origin-to-origin would overestimate against a large target and break
    // admissibility, which shows up as visibly silly detours around big buildings.
    private static int Heuristic(Vector2Int cell, Vector2Int size, Vector2Int goal, Vector2Int goalSize, int stopDistance)
    {
        int cellMaxX = cell.X + size.X - 1;
        int cellMaxY = cell.Y + size.Y - 1;
        int goalMaxX = goal.X + goalSize.X - 1;
        int goalMaxY = goal.Y + goalSize.Y - 1;

        int dx = Math.Max(0, Math.Max(goal.X - cellMaxX, cell.X - goalMaxX));
        int dy = Math.Max(0, Math.Max(goal.Y - cellMaxY, cell.Y - goalMaxY));

        return Math.Max(0, dx + dy - stopDistance);
    }

    private static Vector2Int FirstStep(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int from, Vector2Int goalNode)
    {
        Vector2Int current = goalNode;

        while (cameFrom[current] != from)
        {
            current = cameFrom[current];
        }

        return current;
    }
}