using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;
using Steamworks;
using Valve.VR;
using System.Collections;

public class TitleMana : MonoBehaviour
{
    private string playerName;
    private ulong steamId;
    public int gameModeId = 1;
    public GameObject player;

    private async void Start()
    {
        SteamVR_Fade.View(Color.clear, 0.5f);
        if (SteamManager.Initialized)
        {
            playerName = SteamFriends.GetPersonaName();
            steamId = SteamUser.GetSteamID().m_SteamID;//steamIdを取得
            Debug.Log($"name:{playerName} SteamID:{steamId}");
            await UserModel.I.CreateUserModel();
            bool result = await UserModel.I.RegistUserAsync(
            playerName,
            steamId
            );
            Debug.Log($"result:{result}");

        }
        else
        {
            Debug.LogError("Steam is not initialized.");
        }

        InRoomPlayerData.I.SetMySelf(new PlayerData() { playerObj = player });
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
        SteamVR_Fade.View(Color.white, 0.5f);

        // フェード完了待ち
        yield return new WaitForSeconds(0.5f);

        // 移動
        player.transform.position = Vector3.zero;

        // 1フレーム待つと安定しやすい
        yield return null;

        // フェードイン
        SteamVR_Fade.View(Color.clear, 0.5f);
    }
}
