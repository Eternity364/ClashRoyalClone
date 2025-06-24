using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace Units
{
    public class SpawnParticles : MonoBehaviour
    {
        [SerializeField]
        GameObject lightPart;
        [SerializeField]
        float lightStartTime = 0.1f;
        [SerializeField]
        GameObject debrisPart;
        [SerializeField]
        float debristStartTime = 0.9f;
        [SerializeField]
        GameObject smokePart;
        [SerializeField]
        float smoketStartTime = 0.15f;

        public Dictionary<float, GameObject> Parts
        {
            get
            {
                return new Dictionary<float, GameObject>
                {
                    { lightStartTime, lightPart },
                    { debristStartTime, debrisPart },
                    { smoketStartTime, smokePart }
                };
            }
        }

        public void StartSpawnAnimation(Unit unit, Transform parent, TweenCallback OnSpawnAnimationFinish = null)
        {
            Transform unitTransform = unit.transform;
            float baseOffset = unit.gameObject.GetComponent<NavMeshAgent>().baseOffset;
            Vector3 startPosition = unitTransform.localPosition + new Vector3(0, baseOffset, 0);
            Vector3 originalScale = unitTransform.localScale;
            float yUpOffset = 10f;
            unitTransform.localPosition = new Vector3(startPosition.x, startPosition.y + yUpOffset, startPosition.z);
            Vector3 startScale = new Vector3(originalScale.x, originalScale.y * 1.7f, originalScale.z);
            unitTransform.localScale = startScale;
            unit.SetEmissionStrenght(0.78f);

            float yPos = transform.localPosition.y;
            transform.SetParent(parent);
            transform.localPosition = new Vector3(startPosition.x, 0 + yPos, startPosition.z);

            Sequence spawnAnimation = DOTween.Sequence();
            spawnAnimation.Append(unitTransform.DOLocalMoveY(startPosition.y, 0.45f).SetEase(Ease.InQuad));
            spawnAnimation.Insert(0f, unitTransform.DOScale(startScale - new Vector3(0, startScale.y * 0.7f, 0), 0.4f).SetEase(Ease.InQuad));
            spawnAnimation.Insert(0.4f, unitTransform.DOScale(originalScale, 1f).SetEase(Ease.OutBounce));
            foreach (var part in Parts)
            {
                spawnAnimation.InsertCallback(part.Key, () =>
                {
                    part.Value.SetActive(true);
                    part.Value.GetComponent<ParticleSystem>().Play();
                });
            }
            spawnAnimation.Insert(0.6f,
                DOTween.To(unit.SetEmissionStrenght, 0.78f, 0f, 0.8f).SetEase(Ease.InQuad)
            );
            spawnAnimation.InsertCallback(1.4f, () =>
                {
                    OnSpawnAnimationFinish?.Invoke();
                });
            spawnAnimation.InsertCallback(4f, () =>
                {
                    ObjectPool.Instance.ReturnObject(this.gameObject);
                });
        }

        
        void OnDisable()
        {
            lightPart.SetActive(false);
            debrisPart.SetActive(false);
            smokePart.SetActive(false);
        }
    }
}
