using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Units
{
    [RequireComponent(typeof(Unit))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NavMeshObstacle))]
    public class NetworkUnit : NetworkBehaviour
    {
        private NetworkVariable<Color> teamColor = new();
        private NetworkVariable<float> emissionStrength = new();
        private Unit unit;

        public override void OnNetworkSpawn()
        {
            unit = GetComponent<Unit>();
            if (IsOwner)
            {
                unit.OnTeamColorSet += SetTeamColorNetworkVar;
                unit.OnEmissionStrengthSet += SetEmissionStrengthNetworkVar;
            }
            else if (IsClient)
            {
                teamColor.OnValueChanged += SetTeamColor;
                emissionStrength.OnValueChanged += SetEmissionStrength;
                unit.enabled = false;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshObstacle>().enabled = false;
            }
            SetTeamColor(teamColor.Value, teamColor.Value);
            SetEmissionStrength(emissionStrength.Value, emissionStrength.Value);
            UnitSpawner.Instance.StartSpawnAnimation(unit.Spawnable, true, null);
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                unit.OnTeamColorSet -= SetTeamColorNetworkVar;
                unit.OnEmissionStrengthSet -= SetEmissionStrengthNetworkVar;
            }
            else if (IsClient)
            {
                teamColor.OnValueChanged -= SetTeamColor;
                emissionStrength.OnValueChanged -= SetEmissionStrength;
            }
        }

        void SetTeamColorNetworkVar(Color color)
        {
            teamColor.Value = color;
        }

        void SetTeamColor(Color _, Color color)
        {
            unit.SetTeamColor(color);
        }

        void SetEmissionStrengthNetworkVar(float value)
        {
            emissionStrength.Value = value;
        }

        void SetEmissionStrength(float _, float value)
        {
            unit.SetEmissionStrength(value);
        }
    }
}
