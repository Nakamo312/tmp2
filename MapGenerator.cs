using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class BiomeTiles
{
    public TileBase center;
    public TileBase[] edges;
    public TileBase[] corners;
    public TileBase[] innerCorners;
}

public class MapGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap transitionTilemap;
    [SerializeField] private Tilemap environmentTilemap;

    [Header("Biome Tiles")]
    [SerializeField] private List<BiomeTiles> biomes;

    [Header("Environment Tiles")]
    [SerializeField] private List<TileBase> environmentTiles;

    [Header("Map Settings")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 30;

    [Header("Noise Settings")]
    [SerializeField] private float scaleNoise = 10f;
    [SerializeField] private int seed = 0;

    [Header("Environment Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float environmentThreshold = 0.75f;

    private float offsetX;
    private float offsetY;
    private int[,] biomeMap;

    private void Start()
    {
        InitSeed();
        GenerateFullMap();
    }

    private void InitSeed()
    {
        Random.InitState(seed);
        offsetX = Random.Range(0f, 10000f);
        offsetY = Random.Range(0f, 10000f);
    }

    public void GenerateFullMap()
    {
        ValidateSettings();
        ClearMaps();

        biomeMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noise = Mathf.PerlinNoise((x + offsetX) / scaleNoise, (y + offsetY) / scaleNoise);
                int biomeIndex = Mathf.FloorToInt(noise * biomes.Count);
                biomeIndex = Mathf.Clamp(biomeIndex, 0, biomes.Count - 1);
                biomeMap[x, y] = biomeIndex;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                int biomeIndex = biomeMap[x, y];
                BiomeTiles biome = biomes[biomeIndex];

                backgroundTilemap.SetTile(pos, biome.center);

                TileBase transitionTile = GetTransitionTile(x, y);
                if (transitionTile != null)
                {
                    transitionTilemap.SetTile(pos, transitionTile);
                }

                if (environmentTiles.Count > 0)
                {
                    float noiseEnv = Mathf.PerlinNoise((x + offsetX + 1000f) / scaleNoise, (y + offsetY + 1000f) / scaleNoise);
                    if (noiseEnv > environmentThreshold)
                    {
                        int indexEnv = Random.Range(0, environmentTiles.Count);
                        environmentTilemap.SetTile(pos, environmentTiles[indexEnv]);
                    }
                }
            }
        }
    }

    private TileBase GetTransitionTile(int x, int y)
    {
        int current = biomeMap[x, y];
        BiomeTiles biome = biomes[current];

        bool top = (y + 1 < height) && biomeMap[x, y + 1] == current;
        bool bottom = (y - 1 >= 0) && biomeMap[x, y - 1] == current;
        bool left = (x - 1 >= 0) && biomeMap[x - 1, y] == current;
        bool right = (x + 1 < width) && biomeMap[x + 1, y] == current;

        bool topLeft = (x - 1 >= 0 && y + 1 < height) && biomeMap[x - 1, y + 1] == current;
        bool topRight = (x + 1 < width && y + 1 < height) && biomeMap[x + 1, y + 1] == current;
        bool bottomLeft = (x - 1 >= 0 && y - 1 >= 0) && biomeMap[x - 1, y - 1] == current;
        bool bottomRight = (x + 1 < width && y - 1 >= 0) && biomeMap[x + 1, y - 1] == current;

        if (!top && !left && biome.corners.Length > 0)
            return biome.corners[0];
        if (!top && !right && biome.corners.Length > 1)
            return biome.corners[1];
        if (!bottom && !left && biome.corners.Length > 2)
            return biome.corners[2];
        if (!bottom && !right && biome.corners.Length > 3)
            return biome.corners[3];

        if (top && left && !topLeft && biome.innerCorners.Length > 0)
            return biome.innerCorners[0];
        if (top && right && !topRight && biome.innerCorners.Length > 1)
            return biome.innerCorners[1];
        if (bottom && left && !bottomLeft && biome.innerCorners.Length > 2)
            return biome.innerCorners[2];
        if (bottom && right && !bottomRight && biome.innerCorners.Length > 3)
            return biome.innerCorners[3];

        if (!top && biome.edges.Length > 0)
            return biome.edges[0];
        if (!bottom && biome.edges.Length > 1)
            return biome.edges[1];
        if (!left && biome.edges.Length > 2)
            return biome.edges[2];
        if (!right && biome.edges.Length > 3)
            return biome.edges[3];

        return null;
    }

    private void ClearMaps()
    {
        backgroundTilemap.ClearAllTiles();
        transitionTilemap.ClearAllTiles();
        environmentTilemap.ClearAllTiles();
    }

    private void ValidateSettings()
    {
        scaleNoise = Mathf.Max(scaleNoise, 0.001f);

        if (biomes == null || biomes.Count == 0)
        {
            Debug.LogError("Biomes list is empty! Map generation disabled.");
            enabled = false;
        }

        if (environmentTiles == null || environmentTiles.Count == 0)
        {
            Debug.LogWarning("Environment tiles list is empty — objects will not spawn.");
        }
    }
}
