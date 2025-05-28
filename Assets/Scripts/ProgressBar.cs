using System.Collections;
using System.Collections.Generic;
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
            segments[i].SetActive(value <= fillAmount);
            if (i < segmentsCount - 1)
            {
                divisors[i].SetActive(value <= fillAmount);
            }
        }
    }

    private void CreateSegments()
    {
        for (int i = 0; i < segmentsCount; i++)
        {
            GameObject segment = Instantiate(segmentPrefab, parentTransform);
            Vector3 scale = segment.transform.localScale;
            Vector3 position = segment.transform.localPosition;
            position.x = imageWidth * (1f / segmentsCount * i - 1f / 2f + 1f / segmentsCount / 2f);
            scale.x = 1f / segmentsCount;
            segment.transform.localScale = scale;
            segment.transform.localPosition = position;
            segment.SetActive(true);
            segments.Add(segment);
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
