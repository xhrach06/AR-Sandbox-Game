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
    public float timer = 0.0f;
    void Update()
    {
        timer += Time.deltaTime;
        SetTime(timer);
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
}