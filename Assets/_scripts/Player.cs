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
    public PlayerUI playerUI;
    private PlayerCamera _playerCamera;

    // player inventory script
    private Inventory _inventory;
    public Inventory GetInventory { get { return _inventory; } }


    private InvGameItem _selectedItem;
    private UIWidget _selectedItemBackground;
    public void SelectOrDeselectItem(InvGameItem item, UIWidget background)
    {
        if (_selectedItem == item || item == null)
        {
            if (_selectedItemBackground != null)
                _selectedItemBackground.color = Color.black;
            _selectedItem = null;
            _selectedItemBackground = null;
        }
        // new selection
        else if (_selectedItem == null || _selectedItem != item)
        {
            if (_selectedItemBackground != null)
                _selectedItemBackground.color = Color.black;
            _selectedItem = item;
            _selectedItemBackground = background;
            _selectedItemBackground.color = Color.cyan;
        }
    }

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
        playerUI = GetComponent<PlayerUI>();
        _inventory = GetComponent<Inventory>();
        _initialSpawnPosition = transform.position;
        packmulesWaiting = maxPackmules;
        // let the mountain generate before beginning
        StartCoroutine(WaitForMountainAndGo());

        VectorLine.canvas3D.gameObject.layer = LayerMask.NameToLayer("VectorLines");
    }

    public override bool ShouldDisplayPaths()
    {
        if (playerUI.shop.gameObject.activeSelf)
            return false;

        return true;
    }

    public bool PurchaseItem(InvGameItem item)
    {
        // if we have enough funds...
        if (_inventory.currentCash >= item.baseItem.cost)
        {
            // try add to item storage - will return false if no room
            bool successfullyAdded = playerUI.inBaseInventory.AddItemToStorage(item);
            if (successfullyAdded)
            {
                // minus the money
                _inventory.currentCash -= item.baseItem.cost;
                return true;
            }
        }
        return false;
    }

    public bool IsInHomeTile { get { return closestTile.x == -1; } }

    IEnumerator WaitForMountainAndGo()
    {
        while (Mountain.Instance.mountainGenerationComplete)
            yield return null;

        Mountain.Instance.SetFaceVisibility(currentFace);
        closestTile = Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[currentFace]);
        //movement.ClickedOnTile(Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]));
    }

    void Update()
    {
        // remember to call the base class update
        base.Update();

        // stamina / rest updates
        playerUI.restBar.value = (_turnTimer * 1000) / restInMilliseconds;
        playerUI.staminaBar.value += Time.deltaTime * GetCurrentStaminaRefreshRate();

        // current cash updates
        playerUI.currentCashLabel.text = _inventory.currentCash.ToString();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            bool leftClick = Input.GetMouseButtonDown(0);

            Ray ray = _playerCamera.playerMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "ItemShop" && leftClick)
                    playerUI.ShopClicked();
                else
                {
                    // don't accept movement clicks when shop is open
                    if (playerUI.shop.gameObject.activeSelf)
                        return;

                    if (leftClick)
                    {
                        clickedOnTile = Mountain.Instance.GetClosestTile(currentFace, hit.point);

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
                            Tile muleTarget = Mountain.Instance.GetClosestTile(currentFace, hit.point);
                            List<Tile> pathFound = Mountain.Instance.GetPathBetweenTiles(currentFace, Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]), muleTarget);
                            if (pathFound != null)
                                SpawnPackmule(muleTarget);
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

    void SpawnPackmule(Tile target)
    {
        packmulesWaiting--;
        GameObject mule = (GameObject)Instantiate(packmulePrefab, _initialSpawnPosition, Quaternion.identity);
        Packmule packmule = mule.GetComponent<Packmule>();
        packmule.currentFace = this.currentFace;
        packmule.closestTile = null;
        packmule.SetMuleType(this, target, target.revealed ? Packmule.MuleType.Null : Packmule.MuleType.Explorer);
        packmule.movement.ClickedOnTile(target);
        currentPackmules.Add(packmule);
    }

    public void ToggleChangeFaceArrow(int direction, bool show)
    {
        if (direction == Mountain.RIGHT)
            playerUI.changeFaceRightButton.SetActive(show);
        else if (direction == Mountain.LEFT)
            playerUI.changeFaceLeftButton.SetActive(show);
    }

    float GetCurrentStaminaRefreshRate()
    {
        if (movement.isMoving)
            return defaultStaminaRefreshRate * 0.25f;

        // if none of the above conditions are true we just retrun default value
        return defaultStaminaRefreshRate;
    }
}