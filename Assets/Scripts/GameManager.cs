using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public float loseY = -5f;

    [Header("Game State")]
    public bool isGameOver;

    private float survivalTime;

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        survivalTime += Time.deltaTime;

        if (player != null && player.position.y < loseY)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        Debug.Log("Game Over! Survival time: " + survivalTime.ToString("0.0") + " seconds");

        // Пока просто перезапускаем сцену через 2 секунды.
        Invoke(nameof(RestartScene), 2f);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetSurvivalTime()
    {
        return survivalTime;
    }
}
