using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using KGF;

// a Player class, extended from the base Character. Includes stamina/rest management,
// click movement, inventory
//[RequireComponent(typeof(PlayerCamera), typeof(PlayerUI), typeof(Movement))]
public class Player : Character {

    public Tile clickedOnTile;

    // stamina
    public float defaultStaminaRefreshRate = 0.10f;

    // player camera scripts
    public PlayerUI playerUI;
    public PlayerCamera playerCamera;
    public AudioListener audioListener;
    //public KGFOrbitCam orbitCam;

    // player inventory
    private PlayerInventory _inventory;

    // packmule vars
    //public Vector3 initialSpawnPosition;
    public GameObject packmulePrefab;
    public List<Packmule> currentPackmules = new List<Packmule>();
    public int maxPackmules = 2;
    public int packmulesWaiting;
    public float packmuleResourceGatheringTime = 5.0f;
    
    void Awake()
    {
        playerCamera = GetComponent<PlayerCamera>();
        //orbitCam = _playerCamera.playerMainCamera.GetComponent<KGFOrbitCam>();
        playerUI = GetComponent<PlayerUI>();
        _inventory = GetComponent<PlayerInventory>();
        combat = GetComponent<Combat>();
        packmulesWaiting = maxPackmules;
        networkCharacter = transform.root.GetComponent<NetworkCharacter>();
    }

    void Start()
    {
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
#endregion

    public bool IsInHomeTile { get { return closestTile.x == -1; } }

    IEnumerator WaitForMountainAndGo()
    {
        while (Mountain.Instance == null)
            yield return null;

        while (!Mountain.Instance.mountainGenerationComplete)
            yield return null;

        while (currentFace == "")
            yield return null;

        // set player to correct starting tile and camera rotation before fading in
        characterModel.transform.position = Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[currentFace]).tileTransform.position;
        //orbitCam.itsTarget.itsTarget = Mountain.Instance.cameraTarget;
        //Mountain.Instance.SetPlayerFaceFocus(this, _homeFace);
        playerCamera.SetStartFaceRotation(homeFace);

        // wait an extra time so all pathing is hidden
        yield return new WaitForSeconds(2.50f);

        playerUI.fader.FadeToClear();

        Mountain.Instance.SetFaceVisibility(currentFace);
        closestTile = Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[currentFace]);
    }

    void Update()
    {
        // remember to call the base class update
        base.Update();

        // stamina / rest / life updates
        playerUI.turnImage.fillAmount = (_turnTimer * 1000) / restInMilliseconds;
        playerUI.staminaImage.fillAmount += Time.deltaTime * GetCurrentStaminaRefreshRate();
        playerUI.lifeImage.fillAmount = combat.CurrentLife / combat.maxLife;

        // current cash updates
        playerUI.currentCash.text = _inventory.currentCash.ToString();

#region PLAYER_CLICKING
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            bool leftClick = Input.GetMouseButtonDown(0);
            bool rightClick = Input.GetMouseButtonDown(1);

            // clear any actions if we direct the player elsewhere
            if (leftClick && actionToProcess != ActionType.none)
                actionToProcess = ActionType.none;

            Ray ray = playerCamera.playerMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // LEFT CLICKED THE SHOP
                //if (hit.collider.tag == "ItemShop" && leftClick)
                //    playerUI.ShopClicked();

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
                //else
                //{
                    // don't accept movement clicks when shop is open
                    //if (playerUI.shop.gameObject.activeSelf)
                    //    return;

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
                            if (hit.collider.tag == "Monster")
                            {
                                Monster monster = hit.collider.GetComponent<Monster>();
                                if (monster != null)
                                {
                                    if (monster.isDead)
                                    {
                                        Tile muleTarget = monster.closestTile;
                                        List<Tile> pathFound = Mountain.Instance.GetPathBetweenTiles(currentFace, Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]), muleTarget);
                                        if (pathFound != null)
                                            SpawnPackmule(muleTarget, Packmule.MuleType.CorpseCollector);
                                    }
                                }
                            }
                            else
                            {
                                Tile muleTarget = Mountain.Instance.GetClosestTile(currentFace, hit.point);
                                List<Tile> pathFound = Mountain.Instance.GetPathBetweenTiles(currentFace, Mountain.Instance.GetStartPositionTile(Mountain.Instance.faces[currentFace]), muleTarget);
                                if (pathFound != null)
                                    SpawnPackmule(muleTarget);
                            }
                        }
                    }
                //}
            }
        }
#endregion

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
        _inventory.currentCash += amount;
    }

    Packmule SpawnPackmule(Tile target, Packmule.MuleType forcedMuleType = Packmule.MuleType.Null)
    {
        packmulesWaiting--;
        GameObject mule = (GameObject)Instantiate(packmulePrefab, Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[homeFace]).tileTransform.position, Quaternion.identity);
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