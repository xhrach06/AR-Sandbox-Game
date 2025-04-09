using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudManager : MonoBehaviour
{
    private Canvas canvas;
    public TMP_Text TimerContainer;
    public TMP_Text KillCountContainer;
    public TMP_Text EnemyCounterContainer;
    public TMP_Text GameOverTextContainer;
    public float timer = 0.0f;
    private int lastDisplayedTime = -1;

    void Update()
    {
        timer += Time.deltaTime;
        int timeInt = (int)timer;

        if (timeInt != lastDisplayedTime)
        {
            SetTime(timeInt);
            lastDisplayedTime = timeInt;
        }
    }

    public void SetTime(float time)
    {
        time = (int)time;
        TimerContainer.text = $"{time:F1}s";
    }
    public void SetKillCounter(int enemyCount)
    {
        KillCountContainer.text = $"Enemies Alive: {enemyCount}";
    }
    public void SetEnemyCounter(int killCount)
    {
        EnemyCounterContainer.text = $"Enemies Defeated: {killCount}";
    }
    public void SetGameOverText(string message)
    {
        GameOverTextContainer.text = $"{message}";
    }
}