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
        private NetworkVariable<int> hp = new();
        private NetworkVariable<Color> damageColor = new();
        private NetworkVariable<int> team = new();

        private ProgressBar progressBar;

        public override void OnNetworkSpawn()
        {
            if (unit == null)
                unit = GetComponent<Unit>();

            if (IsOwner)
            {
                unit.OnTeamColorSet += SetTeamColorNetworkVar;
                unit.OnEmissionStrengthSet += SetEmissionStrengthNetworkVar;
                unit.OnDamageColorSet += SetDamageColorNetworkVar;
                unit.OnHealthChanged += SetHPNetworkVar;
            }
            else if (IsClient)
            {
                teamColor.OnValueChanged += SetTeamColor;
                emissionStrength.OnValueChanged += SetEmissionStrength;
                damageColor.OnValueChanged += SetDamageColor;
                index.OnValueChanged += RemoveClientCopy;
                hp.OnValueChanged += SetProgressBarValue;

                unit.enabled = false;
                GetComponent<NetworkTransform>().Interpolate = false;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<NavMeshObstacle>().enabled = false;
                SetTeamColor(teamColor.Value, teamColor.Value);
                SetEmissionStrength(emissionStrength.Value, emissionStrength.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                unit.OnTeamColorSet -= SetTeamColorNetworkVar;
                unit.OnEmissionStrengthSet -= SetEmissionStrengthNetworkVar;
                unit.OnDamageColorSet -= SetDamageColorNetworkVar;
                unit.OnHealthChanged -= SetHPNetworkVar;
            }
            else if (IsClient)
            {
                teamColor.OnValueChanged -= SetTeamColor;
                emissionStrength.OnValueChanged -= SetEmissionStrength;
                index.OnValueChanged -= RemoveClientCopy;
                damageColor.OnValueChanged -= SetDamageColor;
                hp.OnValueChanged -= SetProgressBarValue;
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
                UnitSpawner.Instance.StartParticlesOnlySpawnAnimation(position, transform.parent, unit.Data.Size, () =>
                {
                    GetComponent<NetworkTransform>().Interpolate = true;
                    progressBar.gameObject.SetActive(true);
                });
            }
        }

        void SetTeamColor(Color _, Color color)
        {
            unit.SetTeamColor(color);
            progressBar?.ChangeColors(progressBar.backgroundColor, color);
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

        void SetDamageColorNetworkVar(Color color)
        {
            damageColor.Value = color;
        }

        void SetDamageColor(Color _, Color color)
        {
            unit.SetDamageColor(color);
        }

        void SetProgressBarValue(int _, int hp)
        {
            float fillAmount = (float)hp / (float)unit.Data.MaxHealth * 100f;
            progressBar?.SetFillAmount(fillAmount);
            if (hp <= 0)
            {
                ProgressBarManager.Instance.RemoveProgressBar(unit);
            }
        }

        void SetHPNetworkVar(Unit unit)
        {
            this.hp.Value = unit.Health;
        }

        void SetTeamNetworkVar(int team)
        {
            this.team.Value = team;
        }

        void CreateProgressBar(int _, int team)
        {
            unit.Team = team; 
            progressBar = ProgressBarManager.Instance.CreateProgressBar(unit);
            progressBar.gameObject.SetActive(unit is Base);
        }
    }
}
