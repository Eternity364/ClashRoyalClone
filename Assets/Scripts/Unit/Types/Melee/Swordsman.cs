
using UnityEngine;
using DG.Tweening;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;

namespace Units{
    public class Swordsman : Melee
    {    
        [SerializeField]
        private GameObject swordInHand;
        [SerializeField]
        private GameObject swordInTheBack;

        public override void SetAttackTarget(Unit target, bool overrideMandatoryFirstAttack = false) {
            bool hadTargetBefore = HasTarget;
            base.SetAttackTarget(target, overrideMandatoryFirstAttack);
            SheathWeapon(false, hadTargetBefore);
        }
        
        protected override void ClearAttackTarget(Unit unit) {
            SheathWeapon(true);
            base.ClearAttackTarget(unit);
        }
        
        private void SheathWeapon(bool active, bool forceStartAttacking = false)
        {
            if (isDead)
                return;
            if (HasTarget != active && forceStartAttacking)
            {
                StartAttacking();
                return;
            }
            
            SwitchWeaponContext context = new SwitchWeaponContext();
            float callbackDelay = active ? 0.6f : 0.1f;
            context.type = active ? "Sheath" : "Unsheath";
            context.side = active ? "None" : "Right";
            context.sheathLocation = "Back";
            context.leftWeapon = (int)Weapon.Relax;
            context.rightWeapon = active ? (int)Weapon.Relax : (int)Weapon.RightSword;
            rPGCharacterController.StartAction("SwitchWeapon", context);

            seq?.Kill();
            seq = DOTween.Sequence();
            seq.InsertCallback(callbackDelay, () =>
            {
                swordInHand.SetActive(!active);
                swordInTheBack.SetActive(active);
                if (active)
                    animator.SetBool("Trigger", true);
            });
            seq.InsertCallback(1, () =>
            {
                if (!active)
                    StartAttacking();
            });
        }
    }
}
