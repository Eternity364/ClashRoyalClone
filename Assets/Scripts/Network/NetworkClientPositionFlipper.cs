using UnityEngine;

public class NetworkClientPositionFlipper : MonoBehaviour
{
    [SerializeField]
    private bool isFlipped;
    
    public static NetworkClientPositionFlipper Instance { get; private set; }
    public Vector3 ScaleMiltiplier => isFlipped ? new Vector3(-1, 1, -1) : Vector3.one;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple NetworkClientPositionFlipper instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
}
