using UnityEngine;
using System.Collections;
using Vectrosity;

[RequireComponent(typeof(Player))]
public class PlayerUI : MonoBehaviour {

    public Camera playerUICamera;
    public UIProgressBar staminaBar;
    public UIProgressBar restBar;

    public GameObject changeFaceLeftButton;
    public GameObject changeFaceRightButton;

    public Shop shop;
    public UILabel currentCashLabel;

    public void ShopClicked()
    {
        shop.gameObject.SetActive(!shop.gameObject.activeSelf);
        VectorLine.canvas.gameObject.SetActive(!shop.gameObject.activeSelf);
    }
}
