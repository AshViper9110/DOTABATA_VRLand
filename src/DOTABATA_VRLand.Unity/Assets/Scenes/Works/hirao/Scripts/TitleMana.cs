using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;
using Steamworks;

public class TitleMana : MonoBehaviour
{
    private string playerName;
    public int gameModeId = 1;

    private void Start()
    {
        playerName = SteamFriends.GetPersonaName();
    }
    public RoomConfig SetNames()
    {
        RoomConfig roomConfig = new RoomConfig()
        {
            Name = playerName + "Name",
            Password = "0000",
            GameModeId = gameModeId,
        };
        return roomConfig;
    }

    /// <summary>
    /// Gameシーンに移動ボタン
    /// </summary>
    public async void JointoNextScene(string name)
    {
        await NetworkManager.I.JointoNextScene(name, playerName, SetNames());
    }
}
