using UnityEngine;
using System.Collections;
using Vectrosity;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class PlayerUI : MonoBehaviour {

    public Camera playerUICamera;
    public UIProgressBar staminaBar;
    public UIProgressBar restBar;

    public GameObject changeFaceLeftButton;
    public GameObject changeFaceRightButton;

    //public GameObject onPlayerInventory;
    //public UIItemStorage inBaseInventory;

    public Shop shop;
    public UILabel currentCashLabel;

    public Image turnImage;
    public Image lifeImage;
    public Image staminaImage;

    public void ShopClicked()
    {
        shop.gameObject.SetActive(!shop.gameObject.activeSelf);
        shop.UpdateItemDisplay("");
    }


}
