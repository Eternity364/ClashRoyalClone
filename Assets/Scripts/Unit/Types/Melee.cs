
using UnityEngine;
using DG.Tweening;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using System.Collections;
using UnityEngine.Events;

namespace Units{
    public class Melee : Unit
    {    
        protected Sequence seq;

        protected override void PerformAttack(TweenCallback OnFinish) {
            base.PerformAttack(OnFinish);

            if (attackTarget != null)
            {
                Debug.Log("Performing melee attack on " + attackTarget.name);
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
