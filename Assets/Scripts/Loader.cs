using System.Collections.Generic;
using Units;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Mode
{
    SinglePlayer,
    Host,
    Client
}

public class Loader : MonoBehaviour
{
    [SerializeField] Mode mode;
    [SerializeField] List<NavMeshSurface> surfaces;
    [SerializeField] List<UnitAutoSpawner> enemySpawners;
    [SerializeField] TargetAcquiring targetAcquiring;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        new SinglePlayerControlScheme();
        switch (this.mode)
        {
            case Mode.SinglePlayer:
                StartSinglePlayer();
                break;
            case Mode.Host:
                StartHost();
                break;
            case Mode.Client:
                StartClient();
                break;
        }
    }

    void StartSinglePlayer()
    {
        new SinglePlayerControlScheme();
    }

    void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    void StartClient()
    {
        surfaces.ForEach(surf => surf.enabled = false);
        enemySpawners.ForEach(spawner => spawner.enabled = false);
        targetAcquiring.enabled = false;
        NetworkManager.Singleton.StartClient();
    }
}
