namespace bash_royale;

public enum TowerSlot
{
    King,
    Left,
    Right
}

// Tower positions are fixed by the arena layout, so they're computed from
// GameSettings rather than stored as mutable state.
public static class Arena
{
    public static Vector2Int GetTowerPosition(PlayerId owner, TowerSlot slot)
    {
        int centerX = GameSettings.GAME_WIDTH / 2;
        int laneOffsetX = GameSettings.GAME_WIDTH / 4;
        int kingY = owner == PlayerId.One ? GameSettings.GAME_HEIGHT - 1 : 0;
        int princessY = owner == PlayerId.One ? GameSettings.GAME_HEIGHT - 3 : 2;

        return slot switch
        {
            TowerSlot.King => new Vector2Int(centerX, kingY),
            TowerSlot.Left => new Vector2Int(centerX - laneOffsetX, princessY),
            TowerSlot.Right => new Vector2Int(centerX + laneOffsetX, princessY),
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };
    }

    // King tower can't be targeted until both princess towers on that side have fallen.
    public static IEnumerable<TowerSlot> GetTargetableTowers(PlayerState player)
    {
        bool leftAlive = player.LeftTowerHealth > 0;
        bool rightAlive = player.RightTowerHealth > 0;

        if (leftAlive) yield return TowerSlot.Left;
        if (rightAlive) yield return TowerSlot.Right;
        if (!leftAlive && !rightAlive) yield return TowerSlot.King;
    }
}
