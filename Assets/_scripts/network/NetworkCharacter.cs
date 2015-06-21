using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

    public Character character;
    public GameObject characterModel;
    public GameObject[] objectsToEnableForOwned;

    public Vector3 correctCharacterPosition;
    public Quaternion correctCharacterRotation;

    public string face;
    public Vector2 tileCoords = Vector2.zero;

    void Start()
    {
        if (NetworkHandler.Instance.Online)
            NetworkHandler.Instance.playerRootsByID[photonView.ownerId - 1] = gameObject;
    }

    void Update()
    {
        if (!photonView.isMine)
        {
            characterModel.transform.position = Vector3.Lerp(characterModel.transform.position, this.correctCharacterPosition, Time.deltaTime * 5);
            characterModel.transform.rotation = Quaternion.Lerp(characterModel.transform.rotation, this.correctCharacterRotation, Time.deltaTime * 5);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(characterModel.transform.position);
            stream.SendNext(characterModel.transform.rotation);

        }
        else
        {
            // Network player, receive data
            this.correctCharacterPosition = (Vector3)stream.ReceiveNext();
            this.correctCharacterRotation = (Quaternion)stream.ReceiveNext();

        }
    } 

    public void OwnThisCharacter()
    {
        foreach (GameObject obj in objectsToEnableForOwned)
            obj.SetActive(true);

        characterModel.GetComponent<Renderer>().enabled = true;

        if (character is Player)
        {
            Player player = character as Player;
            player.audioListener.gameObject.SetActive(true);
        }

        // keeping list of owned characters for quick access
        NetworkHandler.Instance.ownedCharacters.Add(character);
    }

    [RPC]
    public void ChangedTile(int[] detailArray)
    {
        // [0] : photon player ID
        // [1] : face index
        // [2] : tile x
        // [3] : tile y

        NetworkCharacter nc = NetworkHandler.Instance.playerRootsByID[detailArray[0] - 1].GetComponent<NetworkCharacter>();
        nc.face = Mountain.Instance.GetFaceNameByIndex(detailArray[1]);
        nc.tileCoords.x = detailArray[2];
        nc.tileCoords.y = detailArray[3];

        // we'll only run this loop on the player
        bool visible = false;

        // can I see this character after this tile change? must check with ALL my characters
        foreach (Character c in NetworkHandler.Instance.ownedCharacters)
        {
            if (c.currentFace == nc.face && c.closestTile.x == nc.tileCoords.x && c.closestTile.y == nc.tileCoords.y)
            {
                // yes I can
                visible = true;
            }
        }

        nc.characterModel.GetComponent<Renderer>().enabled = visible;
    }
}
