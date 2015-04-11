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
    //private Inventory _inventory;
    //public Inventory GetInventory { get { return _inventory; } }

    private InvGameItem _selectedItem;
    private UIWidget _selectedItemBackground;

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
        //_inventory = GetComponent<Inventory>();
        _initialSpawnPosition = transform.position;
        packmulesWaiting = maxPackmules;
        // let the mountain generate before beginning
        StartCoroutine(WaitForMountainAndGo());

        VectorLine.canvas3D.gameObject.layer = LayerMask.NameToLayer("VectorLines");
    }

#region OVERRIDES
    public override bool ShouldDisplayPaths()
    {
        if (playerUI.shop.gameObject.activeSelf)
            return false;

        return true;
    }

    //public override void UseItem()
    //{
    //    // check if selected item is this one, and if so clear it

    //    itemSlotToUse.Replace(null);
    //    itemSlotToUse.background.color = Color.black;
    //}
#endregion

    //public void SelectOrDeselectItem(InvGameItem item, UIWidget background, UIItemSlot slot)
    //{
    //    if (_selectedItem == item || item == null)
    //    {
    //        if (_selectedItemBackground != null)
    //            _selectedItemBackground.color = Color.black;
    //        _selectedItem = null;
    //        _selectedItemBackground = null;
    //    }
    //    // new selection
    //    else if (_selectedItem == null || _selectedItem != item)
    //    {
    //        if (_selectedItemBackground != null)
    //            _selectedItemBackground.color = Color.black;
    //        _selectedItem = item;
    //        _selectedItemBackground = background;
    //        _selectedItemBackground.color = Color.cyan;
    //        itemSlotToUse = slot;
    //    }
    //}

    //public bool PurchaseItem(InvGameItem item)
    //{
    //    // if we have enough funds...
    //    if (_inventory.currentCash >= item.baseItem.cost)
    //    {
    //        // try add to item storage - will return false if no room
    //        bool successfullyAdded = playerUI.inBaseInventory.AddItemToStorage(item);
    //        if (successfullyAdded)
    //        {
    //            // minus the money
    //            _inventory.currentCash -= item.baseItem.cost;
    //            return true;
    //        }
    //    }
    //    return false;
    //}

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
        //playerUI.restBar.value = (_turnTimer * 1000) / restInMilliseconds;
        //playerUI.staminaBar.value += Time.deltaTime * GetCurrentStaminaRefreshRate();
        playerUI.turnImage.fillAmount = (_turnTimer * 1000) / restInMilliseconds;
        playerUI.staminaImage.fillAmount += Time.deltaTime * GetCurrentStaminaRefreshRate();

        // current cash updates
        //playerUI.currentCashLabel.text = _inventory.currentCash.ToString();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            bool leftClick = Input.GetMouseButtonDown(0);
            bool rightClick = Input.GetMouseButtonDown(1);

            // clear any actions if we direct the player elsewhere
            if (leftClick && actionToProcess != ActionType.none)
            {
                actionToProcess = ActionType.none;
                //objectToAction = null;
            }

            Ray ray = _playerCamera.playerMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // LEFT CLICKED THE SHOP
                if (hit.collider.tag == "ItemShop" && leftClick)
                    playerUI.ShopClicked();

                // CLICKED SOME BLOCKAGE
                //else if (hit.collider.tag == "Blockage" && _selectedItem != null && _selectedItem.baseItem.name == "spade" &&
                //    (rightClick || (leftClick && _selectedItem.location == UIItemStorage.Location.OnPerson)))
                //{

                    //Blockage blockage = hit.collider.GetComponent<Blockage>();

                    //// if we AREN'T there, then we should pick the tile with the blockage which has the least amount of steps
                    //List<Tile> pathToTileOne = Mountain.Instance.GetPathBetweenTiles(currentFace, leftClick ? closestTile : Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]), blockage.tileOne);
                    //List<Tile> pathToTileTwo = Mountain.Instance.GetPathBetweenTiles(currentFace, leftClick ? closestTile : Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]), blockage.tileTwo);
                    //Tile leastDistanceTile = pathToTileOne != null && pathToTileTwo != null && pathToTileTwo.Count < pathToTileOne.Count ? blockage.tileTwo : blockage.tileOne;

                    //if (leftClick)
                    //{
                    //    Tile currentTarget = movement.GetTargetTile();

                    //    objectToAction = blockage.gameObject;
                    //    _selectedItemBackground.color = Color.blue;
                        
                    //    // if we are not moving and are already on the correct tile
                    //    if (!movement.isMoving && closestTile == leastDistanceTile)
                    //    {
                    //        // clear any existing paths before starting to clear blockage
                    //        movement.ClearPaths(false);
                    //        GoalReached();
                    //    }

                    //    // else if our current tile target is NOT the correct tile
                    //    else if (currentTarget != leastDistanceTile)
                    //    {
                    //        clickedOnTile = leastDistanceTile;
                    //        if (clickedOnTile != null)
                    //            movement.ClickedOnTile(clickedOnTile);
                    //    }

                    //}
                    //else if (rightClick && packmulesWaiting > 0)
                    //{
                    //    Packmule packmule = SpawnPackmule(leastDistanceTile, Packmule.MuleType.BlockageClearer);
                    //    packmule.objectToAction = blockage.gameObject;
                    //}
                //}

                // CLICKED ANYTHING ELSE
                else
                {
                    // don't accept movement clicks when shop is open
                    if (playerUI.shop.gameObject.activeSelf)
                        return;

                    if (leftClick)
                    {
                        clickedOnTile = Mountain.Instance.GetClosestTile(currentFace, hit.point);

                        // we're already moving to the clicked on tile
                        if (clickedOnTile != movement.GetTargetTile())
                        {
                            if (clickedOnTile == closestTile && !movement.isMoving)
                            {
                                // we're already on the correct tile
                                movement.ClearPaths(true);
                                return;
                            }
                            else if (clickedOnTile == movement.GetNextTile())
                            {
                                // we're almost there - remove any tiles past the next one
                                movement.SetNextTileAsTarget();
                                return;
                            }
                        }

                        // pass to the movement script
                        movement.ClickedOnTile(clickedOnTile);
                    }
                    else if (rightClick)
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

    public override void GoalReached()
    {
        //if (objectToAction != null)
        //{
        //    // check if the object is a blockage - clear it
        //    if (objectToAction.GetComponent<Blockage>() != null)
        //    {
        //        actionToProcess = ActionType.clearBlockage;
        //    }
        //}
    }

    public void DepositCash(int amount)
    {
        //_inventory.currentCash += amount;
    }

    public void ChangeFaceFocus(int toRotate)
    {
        _playerCamera.ChangeFaceFocus(toRotate);
    }

    Packmule SpawnPackmule(Tile target, Packmule.MuleType forcedMuleType = Packmule.MuleType.Null)
    {
        packmulesWaiting--;
        GameObject mule = (GameObject)Instantiate(packmulePrefab, _initialSpawnPosition, Quaternion.identity);
        Packmule packmule = mule.GetComponent<Packmule>();
        packmule.currentFace = this.currentFace;
        packmule.closestTile = null;
        if (forcedMuleType == Packmule.MuleType.Null)
            packmule.SetMuleType(this, target, target.revealed ? Packmule.MuleType.Null : Packmule.MuleType.Explorer);
        else
            packmule.SetMuleType(this, target, forcedMuleType);
        packmule.movement.ClickedOnTile(target);
        currentPackmules.Add(packmule);
        return packmule;
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