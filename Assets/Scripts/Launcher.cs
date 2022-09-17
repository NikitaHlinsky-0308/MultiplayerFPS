using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using Random = UnityEngine.Random;


public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;
    private void Awake()
        {
            instance = this;
        }
    
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    public GameObject menuButtons;
    public GameObject createRoomPanel;
    public TMP_InputField roomNameInput;
    public GameObject roomPanel;
    public TMP_Text RoomNameText, playerNameText;
    private List<TMP_Text> _allPlayersText = new List<TMP_Text>();
    public GameObject errorPanel;
    public TMP_Text erroeText;
    public GameObject roomBrowserPanel;
    public RoomButton roomButton;
    public GameObject createNicknamePanel;
    public TMP_InputField NicknameInputField;
    public static bool nicknameHasSet;

    public string levelToPlay;
    public GameObject startBtn;

    public GameObject roomTestBtn;
    
    private List<RoomButton> _allRoomButtons = new List<RoomButton>();

    public string[] allMaps;
    public bool changeMapsBetweenRounds = true;
    

    void Start()
    {
        CloseMenus();
        
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to network ...";

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        
        
#if UNITY_EDITOR
        roomTestBtn.SetActive(true);
#endif

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingText.text = "Connecting to lobby ...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        
        PhotonNetwork.NickName = Random.Range(1, 1000).ToString();

        if (!nicknameHasSet)
        {
            CloseMenus();
            createNicknamePanel.SetActive(true);

            if (PlayerPrefs.HasKey("PlayerName"))
            {
                NicknameInputField.text = PlayerPrefs.GetString("PlayerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        roomBrowserPanel.SetActive(false);
        createNicknamePanel.SetActive(false);
    }

    public void OpenRoomCreateScreen()
    {
        CloseMenus();
        createRoomPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            
            PhotonNetwork.CreateRoom(roomNameInput.text, options);
            
            CloseMenus();
            loadingText.text = "Creationg room ...";
            loadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus(); 
        roomPanel.SetActive(true);

        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        
        ListAllPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startBtn.SetActive(true);
        }
        else
        {
            startBtn.SetActive(false);
        }
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in _allPlayersText )
        {
            Destroy(player.gameObject);
        }
        _allPlayersText.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerText = Instantiate(playerNameText, playerNameText.transform.parent);
            newPlayerText.text = players[i].NickName;
            newPlayerText.gameObject.SetActive(true);
            
            _allPlayersText.Add(newPlayerText);
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerText = Instantiate(playerNameText, playerNameText.transform.parent);
        newPlayerText.text = newPlayer.NickName;
        newPlayerText.gameObject.SetActive(true);
            
        _allPlayersText.Add(newPlayerText);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        erroeText.text = "Failed room creating" + message;
        CloseMenus();
        
        errorPanel.SetActive(true);
    }

    public void CloseErrorPanel()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        erroeText.text = String.Empty;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Roon";
        loadingScreen.Serialize(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserPanel.SetActive(true);
    }
    
    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in _allRoomButtons )
        {
            Destroy(rb.gameObject);
        }
        _allRoomButtons.Clear();

        roomButton.gameObject.SetActive(false);
        
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(roomButton, roomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                
                _allRoomButtons.Add(newButton);
            }
        }
    }

    

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        
        CloseMenus();
        loadingText.text = "Joining room";
        loadingScreen.SetActive(true); 
    }

    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(NicknameInputField.text))
        {
            PhotonNetwork.NickName = NicknameInputField.text;
            PlayerPrefs.SetString("PlayerName", NicknameInputField.text);
            
            CloseMenus();
            menuButtons.SetActive(true);

            nicknameHasSet = true;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startBtn.SetActive(true);
        }
        else
        {
            startBtn.SetActive(false);
        }
    }

    public void StartGame()
    {
        //PhotonNetwork.LoadLevel(levelToPlay);
        
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void QuickStart()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        
        PhotonNetwork.CreateRoom("Test");
        CloseMenus();
        loadingText.text = "Creating room";
        loadingScreen.SetActive(true);
    }
}
