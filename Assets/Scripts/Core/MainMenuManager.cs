﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartCalibration()
    {
        SceneManager.LoadScene("CalibrationScene");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameplayScene");
    }

    public void PlayKinect()
    {
        SceneManager.LoadScene("KinectGameplayScene");
    }
}
