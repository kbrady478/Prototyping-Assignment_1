using UnityEngine;
using TMPro;
using System.Linq;

public class TurnDisplay : MonoBehaviour
{
    public TurnOrder turnOrder;      
    public TMP_Text displayText;     

    
    public void UpdateTurnOrderDisplay()
    {
        if (turnOrder == null || displayText == null) return;

        var orderList = turnOrder.initiativeOrderList;

        if (orderList == null || orderList.Count == 0)
        {
            displayText.text = "Turns: (No units)";
            return;
        }

        string display = string.Join(" -> ",
            orderList.Select(u => u.Stats.charName + (u.IsDead() ? " (Dead)" : "")));

        displayText.text = display;

        Debug.Log("Turn Display Updated: " + display);
    }
}
