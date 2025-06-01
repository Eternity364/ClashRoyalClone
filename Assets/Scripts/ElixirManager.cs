using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ElixirManager : MonoBehaviour
{
    public static ElixirManager Instance { get; private set; }

    [SerializeField]
    private ProgressBar progressBar;
    [SerializeField]
    private TextMeshProUGUI currentValueText;
    [SerializeField]
    private TextMeshProUGUI maxValueText;
    [SerializeField]
    private int maxValue = 10;
    [SerializeField]
    private float changeSpeed = 1.0f;
    [SerializeField]
    private float value = 0f;
    
    private UnityAction<float> onValueChanged;

    public int Value
    {
        get { return (int)value; }
    }

    public void AddOnValueChangedListener(UnityAction<float> listener)
    {
        onValueChanged += listener;
    }

    public void RemoveOnValueChangedListener(UnityAction<float> listener)
    {
        onValueChanged -= listener;
    }

    public void ChangeValue(int change)
    {
        UpdateValue(value + change);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple ElixirManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        progressBar.Init(maxValue);
        maxValueText.text = "Max: " + maxValue.ToString();
    }

    private void UpdateValue(float newValue)
    {
        value = Mathf.Clamp(newValue, 0, maxValue);
        progressBar.SetFillAmount(value / maxValue * 100f);
        currentValueText.text = Value.ToString();
        onValueChanged?.Invoke(value);
    }

    private void Update()
    {
        UpdateValue(value + changeSpeed * Time.deltaTime);
    }
}