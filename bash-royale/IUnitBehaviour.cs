namespace bash_royale;

public interface IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx);
}
// FOR ALL NON ATTACKS ENSURE ACTIONRESULT.ISATTACK IS FALSE
public class WalkForwards(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state,UnitState? target, int targetIdx)
    {
        // ok get list of enemy units
        // filter list to contain only buildings 
        // sort by distance
        // walk toward closest
        // A*
        
        return null;
    }
}
public class ChaseBehaviour(int speed) : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {   
        // A* towards closest enemy unit
        
        // should chase nearest enemy unit?
        return null;
    }
}

public class AttackBehaviour(int damage) : IUnitBehaviour
{
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        // attack enemy unit that is in range
        
        // doesn't move 
        // for attack rate subtract damage from enemy hp 
        if (target is null) return null;
        return null;
    }
}

public class DoNothing() : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        return null;
    }
}

