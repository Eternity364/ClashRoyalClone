using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickableArea : MonoBehaviour
{
    private RaycastHit hit;
    private Ray ray;
    [SerializeField]
    private Collider coll; 

    private UnityAction<Vector3> OnClick; 

    public void Init(UnityAction<Vector3> OnClick) {
        this.OnClick = OnClick;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (coll.Raycast(ray, out hit, 100f))
            {
                print(hit.point);
                OnClick?.Invoke(hit.point);
            }
        }
    }
}
