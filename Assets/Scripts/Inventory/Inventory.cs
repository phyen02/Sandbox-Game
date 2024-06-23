using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public int stackLimit = 64;
    public ToolClass pickaxe;
    public ToolClass axe;
    public ToolClass hammer;

    public Vector2 invenOffset;
    public Vector2 hotbarOffset;
    public Vector2 multiplier;
    public GameObject inventoryUI;
    public GameObject hotbarUI;
    public GameObject inventorySlotPrefab;
    public int inventoryWidth;
    public int inventoryHeight;
    public InventorySlot[,] inventorySlot;
    public GameObject[,] invenUIslot;
    public InventorySlot[] hotbarSlot;
    public GameObject[] hotbarUISlot;

    private void Start()
    {
        inventorySlot = new InventorySlot[inventoryWidth, inventoryHeight];
        invenUIslot = new GameObject[inventoryWidth, inventoryHeight];
        hotbarSlot = new InventorySlot[inventoryWidth];
        hotbarUISlot = new GameObject[inventoryWidth];

        SetupUI();
        UpdateInventoryUI();
        Add(new ItemClass(pickaxe));
        Add(new ItemClass(axe));
        Add(new ItemClass(hammer));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }
    }

    void SetupUI()
    {
        // Inventory
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                GameObject invenSlot = Instantiate(inventorySlotPrefab, inventoryUI.transform.GetChild(0).transform);
                invenSlot.GetComponent<RectTransform>().localPosition = new Vector3((x * multiplier.x) + invenOffset.x,
                                                                                    (y * multiplier.y) + invenOffset.y);
                invenUIslot[x, y] = invenSlot;
                inventorySlot[x, y] = null;
            }
        }

        // Hot bar
        for (int x = 0; x < inventoryWidth; x++)
        {
            GameObject hotbar_Slot = Instantiate(inventorySlotPrefab, hotbarUI.transform.GetChild(0).transform);
            hotbar_Slot.GetComponent<RectTransform>().localPosition = new Vector3((x * multiplier.x) + hotbarOffset.x,
                                                                                    hotbarOffset.y);
            hotbarUISlot[x] = hotbar_Slot;
            hotbarSlot[x] = null;
        }
    }

    void UpdateInventoryUI()
    {
        // Inventory
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                if (inventorySlot[x, y] == null)
                {
                    invenUIslot[x, y].transform.GetChild(0).GetComponent<Image>().sprite = null;
                    invenUIslot[x, y].transform.GetChild(0).GetComponent<Image>().enabled = false;

                    invenUIslot[x, y].transform.GetChild(1).GetComponent<Text>().text = "0";
                    invenUIslot[x, y].transform.GetChild(1).GetComponent<Text>().enabled = false;
                }
                else
                {
                    invenUIslot[x, y].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    invenUIslot[x, y].transform.GetChild(0).GetComponent<Image>().sprite = inventorySlot[x, y].item.itemSprite;

                    invenUIslot[x, y].transform.GetChild(1).GetComponent<Text>().text = inventorySlot[x, y].quantity.ToString();
                    invenUIslot[x, y].transform.GetChild(1).GetComponent<Text>().enabled = true;
                }
            }
        }

        // Hot bar
        for (int x = 0; x < inventoryWidth; x++)
        {
            if (inventorySlot[x, inventoryHeight - 1] == null)
            {
                hotbarUISlot[x].transform.GetChild(0).GetComponent<Image>().sprite = null;
                hotbarUISlot[x].transform.GetChild(0).GetComponent<Image>().enabled = false;

                hotbarUISlot[x].transform.GetChild(1).GetComponent<Text>().text = "0";
                hotbarUISlot[x].transform.GetChild(1).GetComponent<Text>().enabled = false;
            }
            else
            {
                hotbarUISlot[x].transform.GetChild(0).GetComponent<Image>().enabled = true;
                hotbarUISlot[x].transform.GetChild(0).GetComponent<Image>().sprite = inventorySlot[x, inventoryHeight - 1].item.itemSprite;

                hotbarUISlot[x].transform.GetChild(1).GetComponent<Text>().text = inventorySlot[x, inventoryHeight - 1].quantity.ToString();
                hotbarUISlot[x].transform.GetChild(1).GetComponent<Text>().enabled = true;
            }
        }
    }

    public bool Add(ItemClass item)
    {
        Vector2Int itemPos = Contains(item);
        bool added = false;

        if (itemPos != Vector2Int.one * -1)
        {
            inventorySlot[itemPos.x, itemPos.y].quantity += 1;
            added = true;
        }

        if (!added)
        {
            for (int y = inventoryHeight - 1; y >= 0; y--)
            {
                if (added)
                    break;

                for (int x = 0; x < inventoryWidth; x++)
                {
                    if (inventorySlot[x, y] == null)
                    {
                        inventorySlot[x, y] = new InventorySlot { item = item, slot = new Vector2Int(x, y), quantity = 1 };
                        added = true;
                        break;
                    }
                    else
                    {
                        if (inventorySlot[x, y].item.itemName == item.itemName)
                        {
                            inventorySlot[x, y].quantity += 1;
                            added = true;
                            break;
                        }
                    }
                }
            }
        }

        UpdateInventoryUI();
        return added;
    }

    public Vector2Int Contains(ItemClass item)
    {
        for (int y = inventoryHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                if (inventorySlot[x, y] != null)
                {
                    if (inventorySlot[x, y].item.itemName == item.itemName)
                    {
                        if (item.isStackable && inventorySlot[x, y].quantity < stackLimit)
                            return new Vector2Int(x, y);
                    }
                }
            }
        }

        return Vector2Int.one * -1;
    }

    public bool Remove(ItemClass item)
    {
        for (int y = inventoryHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                if (inventorySlot[x, y] != null)
                {
                    if (inventorySlot[x, y].item.itemName == item.itemName)
                    {
                        inventorySlot[x, y].quantity -= 1;

                        if (inventorySlot[x, y].quantity == 0)
                        {
                            inventorySlot[x, y] = null;
                        }

                        UpdateInventoryUI();
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
