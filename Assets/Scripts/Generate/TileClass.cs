using UnityEngine;

[CreateAssetMenu(fileName = "newTileClass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    public string tileName;
    public Sprite[] tileSprites;
}