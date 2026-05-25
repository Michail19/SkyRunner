using UnityEngine;
using UnityEngine.SceneManagement;

public class FallGameOver : MonoBehaviour
{
    public Transform player;
    public float loseY = -5f;

    private bool gameOver;

    private void Update()
    {
        if (gameOver || player == null)
        {
            return;
        }

        if (player.position.y < loseY)
        {
            gameOver = true;
            Debug.Log("Game Over: player fell.");

            // Временно просто перезапуск сцены.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
