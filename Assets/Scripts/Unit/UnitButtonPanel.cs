using DG.Tweening;
using TMPro;
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
        // <summary>
        /// Area where we can place units
        /// </summary>
        [SerializeField]
        ClickableArea unitPlaceArea;
        // <summary>
        /// Whole area of the game field
        /// </summary>
        [SerializeField]
        ClickableArea overallArea;
        // <summary>
        /// Area which player base occupies
        /// This area is used to prevent placing units on the player base
        /// </summary>
        [SerializeField]
        ClickableArea playerBaseArea;
        // <summary>
        /// Whole area of the game UI plane
        /// </summary>
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

        public void CreateFieldElixirAnimation(float value)
        {
            GameObject icon = ObjectPool.Instance.GetObject(iconPrefab.gameObject);
            Vector3 originalScale = icon.transform.localScale;
            icon.transform.SetParent(parentForCopy);
            icon.transform.localRotation = Quaternion.identity;
            Vector3 hitPosition = panelArea.GetMouseHitPosition();
            icon.transform.position = hitPosition;
            icon.transform.localScale = new Vector3(originalScale.x * 0.5f, originalScale.y * 0.15f, originalScale.z);
            var image = icon.GetComponent<Image>();
            image.material = new Material(image.material);
            image.material.color = new Color(image.material.color.r, image.material.color.g, image.material.color.b, 0);
            TextMeshProUGUI costText = icon.GetComponentInChildren<TextMeshProUGUI>();
            costText.text = "-" + value.ToString();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(icon.transform.DOLocalMoveY(icon.transform.localPosition.y + 800f, 1f).SetEase(Ease.OutCirc));
            sequence.Insert(0, icon.transform.DOScale(originalScale, 1.5f).SetEase(Ease.OutElastic));

            sequence.Insert(0, image.material.DOFade(1, 0.4f).SetEase(Ease.OutCubic));
            sequence.Insert(1.4f, image.material.DOFade(0, 0.4f).SetEase(Ease.InCubic));
            sequence.Insert(1.501f, icon.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InCirc));
            sequence.OnComplete(() => {
                icon.transform.localScale = originalScale;
                ObjectPool.Instance.ReturnObject(icon);
            });
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
