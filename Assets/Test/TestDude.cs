using Unity.Netcode;
using UnityEngine;

public class TestDude : NetworkBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        if (!IsOwner)
        {
            return; // Only the owner can control this object
        }
        
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector3 move = new Vector3(h, 0, v).normalized * moveSpeed * Time.deltaTime;
        transform.position += move;
    }
}