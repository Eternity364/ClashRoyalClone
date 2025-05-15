using System.Drawing;
using UnityEngine;
using UnityEngine.Events;

public class ClickableArea : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    [SerializeField]
    private Collider coll; 


    private UnityAction<Vector3, int> OnClickEvent;

    public void SetOnClickEvent(UnityAction<Vector3, int> onClickEvent) {
        this.OnClickEvent += onClickEvent;
    }

    
    public float GetDistanceToArea(Vector3 position) {
        if (coll == null) {
            Debug.LogError("Collider is not set.");
            return float.MaxValue;
        }
        
        return (position - coll.ClosestPoint(position)).magnitude;
    }

    public Vector3 GetMouseHitPosition() {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (coll == null) {
            Debug.LogError("Collider is not set.");
            return Vector3.zero;
        }
        if (coll.Raycast(ray, out hit, 100f)) {
            return hit.point;
        }
        return Vector3.zero;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnClick(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            OnClick(1);
        }
    }
    

    private void OnClick(int button) {
        Vector3 hitPoint = GetMouseHitPosition(); 
        if (hitPoint != Vector3.zero && OnClickEvent != null)
        {
            OnClickEvent.Invoke(hitPoint, button);
        }
    }
}
