using UnityEngine;
using System.Collections;

public class GameManager : Photon.MonoBehaviour {

    public GameObject mountainPrefab;

    void Start()
    {
        if (PhotonNetwork.isMasterClient)
            PhotonNetwork.Instantiate("mountain", Vector3.zero, mountainPrefab.transform.rotation, 0, null);
    }
}
