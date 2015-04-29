using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    public string itemName;                                     //itemName of the item
    public int itemID;                                          //itemID of the item
    public string itemDesc;                                     //itemDesc of the item
    public Sprite itemIcon;                                     //itemIcon of the item
    public GameObject itemModel;                                //itemModel of the item
    public int itemValue = 1;                                   //itemValue is at start 1
    public ItemType itemType;                                   //itemType of the Item
    public float itemWeight;                                    //itemWeight of the item
    public int maxStack = 1;
    public int indexItemInList = 999;    
    public int rarity;

    public AudioClip hitAudio;
    public AudioClip missAudio;

    public Monster monsterReference;

    [SerializeField]
    public List<ItemAttribute> itemAttributes = new List<ItemAttribute>();    
    
    public Item(){}

    public Item(string name, int id, string desc, Sprite icon, GameObject model, int maxStack, ItemType type, string sendmessagetext, List<ItemAttribute> itemAttributes)                 //function to create a instance of the Item
    {
        itemName = name;
        itemID = id;
        itemDesc = desc;
        itemIcon = icon;
        itemModel = model;
        itemType = type;
        this.maxStack = maxStack;
        this.itemAttributes = itemAttributes;
    }

    public Item getCopy()
    {
        return (Item)this.MemberwiseClone();        
    }

    /// <summary>
    /// Item is considered a shield if it has no attack speed
    /// and a block value
    /// </summary>
    public bool IsShield()
    {
        bool hasAtkSpeed = GetValueFromAttributes("attack speed") != 0;
        bool blockFound = GetValueFromAttributes("block chance") != 0;

        if (!hasAtkSpeed && blockFound)
            return true;
        else
            return false;
    }

    public float GetValueFromAttributes(string attribute)
    {
        foreach (ItemAttribute att in itemAttributes)
        {
            if (att.attributeName == attribute)
                return att.attributeValue;
        }
        return 0;
    }

    public void SetValueInAttributes(string attribute, float addition)
    {
        foreach (ItemAttribute att in itemAttributes)
        {
            if (att.attributeName == attribute)
                att.attributeValue += addition;
        }
    }
}


