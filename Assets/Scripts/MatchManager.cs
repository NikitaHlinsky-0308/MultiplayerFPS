using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using Random = UnityEngine.Random;


public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }
    
    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStats,
        NextMatch
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int _index;

    public bool perpetual;

    private List<LeaderboardPlayer> _lboardPlayers = new List<LeaderboardPlayer>();

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWing = 20;
    public Transform mapCamPoint;
    public GameState state = GameState.Waiting;
    public float waitingAfterEnding = 5f;
    
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);

            state = GameState.Playing;
        }
    }

    private void Update()
    {
        
        
        if (Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
        {
            if(UIManager.instance.leaderboardGO.activeInHierarchy)
            {
                UIManager.instance.leaderboardGO.SetActive(false);
            } else
            {
                ShowLeaderboardPlayers();
            }
            
        }
        
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            //Debug.Log("Recieved events: " + theEvent);

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStats:
                    UpdateStatsReceive(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;
                
            }
        }
    }

    public override void OnEnable()
    {
        
        PhotonNetwork.AddCallbackTarget(this);
    }
    
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent
            (
                (byte)EventCodes.NewPlayer,
                package,
                new RaiseEventOptions{ Receivers = ReceiverGroup.MasterClient},
                new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] dataReceive)
    {
        PlayerInfo player = new PlayerInfo(
            (string)dataReceive[0],
            (int)dataReceive[1],
            (int)dataReceive[2],
            (int)dataReceive[3]
        );
        
        allPlayers.Add(player);
        
        ListPlayersSend(); 
    }

    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count + 1];

        package[0] = state;

    for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece;
        }
        
        PhotonNetwork.RaiseEvent
        (
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions{ Receivers = ReceiverGroup.All},
            new SendOptions { Reliability = true }
        );
    }

    public void ListPlayersReceive(object[] dataReceive)
    {
        allPlayers.Clear();

        state = (GameState)dataReceive[0];

        for (int i = 1; i < dataReceive.Length; i++)
        {
            object[] piece = (object[])dataReceive[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
            );
            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                _index = i - 1;
            }
        }
        
        StateCheck();
    }
    
    public void UpdateStatsSend(int actocSending, int startToUpd, int amountToChange)
    {
        object[] package = new object[]
        {
            actocSending,
            startToUpd,
            amountToChange
        };
        
        PhotonNetwork.RaiseEvent
        (
            (byte)EventCodes.UpdateStats,
            package,
            new RaiseEventOptions{ Receivers = ReceiverGroup.All},
            new SendOptions { Reliability = true }
        );
    }

    public void UpdateStatsReceive(object[] dataReceive)
    {
        int actor = (int)dataReceive[0];
        int stats = (int)dataReceive[1];
        int amount = (int)dataReceive[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (stats)
                {
                    case 0: // kills
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " :kills " + allPlayers[i].kills);
                        break;
                    case 1: // deaths
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " :deaths " + allPlayers[i].deaths);
                        break;
                }

                if (i == _index)
                {
                    UpdateStatsDisplay();
                }
                
                break;
            }
        }
        ScoreCheck();
    }

    public void UpdateStatsDisplay()
    {
        if (allPlayers.Count > _index)
        {
            UIManager.instance.killsCountText.text = "kills: " + allPlayers[_index].kills;
            UIManager.instance.DeathsCoutText.text = "deaths: " + allPlayers[_index].deaths;
        }
        else
        {
            UIManager.instance.killsCountText.text = "kills: 0";
            UIManager.instance.DeathsCoutText.text = "deaths: 0";
        }
        
        
    }

    private void ShowLeaderboardPlayers()
    {
        UIManager.instance.leaderboardGO.SetActive(true);

        foreach (LeaderboardPlayer lb in _lboardPlayers)
        {
            Destroy(lb.gameObject);
        }
        _lboardPlayers.Clear();
        
        UIManager.instance.LeaderboardPlayerDisplay.gameObject.SetActive(false);
        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach (PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(
                UIManager.instance.LeaderboardPlayerDisplay,
                UIManager.instance.LeaderboardPlayerDisplay.transform.parent
            ); 
            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);
            
            newPlayerDisplay.gameObject.SetActive(true);
            _lboardPlayers.Add(newPlayerDisplay);
        }
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selection = players[0];

            foreach (PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > highest)
                        {
                            selection = player;
                            highest = player.kills;
                        }
                }
                
                
            }
            
            sorted.Add(selection);
        }
        
        return sorted;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        
        SceneManager.LoadScene("MainMenu");
    }

    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in allPlayers)
        {
            if (player.kills >= killsToWing && killsToWing > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();
            }
        }
    }

    private void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        state = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        
        UIManager.instance.endGameScreen.SetActive(true);
        //Debug.Log("end game");
        ShowLeaderboardPlayers();
        //UIManager.instance.leaderboardGO.SetActive(true);
        

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;

        StartCoroutine(EndCo());
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitingAfterEnding);
        
        if (!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!Launcher.instance.changeMapsBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = Random.Range(0, Launcher.instance.allMaps.Length);
                    
                    if (Launcher.instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        PhotonNetwork.LoadLevel(Launcher.instance.allMaps[newLevel]);
                    }
                }
            }
        }
        
        
    }

    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent
        (
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions{ Receivers = ReceiverGroup.All},
            new SendOptions { Reliability = true }
        );
    }
    
    public void NextMatchReceive()
    {
        state = GameState.Playing;
        UIManager.instance.endGameScreen.SetActive(false);
        UIManager.instance.leaderboardGO.SetActive(false);

        foreach (PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }
        
        UpdateStatsDisplay();
        
        PlayerSpawner.instance.SpawnPlayer();
    }
}
[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int kills, deaths, actor;

    public PlayerInfo(string _name, int _actor, int _deaths, int _kills)
    {
        name = _name;
        kills = _kills;
        deaths = _deaths;
        actor = _actor;
    }
}
