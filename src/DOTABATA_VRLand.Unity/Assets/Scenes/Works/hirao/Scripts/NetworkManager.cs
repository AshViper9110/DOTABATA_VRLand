using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>
{
    public GameObject SyncPlayerPrefab;
    public GameObject player;
    public Guid myConnectionId;
    public bool isJoin = false;

    /// <summary>
    /// TextにLogを表示
    /// </summary>
    public void TextLogs(string text)
    {
        //textLogs.text = $"{text}\n{textLogs.text}";
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
        RoomModel.I.OnGetMiniGameRanking += OnGetMiniGameRanking;
        RoomModel.I.OnGetRanking += OnGetRanking;
        RoomModel.I.OnHostProgressed += OnHostProgress;
        RoomModel.I.onUpdateNit += OnUpdateNit;
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
    }

    /// <summary>
    /// Gameシーンに移動ボタン
    /// </summary>
    public async Task JointoNextScene(string scene, string name, RoomConfig roomConfig)
    {
        await RoomModel.I.JoinRoomAsync(name, roomConfig);

        await Cysharp.Threading.Tasks.UniTask.WaitUntil(() =>
            InRoomPlayerData.I.PlayerList.ContainsKey(myConnectionId)
        );

        isJoin = true;
        SyncPlayer syncPlayer = player.GetComponent<SyncPlayer>();
        syncPlayer.isLocalPlayer = true;

        SceneManager.LoadScene(scene);
    }
    /// <summary>
    /// ルーム全取得
    /// </summary>
    public async void GetAllRoom(int gameModeid)
    {
        List<RoomInfo> roomNames = await RoomModel.I.GetAllRoomAsync();
        Debug.Log(roomNames);
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの入室通知
    /// </summary>
    private void OnJoinedUser(JoinedUser user)
    {
        TextLogs($"{user.Name}が入室");
        if (user.ConnectionId != myConnectionId)
        {
            GameObject player = Instantiate(SyncPlayerPrefab);
            SyncPlayer syncPlayer = player.GetComponent<SyncPlayer>();
            PlayerData data = new PlayerData()
            {
                playerObj = player,
                joinedUser = user,
            };
            InRoomPlayerData.I.AddPlayer(user.ConnectionId, data);
          
        }
        else
        {
            PlayerData data = new PlayerData()
            {
                playerObj = player,
                joinedUser = user,
            };
            InRoomPlayerData.I.AddPlayer(user.ConnectionId, data);
        }
    }

    /// <summary>
    /// 自身以外の同期
    /// </summary>
    private void OnSyncPlayer(Guid connectionId, PlayerTransformDTO data)
    {
        if (!InRoomPlayerData.I.PlayerList.ContainsKey(connectionId)) return;

        SyncPlayer player = InRoomPlayerData.I.PlayerList[connectionId].playerObj.GetComponent<SyncPlayer>();
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

    /// <summary>
    ///ミニゲームの順位要求
    /// </summary>
    public async void ReqestMinigameRanking(Guid guid)
    {
         RoomModel.I.RequestLastRanking(guid);
    }

    /// <summary>
    /// [サーバー通知]
    /// ミニゲームの順位取得通知
    /// </summary>
    public void OnGetMiniGameRanking(JoinedUser user,int rank)
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        gameManager.miniRankingList[user.JoinOrder + 1]= rank;
        Debug.Log($"{user.JoinOrder}:::{rank}");
        
        if (rank != 0)
        {
            gameManager.InitResult();
        }
        else
        {

        }
    }

    /// <summary>
    ///勝利数要求
    /// </summary>
    public async void ReqestRanking()
    {
        RoomModel.I.RequestAllRoundRanking();
    }

    /// <summary>
    /// [サーバー通知]
    /// ミニゲームの順位取得通知
    /// </summary>
    public void OnGetRanking(List<JoinedUser> ranking, List<int> winCount)
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        int index = 0;
        foreach (JoinedUser user in ranking)
        {
            gameManager.RankingList[user.JoinOrder] = index+1;
            GameManager.playerWinlist[user.JoinOrder] = winCount[index];
            gameManager.SetCrown(user.ConnectionId,user.JoinOrder);
            index++;
        }
    }


    /// <summary>
    ///ミニゲーム大会の司会進行通知送信
    /// </summary>
    public void SendHostProgress()
    {
        RoomModel.I.HostProgress();
    }


    /// <summary>
    /// [サーバー通知]
    /// ミニゲームの順位取得通知
    /// </summary>
    public void OnHostProgress()
    {

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.MoveText();
    }

    ///<summary>
    ///ニットの更新
    /// </summary>
    public void UpdateNit(Guid id,float point)
    {
        RoomModel.I.UpdateNit(id,point);
    }

    ///<summary>
    ///[サーバー通知]
    ///ニットの更新
    /// </summary>
    public void OnUpdateNit(Guid id,float point)
    {
        
        NitnitManager nitnitManager = GameObject.Find("GameManager").GetComponent<NitnitManager>();
        MufflerSetManager mufflerSet = nitnitManager.mufflerSets[InRoomPlayerData.I.PlayerList[id].joinedUser.JoinOrder-1];
     

       
            mufflerSet.addNit(point);
        
       
    }
}
