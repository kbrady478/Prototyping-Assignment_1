using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TurnOrder.Unit unit;     

    private Image fillImage;

    public void Initialize(TurnOrder.Unit assignedUnit)
    {
        unit = assignedUnit;

        if (slider != null && unit != null)
        {
            slider.maxValue = unit.Stats.Vigor;
            slider.value = unit.CurrentVigor;
            //UpdateColor();
        }
    }

    private void Update()
    {
        if (unit == null || slider == null) return;

        slider.value = unit.CurrentVigor;
        //UpdateColor();
    }

    
}
