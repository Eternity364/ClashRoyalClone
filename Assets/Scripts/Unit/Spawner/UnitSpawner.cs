using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


namespace Units{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField]
        UnitButtonPanel panel;
        [SerializeField]
        TargetAcquiring targetAcquiring;
        [SerializeField]
        Transform target;
        [SerializeField]
        GameObject spawnLightParticlesPrefab;
        [SerializeField]
        GameObject spawnDebrisParticlesPrefab;
        [SerializeField]
        Transform parent;
        [SerializeField]
        BulletFactory bulletFactory;
        [SerializeField]
        Color teamColor;
        [SerializeField, Range(0, 1)]
        int team;

        private GameObject unitCopy;
        private const float delayBeforeSpawn = 1f;

        void Start()
        {
            panel.SetOnDragEvents(CreateUnitCopy, TrySpawn, UpdateUnitCopyPosition);
        }

        public void TrySpawn(Vector3 position, Unit unitType, bool spawn)
        {
            StartCoroutine(TrySpawnCor(position, unitType, spawn));
        }

        private IEnumerator TrySpawnCor(Vector3 position, Unit unitType, bool spawn)
        {
            ElixirManager.Instance.ChangeValue(-unitType.Data.Cost);
            GameObject oldUnitCopy = unitCopy;
            unitCopy = null;

            if (!spawn)
            {
                ObjectPool.Instance.ReturnObject(oldUnitCopy);
                yield break;
            }

            yield return new WaitForSeconds(delayBeforeSpawn);

            if (spawn)
            {
                Unit unit = CreateUnit(position, unitType).GetComponent<Unit>();
                StartSpawnAnimation(unit, () =>
                {
                    unit.Init(target, bulletFactory, team, teamColor);
                    targetAcquiring.AddUnit(unit);
                });
                ObjectPool.Instance.ReturnObject(oldUnitCopy);
            }
        }

        private void UpdateUnitCopyPosition(Vector3 position)
        {
            if (unitCopy != null)
            {
                position.y = 0;
                unitCopy.transform.localPosition = position;
            }
        }

        private void CreateUnitCopy(Vector3 position, Unit unitType)
        {
            if (unitCopy != null)
            {
                return;
            }
            unitCopy = CreateUnit(position, unitType, true);
            Unit unit = unitCopy.GetComponent<Unit>();
            unit.SetAlpha(0.5f);
        }

        private GameObject CreateUnit(Vector3 position, Unit unitType, bool isCopy = false)
        {
            GameObject prefab = unitType.gameObject;
            if (isCopy)
            {
                prefab = UnitsList.Instance.GetTransparent(unitType);
            }
            GameObject unit = ObjectPool.Instance.GetObject(prefab);
            unit.transform.SetParent(parent);
            unit.gameObject.SetActive(true);
            position.y = 0;
            unit.transform.localPosition = position;
            unit.GetComponent<Unit>().SetTeamColor(teamColor);

            return unit;
        }

        // Move it to animation helpers
        private void StartSpawnAnimation(Unit unit, TweenCallback OnSpawnAnimationFinish = null)
        {
            Transform unitTransform = unit.transform;
            Vector3 startPosition = unitTransform.localPosition;
            Vector3 originalScale = unitTransform.localScale;
            float yUpOffset = 10f;
            unitTransform.localPosition = new Vector3(startPosition.x, startPosition.y + yUpOffset, startPosition.z);
            Vector3 startScale = new Vector3(originalScale.x, originalScale.y * 1.7f, originalScale.z);
            unitTransform.localScale = startScale;
            unit.SetEmissionStrenght(0.78f);

            GameObject spawnLightParticles = ObjectPool.Instance.GetObject(this.spawnLightParticlesPrefab, false);
            float yPos = spawnLightParticles.transform.localPosition.y;
            spawnLightParticles.transform.SetParent(parent);
            spawnLightParticles.transform.localPosition = new Vector3(startPosition.x, 0 + yPos, startPosition.z);

            GameObject spawnDebrisParticles = ObjectPool.Instance.GetObject(this.spawnDebrisParticlesPrefab, false);
            yPos = spawnDebrisParticles.transform.localPosition.y;
            spawnDebrisParticles.transform.SetParent(parent);
            spawnDebrisParticles.transform.localPosition = new Vector3(startPosition.x, 0 + yPos, startPosition.z);

            Sequence spawnAnimation = DOTween.Sequence();
            spawnAnimation.Append(unitTransform.DOLocalMoveY(startPosition.y, 0.45f).SetEase(Ease.InQuad));
            spawnAnimation.Insert(0f, unitTransform.DOScale(startScale - new Vector3(0, startScale.y * 0.7f, 0), 0.4f).SetEase(Ease.InQuad));
            spawnAnimation.Insert(0.4f, unitTransform.DOScale(originalScale, 1f).SetEase(Ease.OutBounce));
            spawnAnimation.InsertCallback(0.1f, () =>
                {
                    spawnLightParticles.SetActive(true);
                });
            spawnAnimation.InsertCallback(0.6f, () =>
                {
                    spawnDebrisParticles.SetActive(true);
                });
            spawnAnimation.Insert(0.6f,
                DOTween.To(unit.SetEmissionStrenght, 0.78f, 0f, 0.8f).SetEase(Ease.InQuad)
            );
            spawnAnimation.InsertCallback(1.4f, () =>
                {
                    OnSpawnAnimationFinish?.Invoke();
                    //ObjectPool.Instance.ReturnObject(spawnLightParticles);
                });
        }
    }
}
