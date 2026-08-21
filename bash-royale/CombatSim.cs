namespace bash_royale;

// Handles targeting, movement and attacking across both armies. This lives apart from
// UnitSim/PlayerSim because resolving an attack needs to mutate the *enemy's* state.
public static class CombatSim
{
    public static void Update(ref PlayerState playerOne, ref PlayerState playerTwo, float deltaSeconds)
    {
        UpdateUnits(playerOne.Units, ref playerTwo, deltaSeconds);
        UpdateUnits(playerTwo.Units, ref playerOne, deltaSeconds);
    }

    private static void UpdateUnits(List<UnitState> units, ref PlayerState enemy, float deltaSeconds)
    {
        for (int i = 0; i < units.Count; i++)
        {
            UnitState unit = units[i];
            if (unit.Health <= 0)
                continue;

            if (!TryFindNearestTarget(unit.Position, enemy, out Vector2Int targetPosition, out int enemyUnitIndex, out TowerSlot towerSlot))
            {
                units[i] = unit;
                continue;
            }

            UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
            double distance = unit.Position.DistanceTo(targetPosition);

            if (distance <= info.Range)
            {
                unit.AttackCooldown -= deltaSeconds;
                if (unit.AttackCooldown <= 0f)
                {
                    unit.AttackCooldown = 1f / info.AttacksPerSecond;
                    ApplyDamage(ref enemy, enemyUnitIndex, towerSlot, info.Damage);
                }
            }
            else
            {
                Vector2Int direction = new(Math.Sign(targetPosition.X - unit.Position.X), Math.Sign(targetPosition.Y - unit.Position.Y));
                unit = UnitSim.Step(unit, direction, deltaSeconds);
            }

            units[i] = unit;
        }
    }

    private static bool TryFindNearestTarget(Vector2Int from, PlayerState enemy, out Vector2Int targetPosition, out int enemyUnitIndex, out TowerSlot towerSlot)
    {
        targetPosition = default;
        enemyUnitIndex = -1;
        towerSlot = TowerSlot.King;

        double bestDistance = double.MaxValue;
        bool found = false;

        for (int i = 0; i < enemy.Units.Count; i++)
        {
            if (enemy.Units[i].Health <= 0)
                continue;

            double distance = from.DistanceTo(enemy.Units[i].Position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                targetPosition = enemy.Units[i].Position;
                enemyUnitIndex = i;
                found = true;
            }
        }

        // Units only fall back to attacking a tower once no living enemy units are closer.
        if (found)
            return true;

        foreach (TowerSlot slot in Arena.GetTargetableTowers(enemy))
        {
            Vector2Int towerPosition = Arena.GetTowerPosition(enemy.Id, slot);
            double distance = from.DistanceTo(towerPosition);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                targetPosition = towerPosition;
                towerSlot = slot;
                found = true;
            }
        }

        return found;
    }

    private static void ApplyDamage(ref PlayerState enemy, int enemyUnitIndex, TowerSlot towerSlot, int damage)
    {
        if (enemyUnitIndex >= 0)
        {
            UnitState target = enemy.Units[enemyUnitIndex];
            target.Health -= damage;
            enemy.Units[enemyUnitIndex] = target;
            return;
        }

        switch (towerSlot)
        {
            case TowerSlot.Left:
                enemy.LeftTowerHealth = Math.Max(0, enemy.LeftTowerHealth - damage);
                break;
            case TowerSlot.Right:
                enemy.RightTowerHealth = Math.Max(0, enemy.RightTowerHealth - damage);
                break;
            case TowerSlot.King:
                enemy.KingTowerHealth = Math.Max(0, enemy.KingTowerHealth - damage);
                break;
        }
    }
}
