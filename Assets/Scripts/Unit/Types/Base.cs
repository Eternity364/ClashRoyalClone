
using UnityEngine;
using DG.Tweening;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using System.Collections;

namespace Assets.Scripts.Unit {
    public class Base : Unit
    {    
        protected Sequence seq;

        public override void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor) {}

        public override void SetAttackTarget(Unit target) {}

        protected override void PerformAttack() {}
    }
}
