using UnityEngine;

public class GameplayCameraManager : MonoBehaviour
{
    private Camera mainCamera;

    [SerializeField]
    private KinectDepthTerrain kinectDepthTerrain; // Reference to KinectDepthTerrain in the GameplayScene

    private void Start()
    {
        mainCamera = Camera.main;

        // Load the camera settings from PlayerPrefs
        LoadCameraSettings();

        // Load the terrain rotation settings from PlayerPrefs
        if (kinectDepthTerrain != null)
        {
            kinectDepthTerrain.terrainRotation = new Vector2(
                PlayerPrefs.GetFloat("xRotation", 0),
                PlayerPrefs.GetFloat("yRotation", 0)
            );
        }
        else
        {
            Debug.LogWarning("KinectDepthTerrain not assigned to GameplayCameraManager.");
        }
    }

    private void LoadCameraSettings()
    {
        // Load camera position
        Vector3 position = new Vector3(
            PlayerPrefs.GetFloat("CameraPositionX", 0),
            PlayerPrefs.GetFloat("CameraPositionY", 0),
            PlayerPrefs.GetFloat("CameraPositionZ", 0)
        );
        mainCamera.transform.position = position;

        // Load camera size
        mainCamera.orthographicSize = PlayerPrefs.GetFloat("CameraSize", 5);

        //Debug.Log("Gameplay camera settings loaded.");
    }
}
