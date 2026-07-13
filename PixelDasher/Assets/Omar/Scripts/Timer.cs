using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float timeElapsed = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;
        timeElapsed += Time.deltaTime;
        DisplayTime(timeElapsed);
    }

    void DisplayTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Appelé quand le joueur termine le niveau
    public void StopTimer()
    {
        isRunning = false;
    }

    // Appelé quand le joueur redémarre le chapitre
    public void ResetTimer()
    {
        timeElapsed = 0f;
        isRunning = true;
    }
}