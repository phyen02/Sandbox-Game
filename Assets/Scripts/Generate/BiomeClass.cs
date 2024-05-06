using System.Collections;
using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string Name;
    public Color biomeCol;
    public TileAtlas tileAtlas;

    [Header("Noise Setting")]
    public float caveFreq = 0.08f;
    public float terrainFreq = 0.04f;
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generateCave = true;
    public float surfaceValue = 0.25f;
    public int dirtLayerHeight = 5;
    public float heightMultiplier = 4f;

    [Header("Tree")]
    public int treeSpawnChance = 10;

    [Header("Addons")]
    public int grassSpawnChance = 3;

    [Header("Ore Settings")]
    public OreClass[] ores;

}
