using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shop : MonoBehaviour {

    public UIGrid itemGrid;
    public GameObject shopItemPrefab;

    public void DisplayItems(string category)
    {
        // clear existing items in shop grid first
        foreach (Transform oldItem in itemGrid.transform)
            Destroy(oldItem.gameObject);

        // get the correct list
        List<ItemManager.Item> newItems = new List<ItemManager.Item>();
        if (category == "weapon")
            newItems.AddRange(ItemManager.Instance.availableWeapons);
        else if (category == "armour")
            newItems.AddRange(ItemManager.Instance.availableArmour);

        // fill the item prefabs into the shop
        foreach (ItemManager.Item item in newItems)
        {
            GameObject i = (GameObject)Instantiate(shopItemPrefab, Vector3.zero, Quaternion.identity);
            i.transform.Find("name_label").GetComponent<UILabel>().text = item.itemName;
            i.transform.Find("price_label").GetComponent<UILabel>().text = "$" +item.itemPrice.ToString();
            i.transform.parent = itemGrid.transform;
            i.transform.localScale = Vector3.one;
        }

        // refresh the UIGrid
        itemGrid.repositionNow = true;
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }

}
