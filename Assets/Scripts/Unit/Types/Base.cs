
using UnityEngine;
using DG.Tweening;

namespace Units{
    public class Base : Unit
    {    
        [SerializeField]
        private Color teamColor;
        
        protected Sequence seq;

        public override void Init(Transform destination, int team) { }

        protected override void Awake()
        {
            base.Awake();
            SetTeamColor(teamColor);
        }

        public override void SetAttackTarget(Unit target, bool overrideMandatoryFirstAttack = false) { }

        protected override void PerformAttack(TweenCallback OnFinish) {}
    }
}
