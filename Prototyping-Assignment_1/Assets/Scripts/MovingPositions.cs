using UnityEngine;

public class MovingPositions : MonoBehaviour
{
    public GameObject[] playerObjects; // 0 = front, 1 = middle, 2 = back
    public Transform[] playerPositions;

    public GameObject[] enemyObjects; // 0 = front, 1 = middle, 2 = back
    public Transform[] enemyPositions;

    private void Start()
    {
        // Set initial positions instantly
        for (int i = 0; i < playerObjects.Length; i++)
            playerObjects[i].transform.position = playerPositions[i].position;

        for (int i = 0; i < enemyObjects.Length; i++)
            enemyObjects[i].transform.position = enemyPositions[i].position;
    }

    public void MovePlayerInstant(int playerIndex, int newPositionIndex)
    {
        SwapAndMove(playerObjects, playerIndex, playerPositions, newPositionIndex);
    }

    
    public void MoveEnemyInstant(int enemyIndex, int newPositionIndex)
    {
        SwapAndMove(enemyObjects, enemyIndex, enemyPositions, newPositionIndex);
    }

    private void SwapAndMove(GameObject[] units, int index, Transform[] positions, int targetIndex)
    {
        Vector3 targetPos = positions[targetIndex].position;

        
        for (int i = 0; i < units.Length; i++)
        {
            if (i == index) continue;
            if (Vector3.Distance(units[i].transform.position, targetPos) < 0.01f)
            {
                
                Vector3 temp = units[i].transform.position;
                units[i].transform.position = units[index].transform.position;
                break;
            }
        }

        
        units[index].transform.position = targetPos;
    }
}
