using System;
using UnityEngine;

namespace Units
{
    public interface ISpawnable
    {
        void Init(Transform destination, int team) { }
        void SetTeamColor(Color teamColor) { }
        void SetCopyMode(bool enabled) { }
        void PerformActionForEachUnit(Action<Unit> Action) { }
        void SetParentForUnits(Transform parent) { }
        void Release(bool destroyChildren) { }
        float baseOffset { get; }
        UnitData Data { get; }
        GameObject GetGameObject();
    }
}
