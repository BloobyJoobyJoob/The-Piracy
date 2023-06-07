using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MenuNetworkHelper : MonoBehaviour
{
    public TextMeshProUGUI explaination;
    public TMP_InputField LobbyCode;
    public Slider MaxPlayers;
    public TMP_InputField lobbyName;
    public Button bigBackButton;
    public Button tryAgainButton;
    public Toggle PrivateLobby;
    public bool useRandomID = true;
    public TMP_InputField noiseLacunarity;
    public TMP_InputField noisePersistence;
    public TMP_InputField noiseScale;
    public TMP_InputField noiseOctaves;
    public TMP_InputField worldSize;
    public TMP_InputField seed;
    UnityTransport transport;
    Lobby lobby;
    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public async void OnPlayBtn(GameObject enableAfterCompletion)
    {
        explaination.text = "Authenticating";
        InitializationOptions initializationOptions = new();

        if (useRandomID)
        {
            initializationOptions.SetProfile(UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString());
        }
        else
        {
            initializationOptions.SetProfile("Primary");
        }

        try
        {
            await UnityServices.InitializeAsync(initializationOptions);
        }
        catch 
        {
            explaination.text = "Failed to Authenticate";
            tryAgainButton.gameObject.SetActive(true);
            throw;
        }

        explaination.text = "Signing In";

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch
        {
            explaination.text = "Failed to Sign In";
            tryAgainButton.gameObject.SetActive(true);
            throw;
        }

        explaination.text = "Signed In";
        enableAfterCompletion.SetActive(true);
    }

    public void ExitApp()
    {
        explaination.text = "quitting!";
        Debug.Log("Quittin!");
        Application.Quit();
    }

    public async void OnJoinByCodeButton()
    {
        explaination.text = "Joining Lobby";

        Lobby lobby;

        try
        {
            lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode.text);
        }
        catch (Exception e)
        {
            explaination.text = "Failed to join Lobby" + e.Message;
            bigBackButton.gameObject.SetActive(true);
            throw;
        }

        explaination.text = "Getting Relay Joincode";

        JoinAllocation a;

        try
        {
            a = await RelayService.Instance.JoinAllocationAsync(lobby.Data["JoinCode"].Value);
        }
        catch
        {
            explaination.text = "Failed getting Relay Joincode";
            bigBackButton.gameObject.SetActive(true);
            throw;
        }
        
        GameNetworkHelper.Singleton.SetClientLobby(lobby);
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        explaination.text = "Retrieved Relay Joincode";

        NetworkManager.Singleton.StartClient();
    }
    public async void OnQuickJoinButton()
    {
        explaination.text = "Finding Lobby";

        Lobby lobby;

        try
        {
            lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch
        {
            explaination.text = "Failed to find Lobby, consider making one!";
            bigBackButton.gameObject.SetActive(true);
            throw;
        }

        explaination.text = "Getting Relay Joincode";

        JoinAllocation a;

        try
        {
            a = await RelayService.Instance.JoinAllocationAsync(lobby.Data["JoinCode"].Value);
        }
        catch
        {
            explaination.text = "Failed getting Relay Joincode";
            bigBackButton.gameObject.SetActive(true);
            throw;
        }
        
        explaination.text = "Retrieved Relay Joincode";
        GameNetworkHelper.Singleton.SetClientLobby(lobby);

        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);


        NetworkManager.Singleton.StartClient();
    }

    public async void OnHostButton()
    {
        explaination.text = "Creating Relay Allocation";
        int maxPlayers = (int)MaxPlayers.value;

        Allocation a;

        try
        {
            a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        }
        catch 
        {
            explaination.text = "Failed to create relay allocation";
            bigBackButton.gameObject.SetActive(true);
            throw;
        }

        string joinCode;

        explaination.text = "Getting Relay Allocation Join Code";

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        }
        catch
        {
            explaination.text = "Failed to get Relay Allocation Join Code";
            bigBackButton.gameObject.SetActive(true);
            throw;
        }

        Dictionary<string, DataObject> lobbyOptionsData = new();

        lobbyOptionsData.Add("JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: joinCode));
        lobbyOptionsData.Add("Location", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: a.Region));

        lobbyOptionsData.Add("MaxPlayers", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: maxPlayers.ToString()));
        lobbyOptionsData.Add("WorldSize", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: worldSize.text));

        lobbyOptionsData.Add("Seed", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: seed.text == "" ? "0" : seed.text));
        lobbyOptionsData.Add("NoiseScale", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: noiseScale.text == "" ? "100" : noiseScale.text));
        lobbyOptionsData.Add("NoiseOctaves", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: noiseOctaves.text == "" ? "7" : noiseOctaves.text));
        lobbyOptionsData.Add("NoiseLacunarity", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: noiseLacunarity.text == "" ? "1.9" : noiseLacunarity.text));
        lobbyOptionsData.Add("NoisePersistence", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: noisePersistence.text == "" ? "0.5" : noisePersistence.text));

        CreateLobbyOptions lobbyOptions = new();

        lobbyOptions.IsPrivate = PrivateLobby.isOn;
        lobbyOptions.Data = lobbyOptionsData;

        explaination.text = "Creating Lobby";
        try
        {
            lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName.text, maxPlayers, lobbyOptions);
        }
        catch 
        {
            explaination.text = "Failed to create lobby";
            bigBackButton.gameObject.SetActive(true);
            throw;
        }

        explaination.text = "Sucessfully created lobby: " + lobby.LobbyCode;
        Debug.Log("Lobby Code: "+ lobby.LobbyCode);

        GameNetworkHelper.Singleton.StartCoroutine(GameNetworkHelper.Singleton.LobbyHeartBeat(lobby.Id));
        GameNetworkHelper.Singleton.SetClientLobby(lobby);

        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        if (NetworkManager.Singleton.StartHost())
        {
            explaination.text = "Loading game...";
            SceneManager.LoadScene("Main");
        }
        else
        {
            explaination.text = "Failed to host server";
            bigBackButton.gameObject.SetActive(true);
        }
    }
}