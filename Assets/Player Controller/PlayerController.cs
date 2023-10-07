using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public LayerMask layerMask;

    public int selectedSlotIndex = 0;
    public GameObject hotBarSelector;
    public GameObject handHolder;

    public Inventory inventory;
    public bool inventoryShowing = false;

    public ItemClass selectedItem;

    public int playerRange;
    public Vector2Int mousePos;

    public float moveSpeed;
    public float jumpForce;
    public bool onGround;

    public float horizontal;
    public bool hit;
    public bool place;

    private Rigidbody2D rb;
    private Animator anim;

    [HideInInspector]
    public Vector2 spawnPos;
    public TerrainGeneration terrainGenerator;


    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        
    }
    public void Spawn()
    {
        GetComponent<Transform>().position = spawnPos;

    }

    

    private void FixedUpdate()
    {
        //do stuff
        float jump = Input.GetAxisRaw("Jump");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2 (horizontal * moveSpeed, rb.velocity.y);

        if (horizontal > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);


        //jumping
        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
            movement.y = jumpForce;
        }

        //auto jump
        if (FootRaycast() && !HeadRaycast() && movement.x != 0)
        {
            if (onGround)
                movement.y = jumpForce * 0.8f; //jump multiplier for auto  jumping
        }

        rb.velocity = movement;
    }

    private void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        hit = Input.GetMouseButtonDown(0);
        place = Input.GetMouseButton(1);


        //scroll throught hotbar UI
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //scroll up
            if (selectedSlotIndex < inventory.inventoryWidth - 1)
                selectedSlotIndex += 1;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //scroll down
            if (selectedSlotIndex > 0)
                selectedSlotIndex -= 1;
        }

        //set selected slot UI
        hotBarSelector.transform.position = inventory.hotbarUISlots[selectedSlotIndex].transform.position;
        if (selectedItem != null)
        {
            handHolder.GetComponent<SpriteRenderer>().sprite = selectedItem.sprite;
            if (selectedItem.itemType == ItemClass.ItemType.block)
                handHolder.transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            else
                handHolder.transform.localScale = new Vector3(-1, 1, 1);

        }
        else
            handHolder.GetComponent<SpriteRenderer>().sprite = null;

        //set selected item
        if (inventory.inventorySlots[selectedSlotIndex, inventory.inventoryHeight - 1] != null)
        {
            selectedItem = inventory.inventorySlots[selectedSlotIndex, inventory.inventoryHeight - 1].item;
        }
        else
        {
            selectedItem = null;
        }
            
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryShowing = !inventoryShowing;
        }

        if (Vector2.Distance(transform.position, mousePos) <= playerRange &&
            Vector2.Distance(transform.position, mousePos) > 1f)
        {
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
        }

        if (Vector2.Distance(transform.position, mousePos) <= playerRange)
        {
            if (hit)
            {
                //terrainGenerator.RemoveTile(mousePos.x, mousePos.y);
                terrainGenerator.BreakTile(mousePos.x, mousePos.y, selectedItem);
            }

        }
        // set mouse pos
        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);

        inventory.inventoryUI.SetActive(inventoryShowing);

        anim.SetFloat("horizontal", horizontal);
        anim.SetBool("hit", hit || place);
    }

    

    public bool FootRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 1f/*ray length*/, layerMask);
        return hit;

    }
    public bool HeadRaycast()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3.up * 0.5f), -Vector2.right * transform.localScale.x, 1f, layerMask);
        return hit;
    }
}
