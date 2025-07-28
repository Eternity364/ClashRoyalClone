using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    public UnityTransport unityTransport;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    [Command]
    private async void CreateRelay()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            SwitchUnityTransport();

            unityTransport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            Debug.Log("Relay created with ID: " + allocation.AllocationId);
            Debug.Log("Join code: " + joinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to create relay: " + e.Message);
        }
    }

    [Command]
    private async void JoinRelay(string joinCode)
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined relay with ID: " + joinAllocation.AllocationId);

            SwitchUnityTransport();

            unityTransport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to join relay: " + e.Message);
        }
    }

    private void SwitchUnityTransport()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().enabled = false;
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
    }
}
