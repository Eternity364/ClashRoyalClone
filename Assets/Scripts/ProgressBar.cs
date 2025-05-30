using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private Transform fillTransform;
    [SerializeField]
    private Transform parentTransform;
    [SerializeField]
    private Image fillImage;
    [SerializeField]
    private GameObject segmentPrefab;
    [SerializeField]
    private GameObject divisorPrefab;
    [SerializeField]
    private int segmentsCount = 10;
    [Range(0, 1)]
    [SerializeField]
    private float segmentBrightness1 = 0.4f;
    [Range(0, 1)]
    [SerializeField]
    private float segmentBrightness2 = 0.8f;
    [Range(0, 100)]
    [SerializeField]
    private float fillAmount = 0.0f;

    private float imageWidth;
    private List<GameObject> segments = new List<GameObject>();
    private List<GameObject> divisors = new List<GameObject>();

    void Awake()
    {
        imageWidth = fillImage.rectTransform.sizeDelta.x;
        CreateSegments();
        SetFillAmount(fillAmount);
    }

    public void SetFillAmount(float value)
    {
        if (value < 0)
            value = 0;
        else if (value > 100)
            value = 100;
        fillAmount = value;
        fillTransform.localPosition = new Vector3(-(imageWidth * (100 - fillAmount)) / 100f, fillTransform.localPosition.y, fillTransform.localPosition.z);
        SetSegments();
    }

    private void SetSegments()
    {
        for (int i = 0; i < segmentsCount; i++)
        {
            float value = (i + 1) * 100f / segmentsCount;
            bool isActive = value <= fillAmount;
            if (!segments[i].activeSelf && isActive)
                StartSegmentAppearAnimation(segments[i], i);
            else if (segments[i].activeSelf && !isActive)
                StartSegmentDisappearAnimation(segments[i]);
            if (i < segmentsCount - 1)
            {
                divisors[i].SetActive(isActive);
            }
        }
    }

    private void StartSegmentAppearAnimation(GameObject segment, int index)
    {
        float x = imageWidth * (1f / segmentsCount * index - 1f / 2f + 1f / segmentsCount / 2f);
        float segmentWidth = imageWidth / segmentsCount;
        Vector3 targetPosition = new Vector3(x, segment.transform.localPosition.y, segment.transform.localPosition.z);
        segment.transform.localPosition = targetPosition - new Vector3(segmentWidth, 0, 0);
        segment.transform.DOLocalMoveX(x, 0.6f).SetEase(Ease.OutBounce);

        Material segmentMaterial = segment.GetComponent<Image>().material;
        Color color = segmentMaterial.color;
        color.a = 1f;
        segmentMaterial.color = color;
        void UpdateSegmentMaterial(float brightness)
        {
            segmentMaterial.SetFloat("_EmissionStrength", brightness);
        }
        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Append(
            DOTween.To(UpdateSegmentMaterial, 0f, segmentBrightness1, 0.2f).SetEase(Ease.OutQuint)
        );
        seq.Append(
            DOTween.To(UpdateSegmentMaterial, segmentBrightness1, 0f, 1f).SetEase(Ease.InQuad)
        );
    }

    private void StartSegmentDisappearAnimation(GameObject segment)
    {
        Material segmentMaterial = segment.GetComponent<Image>().material;
        void UpdateSegmentMaterial(float brightness)
        {
            segmentMaterial.SetFloat("_EmissionStrength", brightness);
        }
        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Append(
            DOTween.To(UpdateSegmentMaterial, segmentBrightness1, segmentBrightness2, 0.3f).SetEase(Ease.InBack)
        );
        seq.Append(
            DOTween.To(UpdateSegmentMaterial, segmentBrightness2, 0f, 0.3f).SetEase(Ease.OutBack)
        );
        seq.Insert(0.3f,
            segmentMaterial.DOFade(0f, 0.3f).SetEase(Ease.OutBack)
        );
    }

    private void CreateSegments()
    {
        // Need to start creating segments from the end, because of Canvas rendering order in Unity.
        for (int i = segmentsCount - 1; i >= 0; i--)
        {
            GameObject segment = Instantiate(segmentPrefab, parentTransform);
            Image image = segment.GetComponent<Image>();
            image.material = new Material(image.material);
            Vector3 scale = segment.transform.localScale;
            Vector3 position = segment.transform.localPosition;
            position.x = imageWidth * (1f / segmentsCount * i - 1f / 2f + 1f / segmentsCount / 2f);
            scale.x = 1f / segmentsCount;
            segment.transform.localScale = scale;
            segment.transform.localPosition = position;
            Material segmentMaterial = segment.GetComponent<Image>().material;
            Color color = segmentMaterial.color;
            color.a = 0f;
            segmentMaterial.color = color;
            segment.SetActive(true);
            segments.Insert(0, segment);
        }

        for (int i = 0; i < segmentsCount; i++)
        {
            if (i < segmentsCount - 1)
            {
                GameObject divisor = Instantiate(divisorPrefab, parentTransform);
                divisor.transform.localPosition =
                    segments[i].transform.localPosition +
                    new Vector3(0.5f / segmentsCount * imageWidth, 0, 0);
                divisor.SetActive(true);
                divisors.Add(divisor);
            }
        }
    }

    private void OnValidate()
    {
        SetFillAmount(fillAmount);
    }
}
