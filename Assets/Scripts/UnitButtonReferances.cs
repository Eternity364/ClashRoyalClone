using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Unit {
    public class UnitButtonReferanceData {
        public Unit unit;
        public Texture texture;
    }

    [CreateAssetMenu(fileName = "UnitButtonReferences", menuName = "ScriptableObjects/UnitButtonReferences", order = 1)]
    public class UnitButtonReferances : ScriptableObject
    {
        [SerializeField]
        private List<Unit> unitTypes = new();
        [SerializeField]
        private List<Texture> textures = new();

        private Dictionary<Unit, Texture> dic = new();

        public Dictionary<Unit, Texture> Data {
            get {
                return dic;
            }
        }

        private void OnEnable()
        {
            dic.Clear();
            for (int i = 0; i < unitTypes.Count && i < textures.Count; i++)
            {
                dic[unitTypes[i]] = textures[i];
            }
        }
    }
}
