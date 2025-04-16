using UnityEngine;
using TMPro;

/// <summary>
/// Displays game HUD: timer, enemy counts, and game over messages.
/// </summary>
public class HudManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text TimerContainer;
    public TMP_Text KillCountContainer;
    public TMP_Text EnemyCounterContainer;
    public TMP_Text GameOverTextContainer;
    public GameObject RestartButton;

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        SetTime(timer);
    }

    public void SetTime(float time)
    {
        time = (int)time;
        TimerContainer.text = $"{time:F1}s";
    }

    public void SetKillCounter(int enemiesAlive)
    {
        KillCountContainer.text = $"Enemies Alive: {enemiesAlive}";
    }

    public void SetEnemyCounter(int enemiesDefeated)
    {
        EnemyCounterContainer.text = $"Enemies Defeated: {enemiesDefeated}";
    }

    public void SetGameOverText(string message)
    {
        GameOverTextContainer.text = message;
        RestartButton.SetActive(true);
    }

}
