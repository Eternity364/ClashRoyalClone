
using UnityEngine;
using DG.Tweening;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using System.Collections;
using UnityEngine.Events;

namespace Units{
    public class Base : Unit
    {    
        protected Sequence seq;

        public override void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor, UnityAction onSpawnAnimationFinish) {}

        public override void SetAttackTarget(Unit target) {}

        protected override void PerformAttack() {}
    }
}
