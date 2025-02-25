// Purpose: This script is used to create a pool of bullets that can be used by the player to shoot at the enemies. It creates a pool of 100 bullets and reuses them when the player shoots.
using UnityEngine;

public class BulletFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;

    private GameObject[] bullets = new GameObject[100];
    private int currentBullet = -1;

    void Awake()
    {
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i] = Instantiate(bulletPrefab, transform);
            bullets[i].SetActive(false);
        }
    }

    public GameObject Get()
    {
        if (currentBullet == bullets.Length - 1)
            currentBullet = 0;
        else
            currentBullet++;
        return bullets[currentBullet];
    }
}
