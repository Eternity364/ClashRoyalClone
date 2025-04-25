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

    private UnityAction<Vector3, int> OnClickEvent;

    public void SetOnClickEvent(UnityAction<Vector3, int> onClickEvent) {
        this.OnClickEvent += onClickEvent;
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
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (coll.Raycast(ray, out hit, 100f) )
        {
            OnClickEvent.Invoke(hit.point, button);
        }
    }
}
