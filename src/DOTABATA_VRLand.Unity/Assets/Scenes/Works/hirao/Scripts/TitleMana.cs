using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;
using UnityEngine.UI;

public class TitleMana : MonoBehaviour
{
    public InputField nameText;
    public InputField passText;
    public int gameModeId = 1;

    public RoomConfig SetNames()
    {
        RoomConfig roomConfig = new RoomConfig()
        {
            Name = nameText.text,
            Password = passText.text,
            GameModeId = gameModeId,
        };
        return roomConfig;
    }

    /// <summary>
    /// Gameシーンに移動ボタン
    /// </summary>
    public async void JointoNextScene(string name)
    {
        await NetworkManager.I.JointoNextScene(name, SetNames());
    }
}
