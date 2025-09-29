using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class EnemyDisplay : MonoBehaviour
{
    public TurnOrder turnOrder;            

    // Assign Enemy Actions
    public TMP_Text enemyActionText1;      
    public TMP_Text enemyActionText2;
    public TMP_Text enemyActionText3;

    private List<TMP_Text> textFields;

    private void Awake()
    {
        textFields = new List<TMP_Text> { enemyActionText1, enemyActionText2, enemyActionText3 };
    }

    public void DisplayEnemyActions(List<TurnOrder.Unit> aliveEnemies)
    {
        StartCoroutine(DisplayEnemyChoicesRoutine(aliveEnemies));
    }

    private IEnumerator DisplayEnemyChoicesRoutine(List<TurnOrder.Unit> aliveEnemies)
    {
        // Clear previous text
        foreach (var txt in textFields)
            txt.text = "";

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            var enemy = aliveEnemies[i];
            var enemySkills = enemy.Stats.skills;

            if (enemySkills.Length == 0) continue;

            var chosenSkill = enemySkills[Random.Range(0, enemySkills.Length)];

            // Enqueue the action
            turnOrder.enemyActionsQueue.Enqueue(new TurnOrder.PlayerActionType(enemy, chosenSkill));

            // Update the corresponding TMP_Text
            if (i < textFields.Count)
                textFields[i].text = enemy.Stats.charName + " will use " + chosenSkill.skillName;

            Debug.Log(enemy.Stats.charName + " will use " + chosenSkill.skillName);

            yield return new WaitForSeconds(1f); // pacing
        }
    }
}
