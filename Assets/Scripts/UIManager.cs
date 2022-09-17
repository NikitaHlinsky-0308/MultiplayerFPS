using System;
using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static UIManager instance;
    public TMP_Text overheatedMessage;
    //[FormerlySerializedAs("OrevheatedSlider")] public Slider orevheatedSlider;
    public Slider orevheatedSlider;

    public GameObject deathPanel;
    public TMP_Text deathText;

    public Slider currentHealthSlider;

    public TMP_Text killsCountText, DeathsCoutText;

    public GameObject leaderboardGO;
    public LeaderboardPlayer LeaderboardPlayerDisplay;

    public GameObject endGameScreen;

    public GameObject pauseMenu;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }

        if (pauseMenu.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void PauseUnpause()
    {
        if (!pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(false);
        }
    }

    public void MainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
