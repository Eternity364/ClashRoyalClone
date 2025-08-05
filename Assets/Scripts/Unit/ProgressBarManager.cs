using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Units
{
    public class ProgressBarManager : MonoBehaviour
    {
        [SerializeField]
        private ProgressBar progressBarPrefab;
        [SerializeField]
        private Transform parent;
        [SerializeField]
        ClickableArea clickableArea;
        [SerializeField]
        float progressBarYOffset = 20f;
        [SerializeField]
        Vector2 progressBarScaleMedium = new Vector2(0.1f, 2f);
        [SerializeField]
        float progressBarOffsetMedium;
        [SerializeField]
        Vector2 progressBarScaleBig = new Vector2(0.2f, 2f);
        [SerializeField]
        float progressBarOffsetBig;
        [SerializeField]
        float positionAdjustmentStrength = 0.1f;

        private Dictionary<Unit, ProgressBar> activeProgressBars = new();


        public void CreateProgressBar(Unit unit)
        {
            if (unit.Data.Size == Size.Small)
                return;

            ProgressBar progressBar = ObjectPool.Instance.GetObject(progressBarPrefab.gameObject).GetComponent<ProgressBar>();
            progressBar.transform.SetParent(parent);
            progressBar.transform.localScale = GetProgressBarScale(unit);
            progressBar.transform.localRotation = Quaternion.identity;
            activeProgressBars.Add(unit, progressBar);
        }

        // public void UpdateProgressBar(GameObject progressBar, float progress)
        // {
        //     if (activeProgressBars.Contains(progressBar))
        //     {
        //         // Assuming the progress bar has a method to update its fill amount
        //         ProgressBar barComponent = progressBar.GetComponent<ProgressBar>();
        //         if (barComponent != null)
        //         {
        //             barComponent.SetFillAmount(progress);
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Attempted to update a progress bar that is not managed by this manager.");
        //     }
        // }
        

        public void RemoveProgressBar(Unit unit)
        {
            if (activeProgressBars.TryGetValue(unit, out ProgressBar progressBar))
            {
                ObjectPool.Instance.ReturnObject(progressBar.gameObject);
                activeProgressBars.Remove(unit);
            }
            else
            {
                Debug.LogWarning("Attempted to remove a progress bar that does not exist for the unit.");
            }
        }

        private Vector3 GetProgressBarScale(Unit unit)
        {
            if (unit.Data.Size == Size.Medium)
            {
                return new Vector3(progressBarScaleMedium.x, progressBarScaleMedium.y, 1f);
            }
            else if (unit.Data.Size == Size.Big)
            {
                return new Vector3(progressBarScaleBig.x, progressBarScaleBig.y, 1f);
            }
            return Vector3.one;
        }

        private float GetProgressBarOffset(Unit unit)
        {
            if (unit.Data.Size == Size.Medium)
            {
                return progressBarOffsetMedium;
            }
            else if (unit.Data.Size == Size.Big)
            {
                return progressBarOffsetBig;
            }
            return 0f;
        }

        private Vector3 GetProgressBarPosition(Unit unit)
        {
            Vector3 worldPos = unit.transform.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 screenOffsetFromCenter = (Vector2)screenPos - screenCenter;
            screenPos += (Vector3)screenOffsetFromCenter * positionAdjustmentStrength;

            return screenPos + GetProgressBarOffset(unit) * Vector3.up;

            // if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //         (RectTransform)parent, screenPos, Camera.main, out Vector2 localPos))
            // {
            //     return localPos += GetProgressBarOffset(unit) * Vector2.up;
            // }
            // else
            // {
            //     return unit.transform.position + Vector3.up * 2f;
            // }
        }

        private void UpdatePositions()
        {
            foreach (var kvp in activeProgressBars)
            {
                Unit unit = kvp.Key;
                ProgressBar progressBar = kvp.Value;
                
                progressBar.transform.position = GetProgressBarPosition(unit);
            }
        }

        private void Update()
        {
            UpdatePositions();
        }
    }
}
