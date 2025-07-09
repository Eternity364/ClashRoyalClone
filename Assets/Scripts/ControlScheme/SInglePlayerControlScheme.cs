using UnityEngine;
using UnityEngine.Events;

public class SinglePlayerControlScheme : ControlScheme
{
    public override void PutUnitOnTheField(
        Vector3 position,
        Units.Type unitType,
        bool spawn,
        bool payElixir,
        UnityAction<Vector3, Units.Type, bool, bool> spawnAction)
        {
            spawnAction?.Invoke(position, unitType, spawn, payElixir);
        }
}