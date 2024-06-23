using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    /*---------------------------- DEFINES ----------------------------*/
    [Header("Tiles")]
    public float seed;
    public TileAtlas tileAtlas;
    public BiomeClass[] biomes;

    [Header("Terrain")]
    public int worldSize = 100;
    public int chunkSize = 20;
    private GameObject[] worldChunks;
    private GameObject[,] worldForegroundObjects;
    private GameObject[,] worldBackgroundObjects;
    private TileClass[,] worldBackground;
    private TileClass[,] worldForeground;

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

    public PlayerController player;
    public CameraController cam;
    public GameObject tileDrop;

    [Header("Light")]
    public Texture2D worldTileMaps;
    public Material lightShader;
    public float groundLightThreshold = 0.7f;
    public float airLightThreashold = 0.85f;
    public float lightRadius = 7f;
    List<Vector2Int> unlitBlocks = new List<Vector2Int>();

    /*---------------------------- FUNCTION ----------------------------*/
    private void Start()
    {
        /* initialize background & foreground */
        worldBackground = new TileClass[worldSize, worldSize];
        worldForeground = new TileClass[worldSize, worldSize];
        worldBackgroundObjects = new GameObject[worldSize, worldSize];
        worldForegroundObjects = new GameObject[worldSize, worldSize];

        /*----- initialize light ----- */
        worldTileMaps = new Texture2D(worldSize, worldSize);
        //worldTileMaps.filterMode = FilterMode.Point;
        lightShader.SetTexture("_shadowTexture", worldTileMaps);
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                worldTileMaps.SetPixel(x, y, Color.white);
            }
        }

        /*----- generate terrain ----- */
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

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if (worldTileMaps.GetPixel(x, y) == Color.white)
                {
                    LightBlock(x, y, 1f, 0);
                }
            }
        }
        worldTileMaps.Apply();

        /*----- setup camera and player ----- */
        cam.Spawn(new Vector3(player.spawnPos.x, player.spawnPos.y, cam.transform.position.z));
        cam.worldSize = worldSize;
        player.Spawn();

        /*----- loads chunk at player position ----- */
        LoadChunks();
    }

    private void Update()
    {
        LoadChunks();
    }

    public void LoadChunks()
    {
        for (int i = 0; i < worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0),
                                new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 6f)
            {
                worldChunks[i].SetActive(false);
            }
            else
            {
                worldChunks[i].SetActive(true);
            }
        }
    }

    #region Generate Terrain
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
        TileClass tile;

        for (int x = 0; x < worldSize - 1; x++)
        {
            float height;

            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAdditional;

                if (x == worldSize / 2)
                {
                    player.spawnPos = new Vector3(x, height + 1, -1);
                }

                if (y >= height) break;

                if (y < height - curBiome.dirtLayerHeight)
                {
                    tile = curBiome.tileAtlas.stoneTile;

                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                    {
                        tile = tileAtlas.coal;
                    }

                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                    {
                        tile = tileAtlas.iron;
                    }

                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                    {
                        tile = tileAtlas.gold;
                    }

                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                    {
                        tile = tileAtlas.diamond;
                    }
                }
                else if (y < height - 1)
                {
                    tile = curBiome.tileAtlas.dirtTile;
                }
                else
                {
                    tile = curBiome.tileAtlas.grassTile;
                }

                if (y == 0)
                {
                    tile = tileAtlas.bedrock;
                }

                if (generateCave && y > 0)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tile, x, y, true);
                    }
                    else if (tile.wallVariant != null)
                    {
                        PlaceTile(tile.wallVariant, x, y, true);
                    }
                }
                else
                {
                    PlaceTile(tile, x, y, true);
                }

                if (y >= height - 1)
                {
                    int treeSpawn = Random.Range(0, curBiome.treeSpawnChance);
                    if (treeSpawn == 1)
                    {
                        if (GetTileFromWorld(x, y))
                        {
                            if (curBiome.tileAtlas.tree != null)
                            {
                                PlaceTile(curBiome.tileAtlas.tree, x, y + 3, true);
                            }
                        }
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.grassSpawnChance);
                        if (i == 1)
                        {
                            if (GetTileFromWorld(x, y))
                            {
                                if (curBiome.tileAtlas.tallGrass != null)
                                {
                                    PlaceTile(curBiome.tileAtlas.tallGrass, x, y + 1, true);
                                }
                            }
                        }
                    }
                }

                // if (y >= height - 1 && isEdge())
                // {

                // }
            }
        }
        worldTileMaps.Apply();
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
    #endregion

    #region Block interact
    public bool CheckTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            if (tile.isBgElement)
            {
                if (GetTileFromWorld(x + 1, y) ||
                    GetTileFromWorld(x - 1, y) ||
                    GetTileFromWorld(x, y + 1) ||
                    GetTileFromWorld(x, y - 1))
                {
                    if (!GetTileFromWorld(x, y))
                    {
                        RemoveLightSource(x, y);
                        PlaceTile(tile, x, y, isNaturallyPlaced);
                        return true;
                    }
                    else
                    {
                        if (!GetTileFromWorld(x, y).isBgElement)
                        {
                            RemoveLightSource(x, y);
                            PlaceTile(tile, x, y, isNaturallyPlaced);
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (GetTileFromWorld(x + 1, y) ||
                    GetTileFromWorld(x - 1, y) ||
                    GetTileFromWorld(x, y + 1) ||
                    GetTileFromWorld(x, y - 1))
                {
                    if (!GetTileFromWorld(x, y))
                    {
                        RemoveLightSource(x, y);
                        PlaceTile(tile, x, y, isNaturallyPlaced);
                        return true;
                    }
                    else
                    {
                        if (GetTileFromWorld(x, y).isBgElement)
                        {
                            RemoveLightSource(x, y);
                            PlaceTile(tile, x, y, isNaturallyPlaced);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void PlaceTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            GameObject newTile = new GameObject();

            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;

            newTile.transform.parent = worldChunks[chunkCoord].transform;

            newTile.AddComponent<SpriteRenderer>();

            int spriteIndex = Random.Range(0, tile.tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tile.tileSprites[spriteIndex];

            worldTileMaps.SetPixel(x, y, Color.black);

            if (tile.isBgElement)
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
                worldTileMaps.SetPixel(x, y, Color.white);
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }

            // REVIEW
            newTile.name = tile.tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            TileClass newTileClass = TileClass.CreateInstance(tile, isNaturallyPlaced);

            AddObjectToWorld(newTile, tile, x, y);
            AddTileToWorld(newTileClass, x, y);
        }
    }

    void AddTileToWorld(TileClass tile, int x, int y)
    {
        if (tile.isBgElement)
        {
            worldBackground[x, y] = tile;
        }
        else
        {
            worldForeground[x, y] = tile;
        }
    }

    void RemoveTileFromWorld(int x, int y)
    {
        if (worldForeground[x, y] != null)
        {
            worldForeground[x, y] = null;
        }
        else if (worldBackground[x, y] != null)
        {
            worldBackground[x, y] = null;
        }
    }

    TileClass GetTileFromWorld(int x, int y)
    {
        if (worldForeground[x, y] != null && worldForeground[x, y].toolToBreak != ItemClass.ToolType.unbreakable)
        {
            return worldForeground[x, y];
        }
        else if (worldBackground[x, y] != null)
        {
            return worldBackground[x, y];
        }

        return null;
    }

    void AddObjectToWorld(GameObject obj, TileClass tile, int x, int y)
    {
        if (tile.isBgElement)
        {
            worldBackgroundObjects[x, y] = obj;
        }
        else
        {
            worldForegroundObjects[x, y] = obj;
        }
    }

    GameObject GetObjectFromWorld(int x, int y)
    {
        if (worldForegroundObjects[x, y] != null)
        {
            return worldForegroundObjects[x, y];
        }
        else if (worldBackgroundObjects[x, y] != null)
        {
            return worldBackgroundObjects[x, y];
        }

        return null;
    }

    void RemoveObjectFromWorld(int x, int y)
    {
        if (worldForegroundObjects[x, y] != null)
        {
            worldForegroundObjects[x, y] = null;
        }
        else if (worldBackgroundObjects[x, y] != null)
        {
            worldBackgroundObjects[x, y] = null;
        }
    }

    public void RemoveTile(int x, int y)
    {
        if (GetTileFromWorld(x, y) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            TileClass tile = GetTileFromWorld(x, y);
            RemoveTileFromWorld(x, y);

            if (tile.wallVariant != null)
            {
                if (tile.naturallyPlaced)
                {
                    PlaceTile(tile.wallVariant, x, y, true);
                }
            }

            if (tile.tileDrop != null)
            {
                GameObject newItemDrop = Instantiate(tileDrop, new Vector2(x, y + 0.5f), Quaternion.identity);
                newItemDrop.GetComponent<SpriteRenderer>().sprite = tile.tileDrop.tileSprites[0];
                ItemClass itemDrop = new ItemClass(tile.tileDrop);
                newItemDrop.GetComponent<BlockDropController>().item = itemDrop;

            }

            if (!GetTileFromWorld(x, y))
            {
                worldTileMaps.SetPixel(x, y, Color.white);
                LightBlock(x, y, 1f, 0);
                worldTileMaps.Apply();
            }

            Destroy(GetObjectFromWorld(x, y));
            RemoveObjectFromWorld(x, y);
        }
    }

    public bool toolBreakTile(int x, int y, ItemClass item)
    {
        if (GetTileFromWorld(x, y) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            TileClass tile = GetTileFromWorld(x, y);

            if (tile.toolToBreak == ItemClass.ToolType.none)
            {
                RemoveTile(x, y);
                return true;
            }
            else
            {
                if (item != null)
                {
                    if (item.itemType == ItemClass.ItemType.tool)
                    {
                        if (tile.toolToBreak == item.toolType)
                        {
                            RemoveTile(x, y);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Shader
    // REVIEW
    public void LightBlock(int x, int y, float intensity, int interation)
    {
        if (interation < lightRadius)
        {
            worldTileMaps.SetPixel(x, y, Color.white * intensity);

            float thresh = airLightThreashold;

            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                if (worldForeground[x, y])
                {
                    thresh = groundLightThreshold;
                }
                else
                {
                    thresh = airLightThreashold;
                }
            }

            for (int nx = x - 1; nx < x + 2; nx++)
            {
                for (int ny = y - 1; ny < y + 2; ny++)
                {
                    if (nx != x || ny != y)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                        float targetIntensity = Mathf.Pow(thresh, distance) * intensity;

                        if (worldTileMaps.GetPixel(nx, ny).r < targetIntensity)
                        {
                            LightBlock(nx, ny, targetIntensity, interation + 1);
                        }
                    }
                }
            }
        }
        worldTileMaps.Apply();
    }

    // REVIEW
    void RemoveLightSource(int x, int y)
    {
        unlitBlocks.Clear();
        UnlightBlock(x, y, x, y);

        List<Vector2Int> toRelight = new List<Vector2Int>();

        foreach (Vector2Int block in unlitBlocks)
        {
            for (int nx = block.x - 1; nx < block.x + 2; nx++)
            {
                for (int ny = block.y - 1; ny < block.y + 2; ny++)
                {
                    if (worldTileMaps.GetPixel(nx, ny) != null)
                    {
                        if (worldTileMaps.GetPixel(nx, ny).r > worldTileMaps.GetPixel(block.x, block.y).r)
                        {
                            if (!toRelight.Contains(new Vector2Int(nx, ny)))
                            {
                                toRelight.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }
        }

        foreach (Vector2Int source in toRelight)
        {
            LightBlock(source.x, source.y, worldTileMaps.GetPixel(source.x, source.y).r, 0);
        }

        worldTileMaps.Apply();
    }

    // REVIEW
    void UnlightBlock(int x, int y, int ix, int iy)
    {
        if (Mathf.Abs(x - ix) >= lightRadius || Mathf.Abs(y - iy) >= lightRadius || unlitBlocks.Contains(new Vector2Int(x, y)))
            return;

        for (int nx = x - 1; nx < x + 2; nx++)
        {
            for (int ny = y - 1; ny < y + 2; ny++)
            {
                if (nx != x || ny != y)
                {
                    if (worldTileMaps.GetPixel(nx, ny) != null)
                    {
                        if (worldTileMaps.GetPixel(nx, ny).r < worldTileMaps.GetPixel(x, y).r)
                        {
                            UnlightBlock(nx, ny, ix, iy);
                        }
                    }
                }
            }
        }

        worldTileMaps.SetPixel(x, y, Color.black);
        unlitBlocks.Add(new Vector2Int(x, y));
    }
    #endregion
}