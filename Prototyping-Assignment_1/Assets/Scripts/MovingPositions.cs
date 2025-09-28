using UnityEngine;

public class MovingPositions : MonoBehaviour
{
    public GameObject[] playerObjects; // 0 = front, 1 = middle, 2 = back
    public Transform[] playerPositions;

    public GameObject[] enemyObjects; // 0 = front, 1 = middle, 2 = back
    public Transform[] enemyPositions;

    public void MovePlayer(int playerIndex, int newPositionIndex)
    {
        playerObjects[playerIndex].transform.position = playerPositions[newPositionIndex].position;
    }

    public void MoveEnemy(int enemyIndex, int newPositionIndex)
    {
        enemyObjects[enemyIndex].transform.position = enemyPositions[newPositionIndex].position;
    }

    private void Start()
    {
        for (int i = 0; i < playerObjects.Length; i++)
            playerObjects[i].transform.position = playerPositions[i].position;

        for (int i = 0; i < enemyObjects.Length; i++)
            enemyObjects[i].transform.position = enemyPositions[i].position;
    }
}
