using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ItemManager : MonoBehaviour {

    public static ItemManager Instance;

    void Awake()
    {
        Instance = this;
    }

    private string _data;

    public List<Item> availableWeapons = new List<Item>();
    public List<Item> availableArmour = new List<Item>();

    void Start()
    {
        StreamReader reader = File.OpenText("Assets/item_txt_files/weapons.txt");
        while (!reader.EndOfStream)
        {
            string textLine = reader.ReadLine();
            // skip lines beginning with hash
            if (textLine[0] == '#')
                continue;

            Weapon weapon = new Weapon();
            string[] txtNodes = textLine.Split(':');
            for (int i = 0; i < txtNodes.Length; i++)
            {
                if (i == 0) weapon.itemName = txtNodes[i];
                else if (i == 1) int.TryParse(txtNodes[i], out weapon.itemPrice);
                else if (i == 2) float.TryParse(txtNodes[i], out weapon.attackSpeed);
                else if (i == 3) float.TryParse(txtNodes[i], out weapon.damage);
                else if (i == 4) float.TryParse(txtNodes[i], out weapon.hitChance);
            }
            availableWeapons.Add(weapon);
        }
        reader.Close();

        reader = File.OpenText("Assets/item_txt_files/armour.txt");
        while (!reader.EndOfStream)
        {
            string textLine = reader.ReadLine();
            // skip lines beginning with hash
            if (textLine[0] == '#')
                continue;

            Armour armour = new Armour();
            string[] txtNodes = textLine.Split(':');
            for (int i = 0; i < txtNodes.Length; i++)
            {
                if (i == 0) armour.itemName = txtNodes[i];
                else if (i == 1) int.TryParse(txtNodes[i], out armour.itemPrice);
                else if (i == 2) float.TryParse(txtNodes[i], out armour.damageReduction);
                else if (i == 3) float.TryParse(txtNodes[i], out armour.blockChance);
                else if (i == 4) float.TryParse(txtNodes[i], out armour.weight);
            }
            availableArmour.Add(armour);
        }
        reader.Close();
    }

    public void SoldItem(Item item)
    {
        item.sold = true;
        //List<Item> itemParentList = item is Weapon ? availableWeapons : (item is Armour ? availableArmour : null);
        //foreach (Item i in itemParentList)
        //{
        //    if (i == item)
        //    {
        //        itemParentList.Remove(item);
        //        break;
        //    }
        //}
    }

    public Item GetItemByName(string itemName)
    {
        Item itemFound = null;
        foreach (Item i in availableArmour)
        {
            if (i.itemName == itemName)
            {
                itemFound = i;
                break;
            }
        }
        if (itemFound == null)
        {
            foreach (Item i in availableWeapons)
            {
                if (i.itemName == itemName)
                {
                    itemFound = i;
                    break;
                }
            }
        }
        return itemFound;
    }

    public void PurchaseItem(Player player, Item item)
    {

    }
}
