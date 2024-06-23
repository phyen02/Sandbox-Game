using System.Collections;
using UnityEngine;

[System.Serializable]
public class ItemClass
{
    public enum ItemType
    {
        tool,
        block
    };

    public enum ToolType
    {
        axe,
        pickaxe,
        hammer,
        none,
        unbreakable
    };

    public ItemType itemType;
    public ToolType toolType;

    public string itemName;
    public Sprite itemSprite;
    public bool isStackable;
    
    public ToolClass tool;
    public TileClass tile;

    public ItemClass (TileClass _tile)
    {
        itemName = _tile.tileName;
        itemSprite = _tile.tileDrop.tileSprites[0];
        isStackable = _tile.isStackable;
        itemType = ItemType.block;
        tile = _tile;
    }

    public ItemClass (ToolClass _tool)
    {
        itemName = _tool.toolName;
        itemSprite = _tool.toolSprite;
        isStackable = false;
        toolType = _tool.toolType;
        tool = _tool;
    }
}
