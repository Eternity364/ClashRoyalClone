using UnityEngine;
using UnityEngine.UI;

namespace Units{
    public class UnitButton : MonoBehaviour
    {
        [SerializeField]
        private UnitButtonReferances unitButtonReferances;
        [SerializeField]
        private Button button;

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
    }
}
