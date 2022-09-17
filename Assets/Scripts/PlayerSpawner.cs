using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPref;
    private GameObject _player;
    public GameObject deathEffect;
    public float respawnTime = 3.0f;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
             SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        _player = PhotonNetwork.Instantiate(playerPref.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string damager)
    {
        
        UIManager.instance.deathText.text = "You were killed by " + damager;
        UIManager.instance.deathPanel.SetActive(true);
        
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (_player != null)
        {
            StartCoroutine(DieCo());
        }
    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name, _player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(_player);
        _player = null;
        
        UIManager.instance.deathPanel.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
        
        UIManager.instance.deathPanel.SetActive(false);

        if (MatchManager.instance.state == MatchManager.GameState.Playing && _player == null)
        {
            SpawnPlayer();
        }
    }
}
