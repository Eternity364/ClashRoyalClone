using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Units {
    public class UnitButtonPanel : MonoBehaviour
    {
        
        [SerializeField]
        Unit[] units = new Unit[4];
        [SerializeField]
        Transform parentForCopy;
        [SerializeField]
        ProgressBar progressBar;
        [SerializeField]
        ClickableArea unitPlaceArea;
        [SerializeField]
        ClickableArea overallArea;
        [SerializeField]
        ClickableArea playerBaseArea;
        [SerializeField]
        ClickableArea panelArea;
        [SerializeField]
        Transform iconPrefab;
        [SerializeField]
        float minCopyScale = 0.5f;

        private UnityAction<Vector3, Unit, bool> OnDragEndEvent;
        private UnityAction<Vector3, Unit> OnEnteredUnitPlaceAreaEvent;
        private UnityAction<Vector3> OnDragEvent;

        private UnitButton[] buttons;
        private UnitButton copyButton;
        private float originalDistance;
        private Vector3 originalScale;
        private Vector3 lastHitPosition;
        private bool enteredUnitPlaceArea = false;



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
            ElixirManager.Instance.AddOnValueChangedListener(UpdateButtonsStatus);
        }

        public void SetOnDragEvents(UnityAction<Vector3, Unit> onEnteredUnitPlaceAreaEvent, UnityAction<Vector3, Unit, bool> onDragEndEvent, UnityAction<Vector3> onDragEvent)
        {
            OnEnteredUnitPlaceAreaEvent = onEnteredUnitPlaceAreaEvent;
            OnDragEvent = onDragEvent;
            OnDragEndEvent = onDragEndEvent;
        }

        public void CreateFieldElixirAnimation()
        {
            Vector3 hitPosition = overallArea.GetMouseHitPosition();
            iconPrefab.localPosition = hitPosition;
        }

        private void UpdateButtonsStatus(float elixirValue)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetCostProgress(elixirValue / units[i].Data.Cost);
                if (i < units.Length)
                {
                    buttons[i].GetComponentInChildren<Button>().interactable = units[i].Data.Cost <= elixirValue;
                }
            }
        }

        private void OnBeginDrag(UnitButton button)
        {
            if (!button.GetComponentInChildren<Button>().interactable)
            {
                return;
            }

            copyButton = Instantiate(button, parentForCopy);

            copyButton.transform.localScale = button.transform.localScale;
            copyButton.SetValue(button.Unit, true);
            Vector3 hitPosition = overallArea.GetMouseHitPosition();
            originalDistance = unitPlaceArea.GetDistanceToArea(hitPosition);
            originalScale = button.transform.localScale;
            unitPlaceArea.SetVisible(true);
            progressBar.ShowCostHighlight(true, button.Unit.Data.Cost);
        }

        private void OnEndDrag(UnitButton button)
        {
            if (copyButton == null)
            {
                return;
            }

            Destroy(copyButton.gameObject);
            copyButton = null;
            originalScale = Vector3.zero;
            originalDistance = float.MaxValue;
            unitPlaceArea.SetVisible(false);

            OnDragEndEvent?.Invoke(lastHitPosition, button.Unit, enteredUnitPlaceArea);

            enteredUnitPlaceArea = false;
            lastHitPosition = Vector3.zero;
            progressBar.ShowCostHighlight(false);
        }

        private void UpdateButtonCopyPosition() {
            if (copyButton != null)
            {
                Vector3 hitPosition = overallArea.GetMouseHitPosition();
                copyButton.transform.position = panelArea.GetMouseHitPosition();

                // We can place unit on board only if we have entered UnitPlaceArea at least once
                if (unitPlaceArea.IsMouseOver())
                {
                    enteredUnitPlaceArea = true;
                    OnEnteredUnitPlaceAreaEvent?.Invoke(hitPosition, copyButton.Unit);
                }
                else if (!unitPlaceArea.IsMousePositionAboveCenter(hitPosition))
                {
                    enteredUnitPlaceArea = false;
                }
                SetCopyParametersDependingOnDistance(hitPosition);
                lastHitPosition = AdjustHitPosition(hitPosition);
                OnDragEvent?.Invoke(lastHitPosition);
            }
        }
        
        private Vector3 AdjustHitPosition(Vector3 hitPosition)
        {
            if (playerBaseArea.GetMouseHitPosition() != Vector3.zero)
            {
                hitPosition = playerBaseArea.GetClosestPositionOutside(hitPosition);
            }

            if (!unitPlaceArea.IsMouseOver())
            {
                hitPosition = unitPlaceArea.GetClosestPoint(hitPosition);
            }

            return hitPosition;
        }
        
        private void SetCopyParametersDependingOnDistance(Vector3 hitPosition)
        {
            if (copyButton != null)
            {
                float alpha = unitPlaceArea.GetDistanceToArea(hitPosition) / originalDistance;
                float t = Mathf.Clamp01(1f - alpha);
                if (unitPlaceArea.IsMousePositionAboveCenter(hitPosition))
                    alpha = 0;
                float scale = Mathf.Lerp(1f, minCopyScale, t);
                copyButton.transform.localScale = originalScale * scale;
                copyButton.SetAlpha(Mathf.Clamp01(alpha));
            }
        }
    }
}
