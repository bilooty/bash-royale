using Microsoft.VisualBasic;

namespace bash_royale;

public enum ProjectileType
{
    Zap
}

public struct ProjectileState
{
    public ProjectileState(ProjectileType type, PlayerId owner, Vector2Int position)
    {
        Type = type;
        Owner = owner;
        Position = position;
        ShouldDie = false;
    }
    public ProjectileType Type;
    public bool ShouldDie;
    public Vector2Int Position;
    public PlayerId Owner;
}

public record ProjectileInfo(
    IProjectileBehaviour Behaviour
);

public interface IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState);
}

public struct DamageInstance(int index, int damage);
public class InstantDamage(Vector2Int size, int damage) : IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState)
    {
        PlayerState enemy = GameState.GetPlayerState(gameState, state.Owner == PlayerId.One ? PlayerId.Two : PlayerId.One);
        List<UnitState> units = enemy.Units;
        List<DamageInstance> damageInstances = new();
        for (int i = 0; i < units.Count; i++)
        {   
            UnitState unit = units[i];
            if (unit.Position.X >= state.Position.X
                && unit.Position.Y >= state.Position.Y
                && unit.Position.X < state.Position.X + size.X
                && unit.Position.Y < state.Position.Y)
            {
                damageInstances.Add(new DamageInstance(i, damage));
            }
        }
        return new ProjectileResult(state, damageInstances);
    }
}
public struct ProjectileResult(ProjectileState State, List<DamageInstance> DamageInstances); 
