using UnityEngine;

public class FullscreenManager : MonoBehaviour
{
    void Start()
    {
        // Force Exclusive Fullscreen at Game Start
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.fullScreen = true;
    }

    void Update()
    {
        // Press F11 to toggle fullscreen
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }
    }

    void ToggleFullscreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        Screen.fullScreen = !Screen.fullScreen;
    }
}
