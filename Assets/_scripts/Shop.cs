using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Shop : MonoBehaviour {

    public UIGrid itemGrid;
    public GameObject shopItemPrefab;
    private string _currentlyDisplayingCategory;

    //private List<GameObject> _currentDisplayedItems = new List<GameObject>();

    public Player player;

    //public void DisplayItems(string category)
    //{
        // clear existing items in shop grid first
        //foreach (Transform oldItem in itemGrid.transform)
        //    Destroy(oldItem.gameObject);
        //_currentDisplayedItems.Clear();

        //_currentlyDisplayingCategory = category;

        //// get the correct list
        //List<Item> newItems = new List<Item>();
        //if (category == "weapon")
        //    newItems.AddRange(ItemManager.Instance.availableWeapons);
        //else if (category == "armour")
        //    newItems.AddRange(ItemManager.Instance.availableArmour);

        //// fill the item prefabs into the shop
        //foreach (Item item in newItems)
        //{
        //    if ((LevelParameters.Instance.oneCopyOfEachItem && !item.sold) || !LevelParameters.Instance.oneCopyOfEachItem)
        //        CreateNewItemObject(item, true);
        //}

        //// refresh the UIGrid
        //itemGrid.repositionNow = true;
    //}

    //void CreateNewItemObject(Item item, bool inShop)
    //{
    //    GameObject i = (GameObject)Instantiate(shopItemPrefab, Vector3.zero, Quaternion.identity);
    //    i.name = item.itemName;
    //    i.transform.Find("name_label").GetComponent<UILabel>().text = item.itemName;
    //    i.transform.Find("price_label").GetComponent<UILabel>().text = "$" + item.itemPrice.ToString();
    //    i.transform.parent = itemGrid.transform;
    //    i.transform.localScale = Vector3.one;
        
    //    _currentDisplayedItems.Add(i);

    //    EventDelegate.Add(i.GetComponent<UIButton>().onClick, () => PurchaseItem(i.name));
    //}

    public void CloseShop()
    {
        gameObject.SetActive(false);
        VectorLine.canvas.gameObject.SetActive(true);
    }

    //GameObject FindItemInGrid(string itemName)
    //{
    //    foreach (GameObject obj in _currentDisplayedItems)
    //    {
    //        if (obj.name == itemName)
    //            return obj;
    //    }
    //    return null;
    //}

    //public void PurchaseItem(string itemName)
    //{
    //    //Item item = ItemManager.Instance.GetItemByName(itemName);
    //    GameObject itemGridObject = FindItemInGrid(itemName);
    //    if (itemGridObject != null)
    //    {
    //        Item item = ItemManager.Instance.GetItemByName(itemName);
    //        bool successfulPurchase = player.GetInventory.currentCash >= item.itemPrice;
    //        if (successfulPurchase)
    //        {
    //            // TODO: some kind of feedback for purchasing
    //            player.GetInventory.AddToInventory(item, player.IsInHomeTile);
    //            player.GetInventory.currentCash -= item.itemPrice;

    //            // check if we are playing with only one copy of each item
    //            if (LevelParameters.Instance.oneCopyOfEachItem)
    //            {
    //                ItemManager.Instance.SoldItem(item);
    //                DisplayItems(_currentlyDisplayingCategory);
    //            }
    //        }
    //        else
    //        {
    //            // TODO: handle not enough money... popup? sound? something?
    //        }
    //    }
    //}

}
