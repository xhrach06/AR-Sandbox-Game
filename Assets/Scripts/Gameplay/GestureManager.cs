/*

using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class GestureManager : MonoBehaviour
{
    public KinectDepthTerrain kinectDepthTerrain;  // Reference to Kinect depth system
    public GameObject meteorPrefab;  // Meteor prefab to spawn

    private string serverAddress = "localhost";
    private int serverPort = 8765;
    private Texture2D depthTexture;

    void Start()
    {
        depthTexture = new Texture2D(512, 424, TextureFormat.R8, false);
        StartCoroutine(SendDepthImageRoutine());
    }

    IEnumerator SendDepthImageRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);  // Send image every 0.5 seconds
            SendDepthImage();
        }
    }

    async void SendDepthImage()
    {
        // Capture Kinect depth as grayscale
        byte[] imageBytes = CaptureDepthImage();

        using (TcpClient client = new TcpClient(serverAddress, serverPort))
        using (NetworkStream stream = client.GetStream())
        {
            await stream.WriteAsync(imageBytes, 0, imageBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            ProcessGestureResponse(receivedMessage);
        }
    }

    byte[] CaptureDepthImage()
    {
        float[,] depthData = kinectDepthTerrain.GetDepthMap();
        int width = depthData.GetLength(0);
        int height = depthData.GetLength(1);

        // Ensure the texture is initialized
        if (depthTexture == null || depthTexture.width != width || depthTexture.height != height)
        {
            depthTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float depth = depthData[x, y];
                byte pixelValue = (byte)(depth * 255); // Normalize depth
                depthTexture.SetPixel(x, y, new Color32(pixelValue, pixelValue, pixelValue, 255));
            }
        }

        depthTexture.Apply();
        return depthTexture.EncodeToPNG();
    }


    void ProcessGestureResponse(string response)
    {
        if (response.StartsWith("peace_sign"))
        {
            string[] parts = response.Split(',');
            if (parts.Length == 3)
            {
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);

                SpawnMeteor(new Vector3(x, 10, y));
            }
        }
    }

    void SpawnMeteor(Vector3 position)
    {
        Instantiate(meteorPrefab, position, Quaternion.identity);
        Debug.Log("🔥 Meteor summoned at " + position);
    }
}
*/