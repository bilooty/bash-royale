namespace bash_royale;

public interface IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target);
}
// FOR ALL NON ATTACKS ENSURE ACTIONRESULT.ISATTACK IS FALSE
public class WalkForwards(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state,UnitState? target)
    {
        // ok get list of enemy units
        // filter list to contain only buildings 
        // sort by distance
        // walk toward closest
        // A*
        
        return unit;
    }
}
public class ChaseBehaviour(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target)
    {   
        // A* towards closest enemy unit
        
        // should chase nearest enemy unit?
        return unit;
    }
}

public class AttackBehaviour(int damage) : IUnitBehaviour
{
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target)
    {
        // attack enemy unit that is in range
        
        // doesn't move 
        // for attack rate subtract damage from enemy hp 
        if (target is null) return unit;
        return unit;
    }
}

public class DoNothing() : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target)
    {
        return unit;
    }
}

