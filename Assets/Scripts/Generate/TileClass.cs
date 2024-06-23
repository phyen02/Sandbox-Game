using UnityEngine;

[CreateAssetMenu(fileName = "newTileClass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    public string tileName;
    public TileClass wallVariant;
    public Sprite[] tileSprites;
    public bool isBgElement = false;
    public bool naturallyPlaced = true;
    public TileClass tileDrop;
    public ItemClass.ToolType toolToBreak;
    public bool isStackable = true;
    //public bool isMining;

    public static TileClass CreateInstance(TileClass tile, bool isNaturallyPlaced)
    {
        var thisTile = ScriptableObject.CreateInstance<TileClass>();
        thisTile.Init(tile, isNaturallyPlaced);

        return thisTile;
    }

    public void Init(TileClass tile, bool isNaturallyPlaced)
    {
        tileName = tile.tileName;
        wallVariant = tile.wallVariant;
        tileSprites = tile.tileSprites;
        isBgElement = tile.isBgElement;
        toolToBreak = tile.toolToBreak;
        tileDrop = tile.tileDrop;
        isStackable = tile.isStackable;
        naturallyPlaced = isNaturallyPlaced;
    }
}