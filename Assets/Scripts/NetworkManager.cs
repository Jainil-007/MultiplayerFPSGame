using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("Connection Status")]
    public Text connectionStatusText;


    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject Login_UI_Panel;


    [Header("Game Options UI Panel")]
    public GameObject GameOptions_UI_Panel;

    [Header("Create Room UI Panel")]
    public GameObject CreateRoom_UI_Panel;
    public InputField roomNameInputField;
   

    public InputField maxPlayerInputField;


    [Header("Inside Room UI Panel")]
    public GameObject InsideRoom_UI_Panel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListContent;
    public GameObject startGameButton;


    [Header("Room List UI Panel")]
    public GameObject RoomList_UI_Panel;
    public GameObject roomListEntryPrefab;
    public GameObject roomListParentGameobject;


    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_Panel;


    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameobjects;
    private Dictionary<int, GameObject> playerListGameobjects;


    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        ActivatePanel(Login_UI_Panel.name);

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameobjects = new Dictionary<string, GameObject>();

        PhotonNetwork.AutomaticallySyncScene = true;

    }

    // Update is called once per frame
    private void Update()
    {
        connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }

    #endregion


    #region UI Callbacks
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Playername is invalid!");
        }
    }


    public void OnRoomCreateButtonClicked()
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000,10000);

        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(maxPlayerInputField.text);

        PhotonNetwork.CreateRoom(roomName,roomOptions);
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptions_UI_Panel.name);
    }


    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(RoomList_UI_Panel.name);
    }


    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(GameOptions_UI_Panel.name);

    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(JoinRandomRoom_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();

    }


    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");

        }
    }



    #endregion



    #region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");


    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
        ActivatePanel(GameOptions_UI_Panel.name);

    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name+ " is created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+ " joined to "+ PhotonNetwork.CurrentRoom.Name );
        ActivatePanel(InsideRoom_UI_Panel.name);

     
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }


        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Players/Max.players: " +
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;


        if (playerListGameobjects==null)
        {
            playerListGameobjects = new Dictionary<int, GameObject>();

        }



        //Instantiating player list gameobjects
        foreach (Player player in PhotonNetwork.PlayerList)
        {

            GameObject playerListGameobject = Instantiate(playerListPrefab);
            playerListGameobject.transform.SetParent(playerListContent.transform);
            playerListGameobject.transform.localScale = Vector3.one;

            playerListGameobject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
            if (player.ActorNumber==PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(true);

            }
            else
            {
                playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(false);

            }

            playerListGameobjects.Add(player.ActorNumber, playerListGameobject);
        }
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //update room info text
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                           "Players/Max.players: " +
                           PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                           PhotonNetwork.CurrentRoom.MaxPlayers;



        GameObject playerListGameobject = Instantiate(playerListPrefab);
        playerListGameobject.transform.SetParent(playerListContent.transform);
        playerListGameobject.transform.localScale = Vector3.one;

        playerListGameobject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(true);

        }
        else
        {
            playerListGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(false);

        }

        playerListGameobjects.Add(newPlayer.ActorNumber, playerListGameobject);
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        //update room info text
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                           "Players/Max.players: " +
                           PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                           PhotonNetwork.CurrentRoom.MaxPlayers;



        Destroy(playerListGameobjects[otherPlayer.ActorNumber].gameObject);
        playerListGameobjects.Remove(otherPlayer.ActorNumber);


        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }


    }
    public override void OnLeftRoom()
    {

        ActivatePanel(GameOptions_UI_Panel.name);

        foreach (GameObject playerListGameobject in playerListGameobjects.Values)
        {
            Destroy(playerListGameobject);
        }

        playerListGameobjects.Clear();
        playerListGameobjects = null;




    }



    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible|| room.RemovedFromList )
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                //update cachedRoom list
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                //add the new room to the cached room list
                else
                {
                    cachedRoomList.Add(room.Name, room);

                }

            }
        }

        foreach (RoomInfo room in cachedRoomList.Values)
        {

            GameObject roomListEntryGameobject = Instantiate(roomListEntryPrefab);
            roomListEntryGameobject.transform.SetParent(roomListParentGameobject.transform);
            roomListEntryGameobject.transform.localScale = Vector3.one;


            roomListEntryGameobject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameobject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount+ " / "+ room.MaxPlayers;
            roomListEntryGameobject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(()=>OnJoinRoomButtonClicked(room.Name));

            roomListGameobjects.Add(room.Name,roomListEntryGameobject);



        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);


        string roomName = "Room " + Random.Range(1000,10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName,roomOptions);
    }





    #endregion

    #region Private Methods
    void OnJoinRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(_roomName);
    }

    void ClearRoomListView()
    {
        foreach (var roomListGameobject in roomListGameobjects.Values)
        {
            Destroy(roomListGameobject);
        }

        roomListGameobjects.Clear();
    }



    #endregion

    #region Public Methods
    public void ActivatePanel(string panelToBeActivated)
    {
        Login_UI_Panel.SetActive(panelToBeActivated.Equals(Login_UI_Panel.name));
        GameOptions_UI_Panel.SetActive(panelToBeActivated.Equals(GameOptions_UI_Panel.name));
        CreateRoom_UI_Panel.SetActive(panelToBeActivated.Equals(CreateRoom_UI_Panel.name));
        InsideRoom_UI_Panel.SetActive(panelToBeActivated.Equals(InsideRoom_UI_Panel.name));
        RoomList_UI_Panel.SetActive(panelToBeActivated.Equals(RoomList_UI_Panel.name));
        JoinRandomRoom_UI_Panel.SetActive(panelToBeActivated.Equals(JoinRandomRoom_UI_Panel.name));


    }


    #endregion


}
