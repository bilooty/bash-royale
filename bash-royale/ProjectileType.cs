using Microsoft.VisualBasic;

namespace bash_royale;

public enum ProjectileType
{
    Zap,
    ZapEffect
}

public enum TargetType
{
    Position,
    Location
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
    public int Ticks = 0;
    public PlayerId Owner;
    public Vector2Int TargetLoc;
    public int TargetIndex;

    public static Dictionary<ProjectileType, ProjectileInfo> Infos = new Dictionary<ProjectileType, ProjectileInfo>
    {
        [ProjectileType.Zap] = new ProjectileInfo(
        [
            new InstantDamage(new Vector2Int(3, 3), 10),
            new SummonProj(ProjectileType.ZapEffect)
            ]
            ),
        [ProjectileType.ZapEffect] = new ProjectileInfo(
            [
            new Linger(10)])
    };
}

public record ProjectileInfo(
    List<IProjectileBehaviour> Behaviours
);



public interface IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState);
}

public record DamageInstance(int Index, int Damage, PlayerId targetPlayer);

public class Linger(int duration) : IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState)
    {
        if (state.Ticks > duration)
        {
            state.ShouldDie = true;
        }

        return new ProjectileResult(state, [], []);
    }
}

public class SummonProj(ProjectileType type) : IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState)
    {
        return new ProjectileResult(state, [], [new ProjectileState(type, state.Owner, state.Position)]);
    }
}



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
            if (Vector2Int.Intersects(unit.Position, UnitInfos.GetUnitInfo(unit.Type).Size, 
                    state.Position, size))
            {
                System.Console.WriteLine("Hit!!!");
                damageInstances.Add(new DamageInstance(i, damage, enemy.Id));
            }
        }

        state.ShouldDie = true;
        return new ProjectileResult(state, damageInstances, []);
    }
}
public record ProjectileResult(
    ProjectileState State,
    List<DamageInstance> DamageInstances,
    List<ProjectileState> NewProjectiles
    ); 
