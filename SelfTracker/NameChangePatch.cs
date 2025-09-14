using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;

namespace SelfTracker;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SetNameTagText))]
public static class NameChangePatch
{
    private static void Postfix(VRRig __instance, string name)
    {
        if (!Plugin.Instance.IsInRoom || !__instance.isLocal)
            return;
        
        string map = PhotonNetworkController.Instance.currentJoinTrigger == null
            ? "forest"
            : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
        map = map.ToUpper();

        string json = $@"
{{
    ""embeds"": [
        {{
            ""title"": ""{Plugin.Instance.PlayerName.Value} has joined code `{Plugin.Instance.CurrentRoomName}`"",
            ""description"": ""In game name: `{name}`\nPlayers in room: `{NetworkSystem.Instance.RoomPlayerCount}`\nMap: `{map}`\nPublic room: `{PhotonNetwork.CurrentRoom.IsVisible}`\nIs Modded: `{NetworkSystem.Instance.GameModeString.Contains("MODDED")}`\nGamemode: `{Plugin.Instance.GetGamemodeKey(NetworkSystem.Instance.GameModeString)}`\nQueue: `{Plugin.Instance.GetQueueKey(NetworkSystem.Instance.GameModeString)}`"",
            ""color"": 7415295,
            ""footer"": {{ ""text"": ""Self Tracker by HanSolo1000Falcon"" }}
        }}
    ]
}}";
        
        Plugin.Instance.SendWebhook(json, true);
    }
}