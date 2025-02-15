using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using Windows.Kinect;
using Windows.Kinect;

public class Gesture : MonoBehaviour
{
    [System.Serializable]
    public class ServerResponse
    {
        public string gesture;
        public int? x;
        public int? y;
        public string image_filename;
    }

    [SerializeField] public MultiSourceManager multiSourceManager;
    public RawImage rawImage;

    private string serverIP = "127.0.0.1";
    private int serverPort = 8080;
    private string detectedGesture = "None";
    private float lastSendTime = 0f;
    private const float sendInterval = 1f;  // 2-second interval between sends
    private bool isSending = false;  // Flag to check if we're currently sending data

    private KinectSensor kinectSensor;
    private CoordinateMapper coordinateMapper;

    private async void Start()
    {
        rawImage = FindObjectOfType<RawImage>();
        kinectSensor = KinectSensor.GetDefault();
        coordinateMapper = kinectSensor.CoordinateMapper;
    }

    private void Update()
    {
        // Capture the color and depth frame from the Kinect
        Texture2D colorTexture = CaptureColorFrame();

        // Display the color frame on the RawImage
        if (rawImage != null && colorTexture != null)
        {
            rawImage.texture = colorTexture;
        }

        // Send image to the server at defined intervals, but only if we're not already sending
        if (Time.time - lastSendTime > sendInterval && !isSending)
        {
            lastSendTime = Time.time;
            _ = SendImageToServerAsync(colorTexture);
        }
    }

    private Texture2D FlipImageVertically(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Array.Reverse(pixels);  // Reverse the rows of pixels
        Texture2D flippedTexture = new Texture2D(texture.width, texture.height);
        flippedTexture.SetPixels(pixels);
        flippedTexture.Apply();
        return flippedTexture;
    }

    private Texture2D CaptureColorFrame()
    {
        // Get the color texture from the Kinect's color camera
        Texture2D colorTexture = multiSourceManager.GetColorTexture(); // Assuming GetColorTexture() returns a Texture2D
        ushort[] depthData = multiSourceManager.GetDepthData(); // Assuming GetDepthData() returns a ushort array of depth data

        Color[] colorData = colorTexture.GetPixels();

        // Get the Kinect CoordinateMapper to map depth to color space
        var coordinateMapper = KinectSensor.GetDefault().CoordinateMapper;
        ColorSpacePoint[] colorSpacePoints = new ColorSpacePoint[depthData.Length];

        // Map the depth data to color space
        coordinateMapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

        // Iterate through the depth data and modify the color image
        for (int i = 0; i < depthData.Length; i++)
        {
            ushort depth = depthData[i];
            
            ColorSpacePoint colorSpacePoint = colorSpacePoints[i];

            // Check if the mapped color space point is within the bounds of the color image
            if (colorSpacePoint.X >= 0 && colorSpacePoint.X < colorTexture.width && colorSpacePoint.Y >= 0 && colorSpacePoint.Y < colorTexture.height)
            {
                // Keep the original color for valid depth pixels
                colorData[i] = colorData[i];
            }
            
        }

        // Create a new Texture2D with the updated color data (red for invalid depth pixels)
        Texture2D modifiedTexture = new Texture2D(colorTexture.width, colorTexture.height, TextureFormat.RGB24, false);
        modifiedTexture.SetPixels(colorData); // Set the modified color data
        modifiedTexture.Apply(); // Apply the changes

        return modifiedTexture;
    }




    private async Task SendImageToServerAsync(Texture2D texture)
    {
        if (isSending)
        {
            //Debug.Log("Waiting for server response...");
            return;
        }

        isSending = true;
        Texture2D flippedTexture = FlipImageVertically(texture);
        byte[] imageBytes = flippedTexture.EncodeToPNG();

        //Debug.Log("Encoded image size: " + imageBytes.Length);

        try
        {
            using (TcpClient client = new TcpClient(serverIP, serverPort))
            using (NetworkStream stream = client.GetStream())
            {
                await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                client.Client.Shutdown(SocketShutdown.Send);

                byte[] responseBuffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                if (bytesRead > 0)
                {
                    string serverResponse = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                    Debug.Log($"Raw server response: {serverResponse}");
                    if (string.IsNullOrEmpty(serverResponse))
                    {
                        Debug.LogError("Empty response received from server.");
                    }

                    // Deserialize JSON response using JsonUtility
                    try
                    {
                        ServerResponse result = JsonUtility.FromJson<ServerResponse>(serverResponse);
                        if (result != null)
                        {
                            detectedGesture = result.gesture;
                            Debug.Log($"Detected Gesture: {result.gesture}, Position: ({result.x}, {result.y}), Image File: {result.image_filename}");
                            if (result.gesture != "None")
                            {
                                Debug.Log($"Detected Gesture: {result.gesture}, Position: ({result.x}, {result.y}), Image File: {result.image_filename}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSON Parsing Error: {ex.Message}");
                    }
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
            isSending = false;
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application quitting. Cleaning up resources.");
    }
}
