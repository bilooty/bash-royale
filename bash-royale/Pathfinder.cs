namespace bash_royale;

public static class Pathfinder
{
    private const int MaxExpansions = 4096;

    // sqrt(2) scaled by 1000. A cell can sit this much further away in Manhattan terms
    // than in straight-line terms, which the heuristic has to allow for.
    private const int Sqrt2Scaled = 1414;

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
    // `stopDistance` is a straight-line footprint gap, matching UnitSim's metric, so
    // passing AttackRange ends the search exactly where the attack behaviour takes over.
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

            // No decrease-key, so the queue holds stale copies of cells later reached more
            // cheaply. One expansion per cell is enough: the heuristic is consistent, so
            // the first pop of a cell is its best.
            if (!expanded.Add(current)) continue;

            if (AtGoal(current, size, to, goalSize, stopDistance)) return FirstStep(cameFrom, from, current);

            if (++expansions > MaxExpansions) return null;

            int nextCost = costSoFar[current] + 1;

            foreach (Vector2Int direction in Vector2Int.Cardinals)
            {
                Vector2Int neighbour = current + direction;

                if (expanded.Contains(neighbour)) continue;

                // Cheap check before the expensive one: the footprint scan touches
                // size.X * size.Y cells across both unit lists.
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

    // Shares UnitSim's straight-line metric, so the pathfinder can't stop one tile short
    // of, or one tile past, where the attack behaviour expects to engage.
    private static bool AtGoal(Vector2Int cell, Vector2Int size, Vector2Int goal, Vector2Int goalSize, int stopDistance)
    {
        return UnitSim.WithinRange(
            UnitSim.FootprintDistanceSquared(cell, size, goal, goalSize), stopDistance);
    }

    // Movement is 4-connected and uniform cost, so the true cost between two cells is
    // Manhattan. The stop radius is a circle though, and a cell inside a circle of radius
    // r can be up to r*sqrt(2) away in Manhattan terms — subtracting only r would
    // overestimate and cost admissibility. Subtracting the larger bound keeps A* optimal.
    private static int Heuristic(Vector2Int cell, Vector2Int size, Vector2Int goal, Vector2Int goalSize, int stopDistance)
    {
        (int dx, int dy) = UnitSim.FootprintAxisGaps(cell, size, goal, goalSize);

        int slack = (stopDistance * Sqrt2Scaled + 999) / 1000; // ceil(stopDistance * sqrt2)

        return Math.Max(0, dx + dy - slack);
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