using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMana : MonoBehaviour
{
    public GameObject SyncPlayerPrefab;
    public GameObject player;
    private Guid myConnectionId;
    static public bool isJoin = false;
    public Dictionary<Guid, GameObject> playerList = new Dictionary<Guid, GameObject>();

    public InputField nameText;
    public InputField passText;

    public string name;
    public string password;
    public int gameModeId = 1;

    /// <summary>
    /// TextにLogを表示
    /// </summary>
    public void TextLogs(string text)
    {
        //textLogs.text = $"{text}\n{textLogs.text}";
        Debug.Log(text);
    }

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
    /// ConnectionIdの取得
    /// </summary>
    public Guid GetConnectionId() => myConnectionId;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
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
    /// Gameシーンに移動ボタン
    /// </summary>
    public async void JointoNextScene(string name)
    {
        SceneManager.LoadScene(name);
        await RoomModel.I.JoinRoomAsync(SetNames());
    }

    /// <summary>
    /// ルーム全取得
    /// </summary>
    public async void GetAllRoom(int gameModeid)
    {
        List<string> roomNames = await RoomModel.I.GetAllRoomNamesAsync(gameModeid);
        Debug.Log(roomNames);
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
