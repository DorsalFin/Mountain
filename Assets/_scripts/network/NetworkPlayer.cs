using UnityEngine;
using System.Collections;

public class NetworkPlayer : MonoBehaviour {

    public Player player;

    public GameObject[] objectsToEnableForOwned;

    public void ActivateThisPlayer()
    {
        foreach (GameObject obj in objectsToEnableForOwned)
            obj.SetActive(true);
    }

}
