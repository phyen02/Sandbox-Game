using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tiles")]
    public float seed;
    public TileAtlas tileAtlas;
    public BiomeClass[] biomes;

    [Header("Terrain")]
    public int worldSize = 100;
    public int chunkSize = 20;
    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    [Header("Biomes")]
    public float biomeFreq;
    public Gradient biomeGradient;
    public Texture2D biomeMap;
    private BiomeClass curBiome;
    private Color[] biomeCols;

    [Header("Noise Setting")]
    public Texture2D caveNoiseTexture;
    public float caveFreq = 0.08f;
    public float terrainFreq = 0.04f;
    public bool generateCave = true;
    public int heightAdditional = 25;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private void Start()
    {
        seed = Random.Range(-10000, 10000);

        for (int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeCol;
        }

        //GenerateTexture();
        drawBiomeMap();
        drawResources();
        GenerateChunk();
        GenerateTerrain();
    }

    public void GenerateTexture()
    {
        //drawBiomeTexture();

        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int j = 0; j < biomes[i].ores.Length; j++)
            {
                biomes[i].ores[j].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTexture(biomes[i].ores[j].rarity, biomes[i].ores[j].size, biomes[i].ores[j].spreadTexture);
            }

            //GenerateNoiseTexture(biomes[i].caveFreq, biomes[i].surfaceValue, biomes[i].caveNoiseTexture);
            // for (int j = 0; j < biomes[i].ores.Length; j++)
            // {
            // }
        }
    }

    public void drawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(worldSize, worldSize);
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFreq, (y + seed) * biomeFreq);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }

    public void drawBiomeTexture()
    {
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * biomeFreq, (y + seed) * biomeFreq);
                Color col = biomeGradient.Evaluate(v);
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }

    public void drawResources()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        float v;
        float o;

        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);

                // draw Caves
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);

                if (v > curBiome.surfaceValue)
                {
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    caveNoiseTexture.SetPixel(x, y, Color.black);
                }

                // draw ores
                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].rarity, (y + seed) * curBiome.ores[i].rarity);
                        if (o > curBiome.ores[i].size)
                        {
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
                        }
                        ores[i].spreadTexture.Apply();
                    }
                }
            }
        }
        caveNoiseTexture.Apply();

        //draw Ores
        /*for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        float v = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].rarity, (y + seed) * curBiome.ores[i].rarity);
                        if (v > curBiome.ores[i].size)
                        {
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
                        }
                        ores[i].spreadTexture.Apply();
                    }
                }
            }
        } */
    }

    public void GenerateChunk()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;

        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        if (System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y)) >= 0)
        {
            return biomes[System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y))];
        }
        return curBiome;
    }

    public void GenerateTerrain()
    {
        Sprite[] tileSprites;
        for (int x = 0; x < worldSize; x++)
        {
            float height;
            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAdditional;

                if (y >= height) break;

                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileSprites = curBiome.tileAtlas.stoneTile.tileSprites;

                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                    {
                        tileSprites = tileAtlas.coal.tileSprites;
                    }

                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                    {
                        tileSprites = tileAtlas.iron.tileSprites;
                    }

                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                    {
                        tileSprites = tileAtlas.gold.tileSprites;
                    }

                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                    {
                        tileSprites = tileAtlas.diamond.tileSprites;
                    }
                }
                else if (y < height - 1)
                {
                    tileSprites = curBiome.tileAtlas.dirtTile.tileSprites;
                }
                else
                {
                    tileSprites = curBiome.tileAtlas.grassTile.tileSprites;
                }

                if (generateCave)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprites, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, x, y);
                }

                if (y >= height - 1)
                {
                    int treeSpawn = Random.Range(0, curBiome.treeSpawnChance);
                    if (treeSpawn == 1)
                    {
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            if (curBiome.tileAtlas.tree != null)
                            {
                                PlaceTile(curBiome.tileAtlas.tree.tileSprites, x, y + 3);
                            }
                        }
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.grassSpawnChance);
                        if (i == 1)
                        {
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                if (curBiome.tileAtlas.tallGrass != null)
                                {
                                    GenerateGrass(curBiome.tileAtlas.tallGrass.tileSprites, x, y + 1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void GenerateNoiseTexture(float freq, float limit, Texture2D noiseTexture)
    {
        float v;
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * freq, (y + seed) * freq);

                if (v > limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
            }
        }
        noiseTexture.Apply();
    }

    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        if (!worldTiles.Contains(new Vector2Int(x, y)))
        {
            GameObject newTile = new GameObject();

            float chunkCoord = Mathf.RoundToInt(x / chunkSize) * chunkSize;
            chunkCoord /= chunkSize;

            newTile.transform.parent = worldChunks[(int)chunkCoord].transform;

            newTile.AddComponent<SpriteRenderer>();
            int spriteIndex = Random.Range(0, tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];
            newTile.name = tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
            worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
        }
    }

    void GenerateGrass(Sprite[] grassSprites, int x, int y)
    {
        if (!worldTiles.Contains(new Vector2Int(x, y)))
        {
            GameObject newTile = new GameObject();

            float chunkCoord = Mathf.RoundToInt(x / chunkSize) * chunkSize;
            chunkCoord /= chunkSize;

            if (chunkCoord < worldChunks.Length)
            {
                newTile.transform.parent = worldChunks[(int)chunkCoord].transform;
            }

            newTile.AddComponent<SpriteRenderer>();
            int spriteIndex = Random.Range(0, grassSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = grassSprites[spriteIndex];
            newTile.name = grassSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.1f);
            newTile.transform.localScale = new Vector3(1, 2, 1);
            worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
        }
    }
}