using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units {
    public class UnitButtonPanel : MonoBehaviour
    {
        
        [SerializeField]
        Unit[] units = new Unit[4];
        [SerializeField]
        Transform parentForCopy;
        [SerializeField]
        ClickableArea unitPlaceArea;
        [SerializeField]
        ClickableArea overallArea;
        [SerializeField]
        float minCopyScale = 0.5f;

        private UnitButton[] buttons;
        private UnitButton copyButton;
        private float originalDistance;
        private Vector3 originalScale;

        
        void Start()
        {
            buttons = GetComponentsInChildren<UnitButton>();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < units.Length)
                {
                    UnitButton button = buttons[i];
                    buttons[i].SetValue(units[i]);
                    buttons[i].SetBeginDrag(() => OnBeginDrag(button));
                    buttons[i].SetEndDrag(() => OnEndDrag(button));
                    buttons[i].SetOnDrag(UpdateButtonCopyPosition);
                }
            }
        }

        

        private void OnBeginDrag(UnitButton button)
        {
            copyButton = Instantiate(button, parentForCopy);
            button.transform.GetLocalPositionAndRotation(out var localPos, out var localRot);
            copyButton.transform.SetLocalPositionAndRotation(localPos, localRot);
            copyButton.transform.localScale = button.transform.localScale;
            copyButton.SetValue(button.Unit);
            Vector3 hitPosition = overallArea.GetMouseHitPosition();
            originalDistance = unitPlaceArea.GetDistanceToArea(hitPosition);
            originalScale = button.transform.localScale;
        }

        private void OnEndDrag(UnitButton button)
        {
            Destroy(copyButton.gameObject);
            copyButton = null;
            originalScale = Vector3.zero;
            originalDistance = float.MaxValue;
        }

        private void UpdateButtonCopyPosition() {
            if (copyButton != null)
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = 0;
                copyButton.transform.position = mousePosition;
                Vector3 hitPosition = overallArea.GetMouseHitPosition();
                SetCopyParametersDependingOnDistance(hitPosition);
            }
        }
        
        private void SetCopyParametersDependingOnDistance(Vector3 hitPosition)
        {
            if (copyButton != null)
            {
                float t = Mathf.Clamp01(1f - (unitPlaceArea.GetDistanceToArea(hitPosition) / originalDistance));
                float scale = Mathf.Lerp(1f, minCopyScale, t);
                copyButton.transform.localScale = originalScale * scale;
                copyButton.SetAlpha(Mathf.Clamp01(unitPlaceArea.GetDistanceToArea(hitPosition) / originalDistance));
            }
        }
    }
}
