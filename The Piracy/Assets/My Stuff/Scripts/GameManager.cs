using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton = null;

    void Awake(){
        if (Singleton != null){
            Debug.LogError("poos");
        }
        Singleton = this;
    }

    void Start(){
        
        var data = GameNetworkHelper.Singleton.clientLobby.Data;

        int seed = data["Seed"].Value == "0" ? new System.Random().Next() : int.Parse(data["Seed"].Value);
        int octaves = int.Parse(data["NoiseOctaves"].Value);
        float persistance = float.Parse(data["NoisePersistence"].Value);
        float scale = float.Parse(data["NoiseScale"].Value);
        float lacunarity = float.Parse(data["NoiseLacunarity"].Value);

        MapGenerater.Singleton.SetMapInformation(seed, octaves, persistance, scale, lacunarity);
    }
}
