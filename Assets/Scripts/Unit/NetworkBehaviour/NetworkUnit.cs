using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Units
{
    [RequireComponent(typeof(Unit))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NavMeshObstacle))]
    public class NetworkUnit : NetworkBehaviour
    {
        [SerializeField]
        bool spawnAnimation = true;
        [SerializeField]
        Unit unit;

        public NetworkVariable<int> index = new();

        private NetworkVariable<Color> teamColor = new();
        private NetworkVariable<float> emissionStrength = new();
        private NetworkVariable<bool> networkTransformEnabled = new();

        public override void OnNetworkSpawn()
        {
            if (unit == null)
                unit = GetComponent<Unit>();
            //networkTransformEnabled.OnValueChanged += SetNetworkTransformEnabled;
            if (IsOwner)
            {
                unit.OnTeamColorSet += SetTeamColorNetworkVar;
                unit.OnEmissionStrengthSet += SetEmissionStrengthNetworkVar;
            }
            else if (IsClient)
            {
                GetComponent<NetworkTransform>().Interpolate = false;
                teamColor.OnValueChanged += SetTeamColor;
                emissionStrength.OnValueChanged += SetEmissionStrength;
                index.OnValueChanged += RemoveClientCopy;
                unit.enabled = false;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshObstacle>().enabled = false;
                SetTeamColor(teamColor.Value, teamColor.Value);
                SetEmissionStrength(emissionStrength.Value, emissionStrength.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            //networkTransformEnabled.OnValueChanged -= SetNetworkTransformEnabled;
            if (IsOwner)
            {
                unit.OnTeamColorSet -= SetTeamColorNetworkVar;
                unit.OnEmissionStrengthSet -= SetEmissionStrengthNetworkVar;
            }
            else if (IsClient)
            {
                teamColor.OnValueChanged -= SetTeamColor;
                emissionStrength.OnValueChanged -= SetEmissionStrength;
                index.OnValueChanged -= RemoveClientCopy;
            }
        }

        public void StartSpawnAnimation()
        {
            StartSpawnAnimationClientRpc(transform.localPosition);
        }

        [ClientRpc]
        private void StartSpawnAnimationClientRpc(Vector3 position)
        {
            if (IsOwner)
                return;
            
            if (spawnAnimation)
            {  
                UnitSpawner.Instance.StartSpawnAnimation(unit.Spawnable, true, () =>
                {
                    GetComponent<NetworkTransform>().Interpolate = true;
                    //SetNetworkTransformEnabled(false, true);
                });
            }
        }

        public void SetNetworkTransformEnabledNetworkVar(bool enabled)
        {
            networkTransformEnabled.Value = enabled;
        }

        void SetTeamColorNetworkVar(Color color)
        {
            teamColor.Value = color;
        }

        void SetTeamColor(Color _, Color color)
        {
            unit.SetTeamColor(color);
        }

        void RemoveClientCopy(int _, int index)
        {
            UnitSpawner.Instance.RemoveClientCopy(index);
        }

        void SetEmissionStrengthNetworkVar(float value)
        {
            emissionStrength.Value = value;
        }

        void SetEmissionStrength(float _, float value)
        {
            unit.SetEmissionStrength(value);
        }

        void SetNetworkTransformEnabled(bool _, bool enabled)
        {
            GetComponent<NetworkTransform>().enabled = enabled;
        }
    }
}
