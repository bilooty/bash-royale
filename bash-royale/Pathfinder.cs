namespace bash_royale;

public static class Pathfinder
{
    // Returns the next cell to step to, or null if no route exists.
    // Terrain only — occupancy is handled by the caller when it steps.
    public static Vector2Int? NextStep(Vector2Int from, Vector2Int to, MovementLayer layer)
    {
        if (from == to) return null;
        if (!ArenaMap.IsPassable(to, layer)) return null;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Queue<Vector2Int> frontier = new();

        cameFrom[from] = from;
        frontier.Enqueue(from);

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == to) return FirstStep(cameFrom, from, to);

            foreach (Vector2Int direction in Vector2Int.Cardinals)
            {
                Vector2Int neighbour = current + direction;
                if (!ArenaMap.IsPassable(neighbour, layer)) continue;
                if (cameFrom.ContainsKey(neighbour)) continue;

                cameFrom[neighbour] = current;
                frontier.Enqueue(neighbour);
            }
        }

        return null;
    }

    // Walk the parent chain back from the goal until we reach the tile next to the start.
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