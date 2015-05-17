using UnityEngine;
using System.Collections;
using Vectrosity;
using UnityEngine.UI;

//[RequireComponent(typeof(Player))]
public class PlayerUI : MonoBehaviour {

    public Camera playerUICamera;
    public UIProgressBar staminaBar;
    public UIProgressBar restBar;

    public GameObject changeFaceLeftButton;
    public GameObject changeFaceRightButton;

    public Shop shop;
    public UILabel currentCashLabel;

    public AudioSource uiSfx;
    public AudioClip equipWeaponClip;

    public SceneFadeInOut fader;

    // progress bars
    public Image turnImage;
    public Image lifeImage;
    public Image staminaImage;

    // current money
    public Text currentCash;

    public void ShopClicked()
    {
        shop.gameObject.SetActive(!shop.gameObject.activeSelf);
        shop.UpdateItemDisplay("");
    }

    public void PlaySound(string category)
    {
        if (uiSfx == null)
            return;

        if (category == "equip weapon")
            uiSfx.PlayOneShot(equipWeaponClip);
    }
}
