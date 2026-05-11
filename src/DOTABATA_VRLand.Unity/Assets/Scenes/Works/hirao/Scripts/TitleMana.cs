using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMana : MonoBehaviour
{
    public Text textLogs;
    public GameObject SyncPlayerPrefab;
    public GameObject player;
    private Guid myConnectionId;
    static public bool isJoin = false;
    public Dictionary<Guid, GameObject> playerList = new Dictionary<Guid, GameObject>();

    /// <summary>
    /// TextにLogを表示
    /// </summary>
    public void TextLogs(string text)
    {
        textLogs.text = $"{text}\n{textLogs.text}";
        Debug.Log(text);
    }

    /// <summary>
    /// ConnectionIdの取得
    /// </summary>
    public Guid GetConnectionId() => myConnectionId;

    private void Awake()
    {
        RoomModel.I.OnJoinedUser += OnJoinedUser;
        RoomModel.I.OnLeavedUser += OnLeavedUser;
        RoomModel.I.OnUpdatedUserTransfrom += OnSyncPlayer;
    }

    private void OnDisable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnJoinedUser -= OnJoinedUser;
            RoomModel.I.OnLeavedUser -= OnLeavedUser;
        }
    }

    private void OnDestroy()
    {
        isJoin = false;
        OnDisable();
    }

    private async void Start()
    {
        await UserModel.I.CreateUserModel();
        await RoomModel.I.ConnectAsync();

        myConnectionId = RoomModel.I.ConnectionId;

        SyncPlayer syncPlayer = player.GetComponent<SyncPlayer>();
        syncPlayer.isLocalPlayer = true;  // これだけでOK
    }

    /// <summary>
    /// ルーム参加ボタン
    /// </summary>
    public async void JoinRoom()
    {
        await RoomModel.I.JoinRoomAsync();
        isJoin = true;

        string text = "";
        foreach (var user in await UserModel.I.GetAllUsersAsync())
        {
            text += $"Id：{user.Id}, Name：{user.Name}\n";
        }
        TextLogs(text);
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの入室通知
    /// </summary>
    private void OnJoinedUser(JoinedUser user)
    {
        TextLogs($"{user.Name}が入室");
        if(user.ConnectionId != myConnectionId)
        {
            GameObject player = Instantiate(SyncPlayerPrefab);
            playerList.Add(user.ConnectionId, player);

            SyncPlayer syncPlayer = player.GetComponent<SyncPlayer>();
            PlayerData data = new PlayerData() { playerObj = player };
            InRoomPlayerData.I.AddPlayer(user.ConnectionId, data);
        }
    }

    /// <summary>
    /// 自身以外の同期
    /// </summary>
    private void OnSyncPlayer(Guid connectionId, PlayerTransformDTO data)
    {
        if (!playerList.ContainsKey(connectionId)) return;

        SyncPlayer player = playerList[connectionId].GetComponent<SyncPlayer>();
        player.ApplyTransform(data);
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの退室通知
    /// </summary>
    private void OnLeavedUser(Guid connectionId, int joinOrder)
    {
        TextLogs($"ConnectionId：{connectionId} が退室");
        InRoomPlayerData.I.RemovePlayer(connectionId);
    }
}
