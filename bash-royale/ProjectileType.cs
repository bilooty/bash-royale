using System.Numerics;

namespace bash_royale;

public enum ProjectileType
{
    Zap,
    FireBall,
    ZapEffect,
    Arrow,
    CannonBall,
    WizardBall,
    WizardBoom
    
}

public enum TargetType
{
    Unit,
    Location
}
public struct ProjectileState
{
    public ProjectileState(ProjectileType type, PlayerId owner, Vector2Int position)
    {
        Type = type;
        Owner = owner;
        Position = position;
        SubPosition = position * 1000;
        ShouldDie = false;
        Ticks = 0;
    }
    public ProjectileType Type;
    public bool ShouldDie;
    public Vector2Int Position;
    public int Ticks = 0;
    public PlayerId Owner;
    public Vector2Int SubPosition;
    public Vector2Int TargetLoc;
    public int TargetId;


    public static Dictionary<ProjectileType, ProjectileInfo> Infos = new Dictionary<ProjectileType, ProjectileInfo>
    {
        [ProjectileType.WizardBoom] = new ProjectileInfo([ new Linger(4), new InstantDamage(new Vector2Int(3,3), 450)], size:new Vector2Int(3,3), targetType:TargetType.Location),
        [ProjectileType.ZapEffect] = new ProjectileInfo(
            [
            new Linger(4)], size:new Vector2Int(3, 3), targetType:TargetType.Location),
        
        [ProjectileType.Zap] = new ProjectileInfo([
            new InstantDamage(new Vector2Int(3, 3), 120),
            new SummonProj(ProjectileType.ZapEffect),
            
        ], targetType: TargetType.Location, size:new Vector2Int(3,3)),
        [ProjectileType.FireBall] = new ProjectileInfo([new InstantDamage(new Vector2Int(3, 3), 450)], targetType: TargetType.Location),
        [ProjectileType.Arrow] = new ProjectileInfo([new Missile(1000, 100)], null, TargetType.Unit),
        [ProjectileType.CannonBall] = new ProjectileInfo([new Missile(1000, 200)], null, TargetType.Unit),
        [ProjectileType.WizardBall] = new ProjectileInfo([new Splash(1000, ProjectileType.WizardBoom)])

    };
}

public class ProjectileInfo(
    List<IProjectileBehaviour> behaviours,
    Vector2Int? size = null,
    TargetType? targetType = null
)
{
    public TargetType TargetType = targetType ?? TargetType.Unit;
    public List<IProjectileBehaviour> Behaviours => behaviours;
    public Vector2Int? Size => size;
}



public interface IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState);
}
public class Splash(int speed, ProjectileType toCreate) : MoveTowards(speed)
{
    public override ProjectileResult OnArrive(ProjectileState state, GameState gameState)
    {
        state.ShouldDie = true;
        PlayerId targetPlayer = state.Owner == PlayerId.Two ? PlayerId.One :  PlayerId.Two;
        Vector2Int size = ProjectileState.Infos[toCreate].Size ?? new Vector2Int(1, 1);
        Vector2Int offset = new Vector2Int(size.X / 2, size.Y / 2);
      
        return new ProjectileResult(state, [], [new ProjectileState(toCreate, state.Owner, state.Position - offset)]);
    }
}
public class Missile(int speed, int damage) : MoveTowards(speed)
{
    public override ProjectileResult OnArrive(ProjectileState state, GameState gameState)
    {
        state.ShouldDie = true;
        PlayerId targetPlayer = state.Owner == PlayerId.Two ? PlayerId.One :  PlayerId.Two;
        return new ProjectileResult(state, [new DamageInstance(state.TargetId, damage, targetPlayer)], []);
    }
}
public class MoveTowards(int speed) : IProjectileBehaviour
{
    // Fixed-point scale: 1000 "sub-units" per grid cell. Keeps movement in ints
    // so both machines compute byte-identical positions each tick.
    private const int SCALE = 1000;
    private readonly int _speed = speed; // sub-units per tick, e.g. 400 = 0.4 cells/tick

    public int Speed = speed;
    public virtual ProjectileResult OnArrive(ProjectileState state, GameState gameState)
    {
        state.ShouldDie = true;
        return new ProjectileResult(state, [], []);
    }
    public ProjectileResult Update(ProjectileState state, GameState gameState)
    {
        ProjectileInfo info = ProjectileState.Infos[state.Type];
        PlayerState enemy = 
            GameState.GetPlayerState(gameState, state.Owner == PlayerId.One ? PlayerId.Two : PlayerId.One);
        Vector2Int target = info.TargetType == TargetType.Location
            ? state.TargetLoc
            : enemy.Units.First(s => s.Id == state.TargetId).Position;

        int dx = target.X * SCALE - state.SubPosition.X;
        int dy = target.Y * SCALE - state.SubPosition.Y;

        long distSq = (long)dx * dx + (long)dy * dy;
        long stepSq = (long)_speed * _speed;

        if (distSq <= stepSq)
        {
            // Close enough to arrive this tick - snap exactly onto the target
            // rather than overshooting past it.
            System.Console.WriteLine("ARRIVED!");
            state.SubPosition = new Vector2Int(target.X * SCALE, target.Y * SCALE);
            state.Position = target;
            return OnArrive(state, gameState); 
        }

        int dist = IntSqrt(distSq);
        System.Console.WriteLine("dist " +  dist + "speed " + _speed);
        int stepX = (int)((long)dx * _speed / dist);
        int stepY = (int)((long)dy * _speed / dist);
        System.Console.WriteLine("Moved from:  " + state.Position + " " + state.SubPosition);
        state.SubPosition = new Vector2Int(state.SubPosition.X + stepX, state.SubPosition.Y + stepY);
        state.Position = new Vector2Int(state.SubPosition.X / SCALE, state.SubPosition.Y / SCALE);
        System.Console.WriteLine("Moved to:  " + state.Position + " " + state.SubPosition);
        return new ProjectileResult(state, new List<DamageInstance>(), new List<ProjectileState>());
    }

    private static int IntSqrt(long n)
    {
        if (n <= 0) return 0;
        long x = n, y = (x + 1) / 2;
        while (y < x) { x = y; y = (x + n / x) / 2; }
        return (int)x;
    }
}
public record DamageInstance(int Id, int Damage, PlayerId targetPlayer);

public class Linger(int duration) : IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState)
    {
        System.Console.WriteLine("Linger at tick: " + state.Ticks + "/" + duration);
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
        System.Console.WriteLine("Summon projectile");
        return new ProjectileResult(state, [], [new ProjectileState(type, state.Owner, state.Position)]);
    }
}



public class InstantDamage(Vector2Int size, int damage) : IProjectileBehaviour
{
    public ProjectileResult Update(ProjectileState state, GameState gameState)
    {

        if (state.Ticks > 0)
        {
            return new ProjectileResult(state, [], []);
        }
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
                damageInstances.Add(new DamageInstance(unit.Id, damage, enemy.Id));
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
