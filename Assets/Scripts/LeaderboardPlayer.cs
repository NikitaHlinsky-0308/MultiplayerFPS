using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardPlayer : MonoBehaviour
{
    public TMP_Text playerNameText, killsCountBoard, deathsCountBoard;

    public void SetDetails(string username, int kills, int deaths)
    {
        playerNameText.text = username;
        killsCountBoard.text = kills.ToString();
        deathsCountBoard.text = deaths.ToString();
    }
}
