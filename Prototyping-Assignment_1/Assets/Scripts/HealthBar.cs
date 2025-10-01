using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TurnOrder.Unit unit;

    // Optional offset above the unit
    public Vector3 offset = new Vector3(0, 1f, 0);

    private void Update()
    {
        if (unit == null || slider == null) return;

        // Update health value
        slider.value = unit.CurrentVigor;

        // Follow the unit
        if (unit.visualTransform != null)
        {
            transform.position = unit.visualTransform.position + offset;
            transform.rotation = Quaternion.identity; 
        }
    }

    public void Initialize(TurnOrder.Unit assignedUnit)
    {
        unit = assignedUnit;
        if (slider != null && unit != null)
        {
            slider.maxValue = unit.Stats.Vigor;
            slider.value = unit.CurrentVigor;
        }
    }
}
