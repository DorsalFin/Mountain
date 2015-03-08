using UnityEngine;
using System.Collections;
using Vectrosity;

public class LevelParameters : MonoBehaviour {

    public static LevelParameters Instance;

    void Awake()
    {
        Instance = this;
        if (testingMode)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObj in players)
            {
                Player p = playerObj.GetComponent<Player>();
                p.speed = 1;
                p.restInMilliseconds = 1;
            }
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.V))
    //    {
    //        Mountain.Instance.SetFaceVisibility("all");
    //        Mountain.Instance.SetAllTilesDisplayedState(true);
    //    }
    //}

    /// <summary>
    /// turn this on to increase player movement speed and reduce rest for testing
    /// </summary>
    public bool testingMode = false;

    /// <summary>
    /// the amount of paths generated on tiles
    /// </summary>
    public float probabilityOfTilePaths = 50.0f;

    /// <summary>
    /// percentage value indicating the probability that tiles will have
    /// path connections with other tiles on a second pass
    /// </summary>
    public float extraProbabilityOfJoinedTiles = 50.0f;

    /// <summary>
    /// after the tile paths are generated, we add up the player's inital tile's open paths.
    /// if it is LESS than this number, we create open paths up until we reach this.
    /// </summary>
    public int guaranteedPathsFromFirstTile = 2;

    /// <summary>
    /// the number of paths that exist between each face and the next
    /// </summary>
    public int numPathsBetweenOneFaceAndAnother = 2;

    /// <summary>
    /// the distance at which a tile will be revealed
    /// </summary>
    public float minDistanceToRevealTile = 0.75f;
    /// <summary>
    /// the distance at whice a tile will be considered closest to the character
    /// </summary>
    public float distanceToBeConsideredClosest = 1.10f;

    /// <summary>
    /// the amount of cash a character starts the game with
    /// </summary>
    public int startingCash = 50;

    /// <summary>
    /// the amount of minerals a freshly spawned mineral tile holds
    /// </summary>
    public int initialMineralYield = 60;

    /// <summary>
    /// every 'this' seconds a mineral field refreshes it's yield by 1
    /// </summary>
    public int mineralRefreshNumSeconds = 10;

    /// <summary>
    /// this percentage of minerals is gathered by a mule each time they harvest a mineral
    /// tile and walk it back to base
    /// </summary>
    public int percentageMineralsDrainedEachHarvest = 40;

    /// <summary>
    /// whether there will be multiple copies of every item... having this 'false' means everyone
    /// can buy the same items versus each player having unique items
    /// </summary>
    public bool oneCopyOfEachItem = true;
}
