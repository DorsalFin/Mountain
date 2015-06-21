using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class NetworkHandler : Photon.MonoBehaviour {

    public string gameVersion = "v0.0.1";
    bool _online;
    public bool Online { get { return _online; } }
    public bool Offline { get { return !_online; } }


    public static NetworkHandler Instance;
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// this holds all root player objects in a game indexed by photon playerID
    /// </summary>
    public GameObject[] playerRootsByID = new GameObject[4];
    public List<Character> ownedCharacters = new List<Character>();

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
        }
    }

    /// <summary>
    /// can skip multiplayer process and just start single player
    /// </summary>
    public void StartSinglePlayer()
    {
        CancelInvoke("RefreshGameList");
        _online = false;
        Instantiate(Resources.Load("game manager"), Vector3.zero, Quaternion.identity);
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

        _online = true;

        string gameName = gameNameInput.text == "" ? "game" : gameNameInput.text;
        RoomOptions roomOptions = new RoomOptions() { maxPlayers = 4 };
        PhotonNetwork.CreateRoom(gameName, roomOptions, null);
    }

    public void JoinGame(String gameName)
    {
        CancelInvoke("RefreshGameList");

        _online = true;

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
        string startingFace = _online ? Mountain.Instance.faceTransforms[PhotonNetwork.player.ID - 1].name : "north";

        GameObject playerObj = null;

        if (_online)
            playerObj = PhotonNetwork.Instantiate("player root", playerPrefab.transform.position, Quaternion.identity, 0);
        else
            playerObj = (GameObject)Instantiate(Resources.Load("player root"), Vector3.zero, Quaternion.identity);

        NetworkCharacter networkCharacter = playerObj.GetComponent<NetworkCharacter>();
        networkCharacter.OwnThisCharacter();
        networkCharacter.character.SetHomeFace(startingFace);

        Destroy(networkCanvasRoot);
    }
}
