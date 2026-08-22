namespace bash_royale;


public static class Pathfinder
{   
    
    // Units path to a free cell beside the target rather than the target itself, so
    // they spread around it instead of queueing behind one another.
    public static Vector2Int NearestFreeApproach(Vector2Int from, Vector2Int target, MovementLayer layer, GameState state)
    {
        Vector2Int best = target;
        long bestDistance = long.MaxValue;

        foreach (Vector2Int direction in Vector2Int.Cardinals)
        {
            Vector2Int candidate = target + direction;
            if (!ArenaMap.IsPassable(candidate, layer)) continue;
            if (candidate != from && UnitSim.IsOccupied(state, candidate, layer)) continue;

            long distance = UnitSim.DistanceSquared(from, candidate);
            if (distance >= bestDistance) continue;

            bestDistance = distance;
            best = candidate;
        }

        return best;
    }
    // Returns the next cell to step to, or null if no route exists.
    // Terrain only — occupancy is checked by the caller when it steps, so a unit
    // blocked by a body waits a tick instead of re-routing around it.
    public static Vector2Int? NextStep(Vector2Int from, Vector2Int to, MovementLayer layer)
    {
        if (from == to) return null;
        if (!ArenaMap.IsPassable(to, layer)) return null;

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
                if (!ArenaMap.IsPassable(neighbour, layer)) continue;
                if (costSoFar.TryGetValue(neighbour, out int known) && known <= nextCost) continue;

                costSoFar[neighbour] = nextCost;
                cameFrom[neighbour] = current;
                frontier.Enqueue(neighbour, (nextCost + Heuristic(neighbour, to), neighbour.Y, neighbour.X));
            }
        }

        return null;
    }

    // Manhattan distance — never overestimates on a 4-way grid, so A* stays optimal.
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