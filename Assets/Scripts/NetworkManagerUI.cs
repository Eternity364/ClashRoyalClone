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
    [SerializeField] TargetAcquiring targetAcquiring;

    private void Awake()
    {
        new SinglePlayerControlScheme();
        hostButton.onClick.AddListener(() => StartHost());
        clientButton.onClick.AddListener(() => StartClient());
        singlePlayerButton.onClick.AddListener(() => StartSinglePlayer());
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        targetAcquiring.gameObject.SetActive(true);
        StartGame();
    }

    private void StartClient()
    {
        surfaces.ForEach(surf => surf.enabled = false);
        NetworkManager.Singleton.StartClient();
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
