using Unity.Netcode;
using UnityEngine;

namespace Units
{
    public class NetworkSwordsman : NetworkedUnit
    {
        private NetworkVariable<bool> swordActive = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (NetworkManager.Singleton.IsHost)
                gameObject.GetComponent<Swordsman>().OnSetSwordActive += SetSwordActiveNetworkVar;
            else if (NetworkManager.Singleton.IsClient)
                swordActive.OnValueChanged += SetSwordActive;

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (NetworkManager.Singleton.IsHost)
                gameObject.GetComponent<Swordsman>().OnSetSwordActive -= SetSwordActiveNetworkVar;
            else if (NetworkManager.Singleton.IsClient)
                swordActive.OnValueChanged -= SetSwordActive;
        }

        private void SetSwordActiveNetworkVar(bool active)
        {
            swordActive.Value = active;
        }

        private void SetSwordActive(bool _, bool active)
        {
            gameObject.GetComponent<Swordsman>().SetSwordActive(active);
        }
    }
}
