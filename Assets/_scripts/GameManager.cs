using UnityEngine;
using System.Collections;

public class GameManager : Photon.MonoBehaviour {

    public GameObject mountainPrefab;

    void Start()
    {
        if (NetworkHandler.Instance.Online && PhotonNetwork.isMasterClient)
            PhotonNetwork.Instantiate("mountain", Vector3.zero, mountainPrefab.transform.rotation, 0, null);
        else if (NetworkHandler.Instance.Offline)
            Instantiate(Resources.Load("mountain"), Vector3.zero, mountainPrefab.transform.rotation);
    }
}
