
using UnityEngine;
using DG.Tweening;

namespace Units{
    public class Melee : Unit
    {    
        protected Sequence seq;

        protected override void PerformAttack(TweenCallback OnFinish) {
            base.PerformAttack(OnFinish);

            if (attackTarget != null)
            {
                animator.SetInteger("TriggerNumber", 6);
                animator.SetBool("Trigger", true);

                if (seq != null)
                {
                    seq.Kill();
                }
                seq = DOTween.Sequence();
                seq.InsertCallback(0.5f, () =>
                {
                    attackTarget.ReceiveAttack(data.Attack);
                });
                seq.InsertCallback(1f, OnFinish);
            }
        }
    }
}
