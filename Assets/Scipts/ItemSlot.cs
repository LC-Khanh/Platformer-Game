using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //=======ITEM DATA=======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField]
    private int maxNumberOfItems;

    //=======ITEM SLOT=======//
    [SerializeField]
    private TMP_Text quantityTxt;

    [SerializeField]
    private Image itemImage;

    //=======ITEM DESCRIPTION SLOT=======//
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameTxt;
    public TMP_Text itemDescriptionTxt;

    public GameObject selectedShader;
    public bool thisItemSelected;

    InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    public int AdddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        // Check if full
        if (isFull)
            return quantity;

        // Update Name
        this.itemName = itemName;
        
        // Update Image
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        // Update Description
        this.itemDescription = itemDescription;

        // Update Quantity
        this.quantity += quantity;
        if (this.quantity >= maxNumberOfItems)
        {
            quantityTxt.text = quantity.ToString();
            quantityTxt.enabled = true;
            isFull = true;

            // Return the LeftOVerItems
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        // Update Quantity Txt
        quantityTxt.text = this.quantity.ToString();
        quantityTxt.enabled = true;

        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    private void OnRightClick()
    {
        GameObject itemToDrop = new GameObject(itemName);
        Item newItem = itemToDrop.AddComponent<Item>();
        newItem.quantity = 1;
        newItem.itemName = itemName;
        newItem.sprite = itemSprite;
        newItem.itemDescription = itemDescription;

        SpriteRenderer sr = itemToDrop.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingOrder = 5;
        sr.sortingLayerName = "Ground";

        itemToDrop.AddComponent<BoxCollider2D>();

        itemToDrop.transform.position = GameObject.FindWithTag("Player").transform.position + new Vector3(12, 0, 0);
        itemToDrop.transform.localScale = new Vector3(.5f, .5f, .5f);

        this.quantity -= 1;
        quantityTxt.text = this.quantity.ToString();
        if (this.quantity <= 0)
            EmptySlot();
    }

    private void OnLeftClick()
    {
        if (thisItemSelected)
        {
            bool usable = inventoryManager.UseITem(itemName);
            if (usable)
            {
                this.quantity -= 1;
                quantityTxt.text = this.quantity.ToString();
                if (this.quantity <= 0)
                    EmptySlot();
            }

        }
        else
        {

            inventoryManager.DeselectedAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            itemDescriptionNameTxt.text = itemName;
            itemDescriptionTxt.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;

            if (itemDescriptionImage.sprite == null)
                itemDescriptionImage.sprite = emptySprite;
        }
    }

    private void EmptySlot()
    {
        quantityTxt.enabled = false;
        itemImage.sprite = emptySprite;

        itemDescriptionNameTxt.text = " ";
        itemDescriptionTxt.text = " ";
        itemDescriptionImage.sprite = emptySprite;
    }
}
