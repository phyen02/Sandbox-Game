using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public TileClass grassTile;
    public TileClass dirtTile;
    public TileClass stoneTile;
    public TileClass tree;
    public TileClass tallGrass;
    public TileClass bedrock;
    

    [Header("Ores")]
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
    public TileClass diamond;
}
