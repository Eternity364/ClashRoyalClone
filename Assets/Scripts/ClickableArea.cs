using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;

public class ClickableArea : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    [SerializeField]
    private Collider coll;
    [SerializeField]
    private Renderer ren;

    private Material mat;


    private UnityAction<Vector3, int> OnClickEvent;

    private void Awake()
    {
        mat = ren.material;
    }

    public void SetOnClickEvent(UnityAction<Vector3, int> onClickEvent)
    {
        this.OnClickEvent += onClickEvent;
    }
    
    public void SetVisible(bool visible) {
        ren.enabled = visible;
    }

    public float GetDistanceToArea(Vector3 position)
    {
        if (coll == null)
        {
            Debug.LogError("Collider is not set.");
            return float.MaxValue;
        }

        return (position - coll.ClosestPoint(position)).magnitude;
    }

    public Vector3 GetClosestPositionOutside(Vector3 position)
    {
        Vector3 distanceFromCenter = position - coll.bounds.center;
        Vector3 sign = new Vector3(
            Mathf.Sign(distanceFromCenter.x),
            0,
            Mathf.Sign(distanceFromCenter.z)
        );
        Vector3 extents = coll.bounds.extents;
        Vector3 newPosition = coll.bounds.center + new Vector3(
            extents.x * sign.x,
            0,
            extents.z * sign.z
        );
        if (position.x - newPosition.x > position.z - newPosition.z)
        {
            newPosition.x = position.x;
        }
        else
        {
            newPosition.z = position.z;
        }
        return newPosition;
    }

    public Vector3 GetMouseHitPosition()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (coll == null)
        {
            Debug.LogError("Collider is not set.");
            return Vector3.zero;
        }
        if (coll.Raycast(ray, out hit, 100f))
        {
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
        
        if(mat != null)
        {
            mat.SetVector("_ObjectScale", transform.lossyScale);
        }
    }
    

    private void OnClick(int button)
    {
        Vector3 hitPoint = GetMouseHitPosition();
        if (hitPoint != Vector3.zero && OnClickEvent != null)
        {
            OnClickEvent.Invoke(hitPoint, button);
        }
    }
}
