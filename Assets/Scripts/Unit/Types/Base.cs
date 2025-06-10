
using UnityEngine;
using DG.Tweening;

namespace Units{
    public class Base : Unit
    {    
        [SerializeField]
        private Color teamColor;
        
        protected Sequence seq;

        public override void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor) { }

        protected override void Awake()
        {
            base.Awake();
            SetTeamColor(teamColor);
        }

        public override void SetAttackTarget(Unit target) { }

        protected override void PerformAttack() {}
    }
}
