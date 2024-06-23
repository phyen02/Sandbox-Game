using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public LayerMask layerMask;

    public int selectedSlot = 0;
    public GameObject hotbarSelector;
    public GameObject handHolder;

    public Inventory inventory;
    public bool inventoryShowed = false;
    public bool hotbarShowed = true;
    public bool toolShowed = true;

    public ItemClass selectedItem;

    public int playerRange;
    public Vector2Int mousePos;

    public float moveSpeed;
    public float jumpForce;
    public bool onGround;

    private Rigidbody2D rigid;
    private Animator animator;

    public bool mining;
    public bool chopping;
    public bool place;
    private float horizontal;
    private float vertical;
    private float jump;
    AudioManager audioManager;
    public GameObject footstep;
    public GameObject digging;

    [HideInInspector]
    public Vector3 spawnPos;
    public TerrainGeneration terrainGenerator;


    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        footstep.SetActive(false);
        digging.SetActive(false);
    }

    public void Spawn()
    {
        GetComponent<Transform>().position = spawnPos;
        
    }

    private void FixedUpdate()
    {
        vertical = Input.GetAxis("Vertical");
        jump = Input.GetAxis("Jump");

        Vector2 movement = new Vector2(horizontal * moveSpeed, rigid.velocity.y);

        // NOTE: Run
        if (horizontal > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // NOTE: Jump
        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
            {
                movement.y = jumpForce;
                audioManager.PlaySFX(audioManager.jump);
            }
        }

        // NOTE: Autojump
        if (FootRayCast() && !HeadRayCast() && movement.x != 0)
        {
            if (onGround)
            {
                movement.y = jumpForce * 0.6f;
            }
        }

        rigid.velocity = movement;
    }

    private void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        mining = Input.GetMouseButton(0);
        chopping = Input.GetMouseButton(0);
        place = Input.GetMouseButtonDown(1);

        if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0)
        {
            Footsteps();
        }
        else
        {
            StopFootsteps();
        }

        // NOTE Hotbar Scroll
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (selectedSlot < inventory.inventoryWidth - 1)
            {
                selectedSlot += 1;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (selectedSlot > 0)
            {
                selectedSlot -= 1;
            }
        }

        // NOTE Display item
        hotbarSelector.transform.position = inventory.hotbarUISlot[selectedSlot].transform.position;
        if (selectedItem != null && selectedItem.itemType == ItemClass.ItemType.block)
        {
            handHolder.GetComponent<SpriteRenderer>().sprite = selectedItem.itemSprite;
        }
        else
        {
            handHolder.GetComponent<SpriteRenderer>().sprite = null;
        }

        if (inventory.inventorySlot[selectedSlot, inventory.inventoryHeight - 1] != null)
        {
            selectedItem = inventory.inventorySlot[selectedSlot, inventory.inventoryHeight - 1].item;
        }
        else
        {
            selectedItem = null;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryShowed = !inventoryShowed;
            hotbarShowed = !hotbarShowed;
        }

        // NOTE Place block
        if (Vector2.Distance(transform.position, mousePos) <= playerRange &&
            Vector2.Distance(transform.position, mousePos) > 0.5f)
        {
            if (place)
            {
                if (selectedItem != null)
                {
                    if (selectedItem.itemType == ItemClass.ItemType.block)
                    {
                        if (terrainGenerator.CheckTile(selectedItem.tile, mousePos.x, mousePos.y, false))
                            inventory.Remove(selectedItem);
                    }
                }
            }
        }

        // NOTE Tools interacts
        if (Vector2.Distance(transform.position, mousePos) <= playerRange)
        {
            if (mining)
            {
                terrainGenerator.toolBreakTile(mousePos.x, mousePos.y, selectedItem);
            }
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            PlayMining();
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            StopMining();
        }

        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);

        inventory.inventoryUI.SetActive(inventoryShowed);
        inventory.hotbarUI.SetActive(hotbarShowed);
        handHolder.SetActive(toolShowed);

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        if (selectedItem != null && selectedItem.itemType == ItemClass.ItemType.tool && Vector2.Distance(transform.position, mousePos) <= playerRange)
        {
            animator.SetBool("Mining", mining);
        }
    }

    // void OnValidate()
    // {
    //     Debug.DrawRay(transform.position - (Vector3.up * -0.5f), Vector2.right * transform.localScale.x, Color.white, layerMask);
    //     Debug.DrawRay(transform.position + (Vector3.up * 1.5f), Vector2.right * transform.localScale.x, Color.green, layerMask);
    // }

    // NOTE Setup Raycast
    public bool FootRayCast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - (Vector3.up * -0.5f), Vector2.right * transform.localScale.x, 1f, layerMask);
        return hit;
    }

    public bool HeadRayCast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3.up * 1.5f), Vector2.right * transform.localScale.x, 1f, layerMask);

        return hit;
    }

    // NOTE SFX Settings
    public void Footsteps()
    {
        footstep.SetActive(true);
    }

    public void StopFootsteps()
    {
        footstep.SetActive(false);
    }

    public void PlayMining()
    {
        digging.SetActive(true);
    }

    public void StopMining()
    {
        digging.SetActive(false);
    }
}
