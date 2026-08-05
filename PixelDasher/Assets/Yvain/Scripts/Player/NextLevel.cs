using UnityEngine;

public class NextLevel : MonoBehaviour
{
    public int nextLevelIndex;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelIndex);
        }
    }
}
