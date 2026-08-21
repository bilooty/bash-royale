namespace bash_royale;

public interface IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx);
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
     
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        
        if (target is null) return ActionResult.NoAttack(unit);
        
        UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
        if (unit.Ticks % info.TicksPerAttack != 0) return ActionResult.NoAttack(unit);
        
        return new ActionResult(unit, targetIdx, damage, true);
    }
}

public class DoNothing : IUnitBehaviour
{
    public ActionResult Update(UnitState unit, GameState state, UnitState? target, int targetIdx)
    {
        return ActionResult.NoAttack(unit);
    }
}

