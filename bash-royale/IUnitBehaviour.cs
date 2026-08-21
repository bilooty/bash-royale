namespace bash_royale;

public interface IUnitBehaviour
{
    public UnitState Update(UnitState unit, GameState state, UnitState? target);
}
public class WalkForwards(int speed) : IUnitBehaviour
{
    public UnitState Update(UnitState unit, GameState state,UnitState? target)
    {
        // walk to
        return unit;
    }
}
public class ChaseBehaviour(int speed) : IUnitBehaviour
{
    public UnitState Update(UnitState unit, GameState state, UnitState? target)
    {
        // should chase nearest enemy unit?
        return unit;
    }
}

public class AttackBehaviour(int damage, bool isRanged) : IUnitBehaviour
{
     
    public UnitState Update(UnitState unit, GameState state, UnitState? target)
    {
        // attack enemy unit that is in range
        if (target is null) return unit;
        return unit;
    }
}

