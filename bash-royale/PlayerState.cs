namespace bash_royale;

public struct PlayerState
{
    public List<UnitState> Units;
}

public static class PlayerSim
{
    public static PlayerState Update(PlayerState player)
    {
        for (int i = 0; i < player.Units.Count; i++)
        {
            UnitState unit = player.Units[i];
            player.Units[i] = UnitSim.Update(unit);
        }
        return player;
    }
}