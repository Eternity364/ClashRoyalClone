using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Units{
    public class UnitButton : MonoBehaviour
    {
        [SerializeField]
        private UnitButtonReferances unitButtonReferances;
        [SerializeField]
        private EventTrigger eventTrigger;
        [SerializeField]
        private Image image;

        public Unit Unit => unit;

        private Unit unit;

        public void SetValue(Unit unit)
        {
            this.unit = unit;
            if (unitButtonReferances.Data.TryGetValue(unit, out Texture texture))
            {
                Image image = GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            else
            {
                Debug.LogError("Unit not found in UnitButtonReferences.");
            }
        }
        
        public void SetAlpha(float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        
        public void SetBeginDrag(Action OnBeginDrag)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.BeginDrag;
            entry.callback.AddListener((eventData) =>
            {
                OnBeginDrag();
                this.gameObject.GetComponent<Button>().interactable = false;
            });
            eventTrigger.triggers.Add(entry);
        }

        public void SetEndDrag(Action OnEndDrag) {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener( (eventData) => {
                OnEndDrag();
                this.gameObject.GetComponent<Button>().interactable = true;
            } );
            eventTrigger.triggers.Add(entry);
        }

        public void SetOnDrag(Action OnDrag) {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener( (eventData) => { OnDrag(); } );
            eventTrigger.triggers.Add(entry);
        }
    }
}
