using UnityEngine;

[System.Serializable]
public class HeightmapData
{
    public int width;
    public int height;
    public float[] heights; // Store heightmap in a 1D array

    public HeightmapData(float[,] heightmap)
    {
        width = heightmap.GetLength(1); // Width should be second index
        height = heightmap.GetLength(0); // Height should be first index
        heights = new float[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heights[y * width + x] = heightmap[y, x]; // Correct indexing
            }
        }
    }

    public float[,] To2DArray()
    {
        float[,] heightmap = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightmap[y, x] = heights[y * width + x]; // Correct indexing
            }
        }
        return heightmap;
    }
}
