using System;
using UnityEngine;

namespace Units
{
    public interface ISpawnable
    {
        void Init(Transform destination, BulletFactory bulletFactory, int team) { }
        void SetTeamColor(Color teamColor) { }
        void SetCopyMode(bool enabled) { }
        void PerformActionForEachUnit(Action<Unit> Action) { }
        float baseOffset { get; }
        UnitData Data { get; }
        GameObject GetGameObject();
    }
}
