namespace bash_royale;

public static class Pathfinder
{
    // A*
    public static Vector2Int? NextStep(Vector2Int from, Vector2Int to, Vector2Int size, Vector2Int goalSize, MovementLayer layer, GameState state)
    {
        if (from == to) return null;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> costSoFar = new();

        PriorityQueue<Vector2Int, (int, int, int)> frontier = new();

        cameFrom[from] = from;
        costSoFar[from] = 0;
        frontier.Enqueue(from, (Heuristic(from, to), from.Y, from.X));

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == to) return FirstStep(cameFrom, from, to);

            int nextCost = costSoFar[current] + 1;

            foreach (Vector2Int direction in Vector2Int.Cardinals)
            {
                Vector2Int neighbour = current + direction;

                // The goal is exempt, it will always be occupied. Exempt its whole
                // footprint, or a big unit can never reach a big target.
                if (UnitSim.FootprintBlocked(state, neighbour, size, layer, from, to, goalSize)) continue;

                if (costSoFar.TryGetValue(neighbour, out int known) && known <= nextCost) continue;

                costSoFar[neighbour] = nextCost;
                cameFrom[neighbour] = current;
                frontier.Enqueue(neighbour, (nextCost + Heuristic(neighbour, to), neighbour.Y, neighbour.X));
            }
        }

        return null;
    }

    private static bool InGoal(Vector2Int cell, Vector2Int goal, Vector2Int goalSize)
    {
        if (cell.X < goal.X || cell.X >= goal.X + goalSize.X) return false;
        if (cell.Y < goal.Y || cell.Y >= goal.Y + goalSize.Y) return false;
        return true;
    }

    // Manhattan distance
    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static Vector2Int FirstStep(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int from, Vector2Int to)
    {
        Vector2Int current = to;

        while (cameFrom[current] != from)
        {
            current = cameFrom[current];
        }

        return current;
    }
}