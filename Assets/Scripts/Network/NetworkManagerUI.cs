using System.Collections.Generic;
using Units;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField]
    private Button singlePlayerButton;
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button clientButton;
    [SerializeField]
    private GameObject panel;
    [SerializeField] List<NavMeshSurface> surfaces;
    [SerializeField] List<UnitAutoSpawner> enemySpawners;
    [SerializeField] Transform mainParent;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => StartHost());
        clientButton.onClick.AddListener(() => StartClient());
        singlePlayerButton.onClick.AddListener(() => StartSinglePlayer());
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        StartGame();
    }

    private void StartClient()
    {
        surfaces.ForEach(surf => surf.enabled = false);
        NetworkManager.Singleton.StartClient();
        mainParent.Rotate(NetworkClientPositionFlipper.Instance.Angle);
        StartGame();
    }

    private void StartSinglePlayer()
    {
        enemySpawners.ForEach(spawner => spawner.gameObject.SetActive(true));
        StartHost();
    }

    private void StartGame()
    {
        panel.SetActive(true);
        gameObject.SetActive(false);
    }
}
