using DOTABATA_VRLand.Shared.Models.Entities;
using Steamworks;
using System.Collections;
using UnityEngine;
using Valve.VR;

public class TestManager : MonoBehaviour
{
    private string playerName;
    public int gameModeId = 1;
    public GameObject player;

    private void Start()
    {
        if (SteamManager.Initialized)
        {
            playerName = SteamFriends.GetPersonaName();
            Debug.Log(playerName);
        }
        else
        {
            Debug.LogError("Steam is not initialized.");
        }
    }
    public RoomConfig SetNames()
    {
        RoomConfig roomConfig = new RoomConfig()
        {
            Name = "Name",
            Password = "0000",
            GameModeId = gameModeId,
        };
        return roomConfig;
    }

    /// <summary>
    /// Gameシーンに移動ボタン
    /// </summary>
    /// 
    public void JoinLobby()
    {
        StartCoroutine(MoveWithFade());
    }

    private IEnumerator MoveWithFade()
    {
        // 白フェードアウト
        SteamVR_Fade.Start(Color.white, 0.5f);

        // フェード完了待ち
        yield return new WaitForSeconds(0.5f);

        // 移動
        player.transform.position = Vector3.zero;

        // 1フレーム待つと安定しやすい
        yield return null;

        // フェードイン
        SteamVR_Fade.Start(Color.clear, 0.5f);
    }
    public async void JointoNextScene(string name)
    {
        await NetworkManager.I.JointoNextScene(name, playerName, SetNames());
    }
}
