using UnityEngine;


namespace Assets.Scripts.Unit {
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField]
        Unit unitPrefab;
        [SerializeField]
        ClickableArea area;
        [SerializeField]
        TargetAcquiring targetAcquiring;
        [SerializeField]
        Transform target;
        [SerializeField]
        Transform parent;
        [SerializeField]
        BulletFactory bulletFactory;
        [SerializeField]
        Color teamColor;
        [SerializeField, Range(0, 1)]
        int team;
        
        void Start() {
            area.Init(Spawn);
        }

        public void Spawn(Vector3 position)
        {
            Unit unit = ObjectPool.Instance.GetObject(unitPrefab.gameObject).GetComponent<Unit>();
            unit.transform.SetParent(parent);
            unit.gameObject.SetActive(true);
            position.y = 0;
            unit.transform.localPosition = position;
            unit.Init(target, bulletFactory, team, teamColor);
            targetAcquiring.AddUnit(unit);
        }
    }
}
