using UnityEngine;
using System.Collections;

public class Item {

    public string itemName;
    public int itemPrice;
}

public class Weapon : Item
{
    public float damage;
    public float attackSpeed;
    public float hitChance;
}

public class Armour : Item
{
    public float damageReduction;
    public float blockChance;
    public float weight;
}
