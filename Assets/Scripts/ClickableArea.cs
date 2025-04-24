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

    private UnityAction<Vector3>[] OnClick = new UnityAction<Vector3>[2]; 

    public void SetOnClickEvent(UnityAction<Vector3> onClickEvent, int button) {
        if (button != 0 && button != 1) return;
        OnClick[button] = onClickEvent;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (coll.Raycast(ray, out hit, 100f) )
            {
                OnLeftClick?.Invoke(hit.point);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (coll.Raycast(ray, out hit, 100f) )
            {
                OnRightClick?.Invoke(hit.point);
            }
        }
    }

    private void OnClick(int button) {
        if (Input.GetMouseButtonDown(button))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (coll.Raycast(ray, out hit, 100f) )
            {
                OnRightClick?.Invoke(hit.point);
            }
        } 
    }
}
