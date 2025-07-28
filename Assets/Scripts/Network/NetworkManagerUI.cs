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
        //PreStartGame();
        
        hostButton.onClick.AddListener(() => StartHost());
        clientButton.onClick.AddListener(() => StartClient());
        singlePlayerButton.onClick.AddListener(() => StartSinglePlayer());
    }

    private void StartHost()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Factory.Instance.CreateElixirManager();
            StartGame();
        };
        
        NetworkManager.Singleton.StartHost();
        TurnOffButtons();
    }

    private void StartClient()
    {
        surfaces.ForEach(surf => surf.enabled = false);
        mainParent.Rotate(NetworkClientPositionFlipper.Instance.Angle);

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            StartGame();
        };

        TurnOffButtons();
        NetworkManager.Singleton.StartClient();
    }

    private void StartSinglePlayer()
    {
        
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            enemySpawners.ForEach(spawner => spawner.gameObject.SetActive(true));
        };
        StartHost();
    }

    // private void PreStartGame()
    // {
    //     panel.GetComponent<UnitButtonPanel>().Initialize();
    // }

    private void StartGame()
    {
        panel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void TurnOffButtons()
    {
        hostButton.interactable = false;
        clientButton.interactable = false;
        singlePlayerButton.interactable = false;
    }
}
