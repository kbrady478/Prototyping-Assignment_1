using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TurnOrder.Unit unit;     

    private Image fillImage;

    private void Awake()
    {
        if (slider != null)
            fillImage = slider.fillRect.GetComponent<Image>();
    }

    public void Initialize(TurnOrder.Unit assignedUnit)
    {
        unit = assignedUnit;

        if (slider != null && unit != null)
        {
            slider.maxValue = unit.Stats.Vigor;
            slider.value = unit.CurrentVigor;
            UpdateColor();
        }
    }

    private void Update()
    {
        if (unit == null || slider == null) return;

        slider.value = unit.CurrentVigor;
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (fillImage == null || unit == null) return;

        fillImage.color = unit.IsDead() ? Color.red : Color.green;
    }
}
