using UnityEngine;

/// <summary>
/// Serializable representation of a terrain heightmap for saving and loading.
/// Stores the 2D heightmap as a flattened 1D array.
/// </summary>
[System.Serializable]
public class HeightmapData
{
    public int width;

    public int height;

    public float[] heights;

    /// <summary>
    /// Constructs a HeightmapData object from a 2D height array.
    /// </summary>
    /// <param name="heightmap">2D float array representing the terrain heightmap.</param>
    public HeightmapData(float[,] heightmap)
    {
        width = heightmap.GetLength(1); // Columns
        height = heightmap.GetLength(0); // Rows
        heights = new float[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heights[y * width + x] = heightmap[y, x];
            }
        }
    }

    /// <summary>
    /// Converts the stored 1D height array back to a 2D heightmap.
    /// </summary>
    /// <returns>2D float array representing the original heightmap.</returns>
    public float[,] To2DArray()
    {
        float[,] heightmap = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightmap[y, x] = heights[y * width + x];
            }
        }

        return heightmap;
    }
}
