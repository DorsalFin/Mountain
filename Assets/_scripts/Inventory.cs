using UnityEngine;
using System.Collections;

public class Inventory : MonoBehaviour {

    public int currentCash;

    void Start()
    {
        currentCash = LevelParameters.Instance.startingCash;
    }

}
