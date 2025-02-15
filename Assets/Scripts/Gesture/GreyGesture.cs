using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

public class KinectDepth : MonoBehaviour
{
    [SerializeField] public MultiSourceManager multiSourceManager;
    public RawImage rawImage;
    private ushort[] rawDepthData;
    private Texture2D depthTexture;

    private const int MapDepthToByte = 8;
    private readonly Vector2Int depthResolution = new Vector2Int(512, 424);
    private const ushort minDepth = 600;
    private const ushort maxDepth = 1250;

    private string serverIP = "127.0.0.1";
    private int serverPort = 8080;
    private string detectedGesture = "None";
    private float lastSendTime = 0f;
    private const float sendInterval = 2.0f;  // 2-second interval between sends
    private bool isSending = false;  // Flag to check if we're currently sending data

    private async void Start()
    {
        rawImage = FindObjectOfType<RawImage>();
        depthTexture = new Texture2D(depthResolution.x, depthResolution.y, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        rawDepthData = multiSourceManager.GetDepthData();
        ShowDepthData();
        //Debug.Log("Detected Gesture: " + detectedGesture);
    }

    private void ShowDepthData()
    {
        for (int i = 0; i < rawDepthData.Length; i++)
        {
            ushort depth = rawDepthData[i];
            byte grayscaleValue = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            Color pixelColor = new Color(grayscaleValue / 255f, grayscaleValue / 255f, grayscaleValue / 255f);
            int x = i % depthResolution.x;
            int y = i / depthResolution.x;
            depthTexture.SetPixel(x, y, pixelColor);
        }

        depthTexture.Apply();
        if (rawImage != null)
        {
            rawImage.texture = depthTexture;
        }
        else
        {
            Debug.LogError("RawImage component not found!");
        }

        // Send image to server at defined intervals, but only if we're not already sending
        if (Time.time - lastSendTime > sendInterval && !isSending)
        {
            lastSendTime = Time.time;
            _ = SendImageToServerAsync(depthTexture);
        }
    }

    private async Task SendImageToServerAsync(Texture2D texture)
    {
        // Prevent sending another request if we're already sending data
        if (isSending)
        {
            Debug.Log("Waiting for server response...");
            return;
        }

        isSending = true;  // Indicate that we are sending data
        byte[] imageBytes = texture.EncodeToPNG();
        Debug.Log("Encoded image size: " + imageBytes.Length);

        try
        {
            using (TcpClient client = new TcpClient(serverIP, serverPort))
            using (NetworkStream stream = client.GetStream())
            {
                // Send image data
                await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                Debug.Log("Image data sent to server.");

                // Wait for server response
                byte[] responseBuffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                if (bytesRead > 0)
                {
                    string serverResponse = System.Text.Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                    Debug.Log($"Server Response: {serverResponse}");
                    detectedGesture = serverResponse;  // Store the result in detectedGesture
                }
                else
                {
                    Debug.Log("No response from server.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Network Error: {ex.Message}");
        }

        finally
        {
            isSending = false;  // Allow sending the next request
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application quitting. Cleaning up resources.");
    }
}
