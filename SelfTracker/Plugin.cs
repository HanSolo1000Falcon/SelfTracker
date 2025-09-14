using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using Application = UnityEngine.Device.Application;

namespace SelfTracker;

[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }
    
    public bool IsInRoom;
    public string CurrentRoomName;
    
    public ConfigEntry<string> PlayerName;
    private ConfigEntry<string> webhookUrl;

    private string messageId;
    
    private HttpClient client = new();

    private void Awake()
    {
        Instance = this;
        
        ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "SelfTracker.cfg"), true);
        webhookUrl = configFile.Bind("General", "Webhook URL", "https://discord.com/api/webhooks/1411056966654099456/yc4jXw5s9wJGb8AERE5GpS6t4uxUqnXKxULzEznG637HJUd8y4OevO8O-kFASOpGh4oN", "Webhook URL to send the data to");
        PlayerName = configFile.Bind("General", "Player Name", "dsaklfhdsafjkdsafgdsafghdsafhdlskjfhsda", "Your player name, so you can identify who started the game");
        
        Application.quitting += OnQuit;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        
        Harmony harmony = new(Constants.PluginGuid);
        harmony.PatchAll();
    }
    
    private void OnGameInitialized()
    {
        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{PlayerName.Value} has started Gorilla Tag"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";

        SendWebhook(json);
        
        NetworkSystem.Instance.OnJoinedRoomEvent += (Action)OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action)OnLeftRoom;
        
        NetworkSystem.Instance.OnPlayerJoined += (Action<NetPlayer>)OnPlayerJoined;
        NetworkSystem.Instance.OnPlayerLeft += (Action<NetPlayer>)OnPlayerLeft;
    }

    private void OnPlayerJoined(NetPlayer fuckingWeirdo)
    {
        if (fuckingWeirdo.IsLocal)
            return;
        
        string map = PhotonNetworkController.Instance.currentJoinTrigger == null
            ? "forest"
            : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
        map = map.ToUpper();
        
        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{PlayerName.Value} has joined code `{CurrentRoomName}`"",
            ""description"": ""In game name: `{PhotonNetwork.LocalPlayer.NickName}`\nPlayers in room: `{NetworkSystem.Instance.RoomPlayerCount}`\nMap: `{map}`\nPublic room: `{PhotonNetwork.CurrentRoom.IsVisible}`\nIs Modded: `{NetworkSystem.Instance.GameModeString.Contains("MODDED")}`\nGamemode: `{GetGamemodeKey(NetworkSystem.Instance.GameModeString)}`\nQueue: `{GetQueueKey(NetworkSystem.Instance.GameModeString)}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        SendWebhook(json, true);
    }
    
    private void OnPlayerLeft(NetPlayer fuckingWeirdo)
    {
        if (fuckingWeirdo.IsLocal)
            return;
        
        string map = PhotonNetworkController.Instance.currentJoinTrigger == null
            ? "forest"
            : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
        map = map.ToUpper();
        
        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{PlayerName.Value} has joined code `{CurrentRoomName}`"",
            ""description"": ""In game name: `{PhotonNetwork.LocalPlayer.NickName}`\nPlayers in room: `{NetworkSystem.Instance.RoomPlayerCount}`\nMap: `{map}`\nPublic room: `{PhotonNetwork.CurrentRoom.IsVisible}`\nIs Modded: `{NetworkSystem.Instance.GameModeString.Contains("MODDED")}`\nGamemode: `{GetGamemodeKey(NetworkSystem.Instance.GameModeString)}`\nQueue: `{GetQueueKey(NetworkSystem.Instance.GameModeString)}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        SendWebhook(json, true);
    }

    private void OnJoinedRoom()
    {
        IsInRoom = true;
        
        CurrentRoomName = NetworkSystem.Instance.RoomName;
        string map = PhotonNetworkController.Instance.currentJoinTrigger == null
            ? "forest"
            : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
        map = map.ToUpper();

        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{PlayerName.Value} has joined code `{CurrentRoomName}`"",
            ""description"": ""In game name: `{PhotonNetwork.LocalPlayer.NickName}`\nPlayers in room: `{NetworkSystem.Instance.RoomPlayerCount}`\nMap: `{map}`\nPublic room: `{PhotonNetwork.CurrentRoom.IsVisible}`\nIs Modded: `{NetworkSystem.Instance.GameModeString.Contains("MODDED")}`\nGamemode: `{GetGamemodeKey(NetworkSystem.Instance.GameModeString)}`\nQueue: `{GetQueueKey(NetworkSystem.Instance.GameModeString)}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        SendWebhook(json);
    }

    private void OnQuit()
    {
        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{PlayerName.Value} has quit Gorilla Tag"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        SendWebhook(json);
    }

    private void OnLeftRoom()
    {
        if (!IsInRoom)
            return;
        
        IsInRoom = false;
        
        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{PlayerName.Value} has left the code `{CurrentRoomName}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        SendWebhook(json);
    }
    
    public string GetGamemodeKey(string gamemodeString)
    {
        gamemodeString = gamemodeString.ToUpper();
        if (gamemodeString.Contains("CASUAL")) return "CASUAL";
        if (gamemodeString.Contains("INFECTION")) return "INFECTION";
        if (gamemodeString.Contains("HUNT")) return "HUNT";
        if (gamemodeString.Contains("Freeze")) return "FREEZE";
        if (gamemodeString.Contains("PAINTBRAWL")) return "PAINTBRAWL";
        if (gamemodeString.Contains("AMBUSH")) return "AMBUSH";
        if (gamemodeString.Contains("GHOST")) return "GHOST";
        if (gamemodeString.Contains("GUARDIAN")) return "GUARDIAN";
        return gamemodeString;
    }

    public string GetQueueKey(string gamemodeString)
    {
        gamemodeString = gamemodeString.ToUpper();
        if (gamemodeString.Contains("DEFAULT")) return "DEFAULT";
        if (gamemodeString.Contains("MINIGAMES")) return "MINI GAMES";
        if (gamemodeString.Contains("COMPETITIVE")) return "COMPETITIVE";
        return gamemodeString;
    }

    public void SendWebhook(string json, bool isEditing = false)
    {
        StartCoroutine(SendWebhookAsync(json, isEditing));

        /*try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            client.PostAsync(webhookUrl.Value, content).GetAwaiter().GetResult();
        } catch { }*/
    }

    private IEnumerator SendWebhookAsync(string json, bool isEditing)
    {
        UnityWebRequest request;

        if (!isEditing)
        {
            request = new UnityWebRequest(webhookUrl.Value + "?wait=true", "POST");
        }
        else
        {
            string editUrl = $"{webhookUrl.Value}/messages/{messageId}";
            request = new UnityWebRequest(editUrl, "PATCH");
        }

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Webhook {(isEditing ? "edit" : "send")} failed: {request.error}");
        }
        else
        {
            Debug.Log($"Webhook {(isEditing ? "edited" : "sent")} successfully.");

            if (!isEditing)
            {
                var responseText = request.downloadHandler.text;
                int idIndex = responseText.IndexOf("\"id\":\"", StringComparison.Ordinal);
                if (idIndex >= 0)
                {
                    int start = idIndex + 6;
                    int end = responseText.IndexOf('"', start);
                    messageId = responseText.Substring(start, end - start);
                    Debug.Log($"Captured messageId: {messageId}");
                }
                else
                {
                    Debug.LogError("Failed to parse messageId from webhook response: " + responseText);
                }
            }

        }
    }
}