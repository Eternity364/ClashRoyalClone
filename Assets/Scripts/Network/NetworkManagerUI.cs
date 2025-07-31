using System.Collections.Generic;
using Units;
using Unity.AI.Navigation;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
    private Button relayButton;
    [SerializeField]
    private Button relayBackButton;
    [SerializeField]
    private GameObject panel;
    [SerializeField] List<NavMeshSurface> surfaces;
    [SerializeField] List<UnitAutoSpawner> enemySpawners;
    [SerializeField] Transform mainParent;
    [SerializeField] GameObject relayMenu;
    [SerializeField] GameObject waitingText;

    private UnityTransport unityTransport;

    private void Awake()
    {
        unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        relayButton.onClick.AddListener(() =>
        {
            SetRelayMenuActive(true);
        });
        relayBackButton.onClick.AddListener(() =>
        {
            SetRelayMenuActive(false);
        });
        hostButton.onClick.AddListener(() =>
        {
            SwitchUnityTransport();
            StartHost();
        });
        clientButton.onClick.AddListener(() =>
        {
            SwitchUnityTransport();
            StartClient();
        });
        singlePlayerButton.onClick.AddListener(() =>
        {
            SwitchUnityTransport();
            StartSinglePlayer();
        });
    }

    public void StartHost()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Factory.Instance.CreateElixirManager();
            StartGame();
        };

        NetworkManager.Singleton.StartHost();
        SetButtonsInteractable(false);
    }

    public void StartClient()
    {
        SwitchUnityTransport();
        surfaces.ForEach(surf => surf.enabled = false);
        mainParent.Rotate(NetworkClientPositionFlipper.Instance.Angle);

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            StartGame();
        };

        SetButtonsInteractable(false);
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

    private void SetRelayMenuActive(bool active)
    {
        SetButtonsInteractable(!active);
        relayMenu.SetActive(active);
    }

    private void StartGame()
    {
        panel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        relayButton.interactable = interactable;
        hostButton.interactable = interactable;
        clientButton.interactable = interactable;
        singlePlayerButton.interactable = interactable;
    }

    public void SetWaitingMode(bool waiting)
    {
        waitingText.SetActive(waiting);
        SetButtonsInteractable(!waiting);
        if (waiting)
            relayMenu.SetActive(false);
    }
    
    private void SwitchUnityTransport()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().enabled = false;
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
    }
}
