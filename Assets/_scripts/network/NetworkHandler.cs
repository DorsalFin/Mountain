using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class NetworkHandler : Photon.MonoBehaviour {

    public string gameVersion = "v0.0.1";

    public static NetworkHandler Instance;
    void Awake()
    {
        Instance = this;
    }

    public Text connectionDetailsText;
    public Button[] buttonsToEnableOnConnect;
    public GameObject gameButtonPrefab;
    public GameObject networkCanvasRoot;
    public GameObject[] uiStageRoots;
    public Transform roomListTransform;
    public InputField gameNameInput;

    public Text playersInRoomText;

    public GameObject playerPrefab;
    public GameObject managerPrefab;

    public Text titleText;
    public Text debugText;
	
	void Start () 
    {
        PhotonNetwork.ConnectUsingSettings(gameVersion);
	}

    void Update()
    {
        if (networkCanvasRoot != null)
        {
            if (uiStageRoots[0].activeInHierarchy)
                connectionDetailsText.text = PhotonNetwork.connectionStateDetailed.ToString();

            if (uiStageRoots[1].activeInHierarchy)
                playersInRoomText.text = PhotonNetwork.room.playerCount + " / " + PhotonNetwork.room.maxPlayers + "  players connected";

            if (Input.GetKeyDown(KeyCode.M))
            {
                foreach (Tile tile in Mountain.Instance.faces["north"])
                {
                    debugText.text += " tile(" + tile.x + "," + tile.y + ") - ";

                    foreach (int path in tile.openPaths)
                        debugText.text += path + ", ";

                    debugText.text += " // ";
                }
            }
        }
    }
	
    void OnJoinedLobby()
    {
        foreach (Button button in buttonsToEnableOnConnect)
            button.interactable = true;

        InvokeRepeating("RefreshGameList", 0, 5);
    }

    void RefreshGameList()
    {
        // clear games first
        foreach (Transform gameTrans in roomListTransform)
            Destroy(gameTrans.gameObject);

        // get the current list from the photon server
        RoomInfo[] games = PhotonNetwork.GetRoomList();
        Array.Sort(games);

        // and refresh the object in the games list
        foreach (RoomInfo game in games)
        {
            GameObject gameButton = (GameObject)Instantiate(gameButtonPrefab, Vector3.zero, Quaternion.identity);
            gameButton.name = game.name;
            gameButton.transform.parent = roomListTransform;
            gameButton.GetComponentInChildren<Text>().text = game.name + " " + game.playerCount + "/" + game.maxPlayers;
            //gameButton.GetComponent<Button>().onClick += JoinGame;
            Button button = gameButton.GetComponent<Button>();
            button.onClick.AddListener(() => JoinGame(button.name));
        }
    }

    public void CreateGame()
    {
        CancelInvoke("RefreshGameList");

        string gameName = gameNameInput.text == "" ? "game" : gameNameInput.text;
        RoomOptions roomOptions = new RoomOptions() { maxPlayers = 4 };
        PhotonNetwork.CreateRoom(gameName, roomOptions, null);
    }

    public void JoinGame(String gameName)
    {
        CancelInvoke("RefreshGameList");

        PhotonNetwork.JoinRoom(gameName);
    }

    void OnJoinedRoom()
    {
        SetUIStage(1);
    }

    public void StartGame()
    {
        // todo restrict so only host can start game?

        // return if we've already locked the room (which means it's starting)
        PhotonNetwork.room.open = false;

        photonView.RPC("BeginSetup", PhotonTargets.All, null);
    }

    [RPC]
    public void BeginSetup()
    {
        SetUIStage(2);

        if (PhotonNetwork.isMasterClient)
            PhotonNetwork.InstantiateSceneObject("game manager", Vector3.zero, Quaternion.identity, 0, null);
    }

    void SetUIStage(int stage)
    {
        for (int i = 0; i < uiStageRoots.Length; i++)
            uiStageRoots[i].SetActive(i == stage);
    }

    public void BeginFade()
    {
        photonView.RPC("FadeTitleAndBegin", PhotonTargets.All);
    }

    [RPC]
    public void FadeTitleAndBegin()
    {
        StartCoroutine("FadeTitleCoroutine");
    }

    IEnumerator FadeTitleCoroutine()
    {
        yield return new WaitForSeconds(2.50f);
        titleText.CrossFadeAlpha(0, 2, false);
        yield return new WaitForSeconds(2.0f);
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        string startingFace = Mountain.Instance.faceTransforms[PhotonNetwork.player.ID - 1].name;

        GameObject playerObj = PhotonNetwork.Instantiate("player root", playerPrefab.transform.position, Quaternion.identity, 0);
        NetworkPlayer networkPlayer = playerObj.GetComponent<NetworkPlayer>();
        networkPlayer.ActivateThisPlayer();
        networkPlayer.player.SetHomeFace(startingFace);

        Destroy(networkCanvasRoot);
    }
}
