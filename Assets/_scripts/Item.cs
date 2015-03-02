using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class Item
{
    public bool sold;
    public string itemName;
    public int itemPrice;

    //public abstract Item CopyItem(Item item);
}

[System.Serializable]
public class Weapon : Item
{
    public float damage;
    public float attackSpeed;
    public float hitChance;

    //public override Item CopyItem(Item item)
    //{
    //    Weapon clone = (Weapon)item;
    //    return clone;
    //}
}

[System.Serializable]
public class Armour : Item
{
    public float damageReduction;
    public float blockChance;
    public float weight;

    //public override Item CopyItem(Item item)
    //{
    //    Armour clone = (Armour)item;
    //    return clone;
    //}

}