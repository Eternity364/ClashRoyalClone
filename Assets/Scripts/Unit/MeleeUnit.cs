
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using RPGCharacterAnims;
using System;
using RPGCharacterAnims.Actions;

namespace Assets.Scripts.Unit {
    public class MeleeUnit : Unit
    {    
        public override void Init(Transform destination, BulletFactory bulletFactory, int team, Color teamColor) {
            base.Init(destination, bulletFactory, team, teamColor);
        }

        private void UnsheathWeapon() {
            SwitchWeaponContext context = new SwitchWeaponContext();
            context.type = "Unsheath";
            context.side = "Right";
            context.sheathLocation = "Back";
            context.leftWeapon = -1;
            context.rightWeapon = (int)Weapon.RightSword;
            rPGCharacterController.StartAction("SwitchWeapon", context);
        }   

        protected override void PerformAttack() {
            if (attackTarget != null) {
                UnsheathWeapon();
                // animator.SetInteger("TriggerNumber", 6);
                // animator.SetBool("Trigger", true);
                attackTarget.ReceiveAttack(attack);
            }
        }
    }
}
