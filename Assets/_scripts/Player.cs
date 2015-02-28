using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

// a Player class, extended from the base Character. Includes stamina/rest management,
// click movement, inventory
[RequireComponent(typeof(PlayerCamera), typeof(PlayerUI), typeof(Movement))]
public class Player : Character {

    public Tile clickedOnTile;

    // stamina
    public float defaultStaminaRefreshRate = 0.10f;

    // player camera scripts
    private PlayerUI _playerUI;
    private PlayerCamera _playerCamera;

    // player inventory script
    private Inventory _inventory;

    // packmule vars
    private Vector3 _initialSpawnPosition;
    public GameObject packmulePrefab;
    public List<Packmule> currentPackmules = new List<Packmule>();
    public int maxPackmules = 2;
    public int packmulesWaiting;
    public float packmuleResourceGatheringTime = 5.0f;

    
    void Start()
    {
        _playerCamera = GetComponent<PlayerCamera>();
        _playerUI = GetComponent<PlayerUI>();
        _inventory = GetComponent<Inventory>();
        _initialSpawnPosition = transform.position;
        packmulesWaiting = maxPackmules;
        // let the mountain generate before beginning
        StartCoroutine(WaitForMountainAndGo());
    }

    IEnumerator WaitForMountainAndGo()
    {
        while (Mountain.Instance.mountainGenerationComplete)
            yield return null;

        Mountain.Instance.SetFaceVisibility(currentFace);
        closestTile = Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[currentFace]);
        movement.ClickedOnTile(Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]));
    }

    void Update()
    {
        // remember to call the base class update
        base.Update();

        // stamina / rest updates
        _playerUI.restBar.value = (_turnTimer * 1000) / restInMilliseconds;
        _playerUI.staminaBar.value += Time.deltaTime * GetCurrentStaminaRefreshRate();

        // current cash updates
        _playerUI.currentCashLabel.text = _inventory.currentCash.ToString();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            bool leftClick = Input.GetMouseButtonDown(0);

            Ray ray = _playerCamera.playerMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "ItemShop" && leftClick)
                    _playerUI.ShopClicked();
                else
                {
                    // don't accept movement clicks when shop is open
                    if (_playerUI.shop.gameObject.activeSelf)
                        return;

                    clickedOnTile = Mountain.Instance.GetClosestTile(currentFace, hit.point);

                    if (leftClick)
                    {
                        if (clickedOnTile == closestTile)
                            return;

                        // pass to the movement script
                        movement.ClickedOnTile(clickedOnTile);
                    }
                    else // rightClick
                    {
                        // send a packmule to the tile if we have one available
                        if (packmulesWaiting > 0)
                        {
                            List<Tile> pathFound = Mountain.Instance.GetPathBetweenTiles(currentFace, Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]), clickedOnTile);
                            if (pathFound != null)
                                SpawnPackmule();
                        }
                    }
                }
            }
        }

    }

    public void DepositCash(int amount)
    {
        _inventory.currentCash += amount;
    }

    public void ChangeFaceFocus(int toRotate)
    {
        _playerCamera.ChangeFaceFocus(toRotate);
    }

    void SpawnPackmule()
    {
        packmulesWaiting--;
        GameObject mule = (GameObject)Instantiate(packmulePrefab, _initialSpawnPosition, Quaternion.identity);
        Packmule packmule = mule.GetComponent<Packmule>();
        packmule.currentFace = this.currentFace;
        packmule.closestTile = null;
        packmule.SetMuleType(this, clickedOnTile, clickedOnTile.revealed ? Packmule.MuleType.Null : Packmule.MuleType.Explorer);
        packmule.movement.ClickedOnTile(clickedOnTile);
        currentPackmules.Add(packmule);
    }

    public void ToggleChangeFaceArrow(int direction, bool show)
    {
        if (direction == Mountain.RIGHT)
            _playerUI.changeFaceRightButton.SetActive(show);
        else if (direction == Mountain.LEFT)
            _playerUI.changeFaceLeftButton.SetActive(show);
    }

    float GetCurrentStaminaRefreshRate()
    {
        if (movement.isMoving)
            return defaultStaminaRefreshRate * 0.25f;

        // if none of the above conditions are true we just retrun default value
        return defaultStaminaRefreshRate;
    }
}