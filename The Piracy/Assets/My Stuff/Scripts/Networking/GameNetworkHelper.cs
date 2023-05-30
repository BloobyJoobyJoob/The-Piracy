using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameNetworkHelper : MonoBehaviour
{
    public static GameNetworkHelper Singleton = null;
    public Lobby clientLobby {get; private set; }
    private void Awake()
    {
        if (Singleton != null)
        {
            Debug.LogError("sadas");
            return;
        }
        Singleton = this;
    }

    public void SetClientLobby(Lobby l){
        clientLobby = l;
    }

    public IEnumerator LobbyHeartBeat(string lobbyID)
    {
        while (true)
        {
            Debug.Log("Sending Lobby Heartbeat");
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return new WaitForSeconds(15);
        }
    }
}
