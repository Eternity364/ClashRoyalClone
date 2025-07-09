using UnityEngine;
using UnityEngine.Events;

public abstract class ControlScheme
{
    private static ControlScheme _instance;
    public static ControlScheme Instance
    {
        get { return _instance; }
        protected set { _instance = value; }
    }

    protected ControlScheme()
    {
        _instance = this;
    }

    public abstract void PutUnitOnTheField(
        Vector3 position,
        Units.Type unitType,
        bool spawn,
        bool payElixir,
        UnityAction<Vector3, Units.Type, bool, bool> spawnAction);
}