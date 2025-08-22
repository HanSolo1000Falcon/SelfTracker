using System;
using System.Collections;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine.Networking;

namespace SelfTracker;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private ConfigEntry<string> webhookUrl;
    private ConfigEntry<string> playerName;
    
    private string currentRoomName;

    private void Awake()
    {
        ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "SelfTracker.cfg"), true);
        webhookUrl = configFile.Bind("General", "Webhook URL", "https://discord.com/api/webhooks/1408476445923086396/N_M5JrdQZ0jd1yT64qVSN7dCjXjqVAct4AvUIOjN-rSaE4Wj1U216avD0GyuildJTGOa", "Webhook URL to send the data to");
        playerName = configFile.Bind("General", "Player Name", "Clown Doesnt Check Config", "Your player name, so you can identify who started the game");
    }
    
    private void OnGameInitialized()
    {
        string json = $@"
{{
    ""content"": ""### Self Tracker"",
    ""embeds"": [
        {{
            ""title"": ""{playerName.Value} has started Gorilla Tag"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        StartCoroutine(SendToWebhook(json));

        NetworkSystem.Instance.OnJoinedRoomEvent += (Action)OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action)OnLeftRoom;
    }

    private void OnJoinedRoom()
    {
        currentRoomName = NetworkSystem.Instance.RoomName;
        string gamemode = PhotonNetworkController.Instance.currentJoinTrigger == null
            ? "forest"
            : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
        gamemode = gamemode.ToUpper();

        string json = $@"
{{
    ""content"": ""### Self Tracker"",
    ""embeds"": [
        {{
            ""title"": ""{playerName.Value} has joined code `{currentRoomName}`"",
            ""description"": ""In game name: `{PhotonNetwork.LocalPlayer.NickName}`\nPlayers in room: `{NetworkSystem.Instance.RoomPlayerCount}`\nGamemode: `{gamemode}`\nPublic room: `{PhotonNetwork.CurrentRoom.IsVisible}`\nIs Modded: `{NetworkSystem.Instance.GameModeString.Contains("MODDED")}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";

        StartCoroutine(SendToWebhook(json));
    }
    
    private void OnLeftRoom()
    {
        string json = $@"
{{
    ""content"": ""### Self Tracker"",
    ""embeds"": [
        {{
            ""title"": ""{playerName.Value} has left the code `{currentRoomName}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";

        StartCoroutine(SendToWebhook(json));
    }

    private IEnumerator SendToWebhook(string json)
    {
        UnityWebRequest request = new UnityWebRequest(webhookUrl.Value, "POST");
        byte[] bodyRaw = new UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
    }
    
    private void Start() => GorillaTagger.OnPlayerSpawned(OnGameInitialized);
}