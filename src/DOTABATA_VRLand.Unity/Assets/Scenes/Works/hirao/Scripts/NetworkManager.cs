using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : Singleton<NetworkManager>
{
    public GameObject SyncPlayerPrefab;
    public Guid myConnectionId;
    public bool isJoin = false;
    public int gameModeId = 0;//フリープレイ:0/大会モード:1

    [Header("Sound Sync")]
    [SerializeField] private AudioSource myAudioSource;
    [SerializeField] private List<AudioClip> audioClips;

    // 入室したプレイヤーにつけるマテリアル
    [SerializeField] private List<PlayerSetMaterial> playerSetMaterials = new List<PlayerSetMaterial>();

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
        if (RoomModel.I != null)
        {
            RoomModel.I.OnJoinedUser += OnJoinedUser;
            RoomModel.I.OnLeavedUser += OnLeavedUser;
            RoomModel.I.OnUpdatedUserTransfrom += OnSyncPlayer;
            RoomModel.I.OnGetMiniGameRanking += OnGetMiniGameRanking;
            RoomModel.I.OnGetRanking += OnGetRanking;
            RoomModel.I.OnHostProgressed += OnHostProgress;
            RoomModel.I.onUpdateNit += OnUpdateNit;
            RoomModel.I.OnAudioAsyncAction += OnAudioAsync;
        }
    }

    private void OnDisable()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnJoinedUser -= OnJoinedUser;
            RoomModel.I.OnLeavedUser -= OnLeavedUser;
            RoomModel.I.OnAudioAsyncAction -= OnAudioAsync;
        }
    }

    private void OnDestroy()
    {
        isJoin = false;
        OnDisable();
    }

    private async void Start()
    {
        if (RoomModel.I != null)
        {
            await UserModel.I.CreateUserModel();
            await RoomModel.I.ConnectAsync();
            myConnectionId = RoomModel.I.ConnectionId;
        }
    }

    /// <summary>
    /// Gameシーンに移動ボタン
    /// </summary>
    public async Task JointoNextScene(string scene, ulong steamID, RoomConfig roomConfig)
    {
        await RoomModel.I.JoinRoomAsync(steamID, roomConfig);

        await Cysharp.Threading.Tasks.UniTask.WaitUntil(() =>
            InRoomPlayerData.I.PlayerList.ContainsKey(myConnectionId)
        );

        isJoin = true;
        SyncPlayer syncPlayer = GameObject.Find("Player(Clone)").GetComponent<SyncPlayer>();
        syncPlayer.isLocalPlayer = true;

        SceneManager.LoadScene(scene);
    }

    /// <summary>
    /// Roomに参加ボタン
    /// </summary>
    public async Task JointoRoom(ulong steamID, RoomConfig roomConfig)
    {
        await RoomModel.I.JoinRoomAsync(steamID, roomConfig);

        await Cysharp.Threading.Tasks.UniTask.WaitUntil(() =>
            InRoomPlayerData.I.PlayerList.ContainsKey(myConnectionId)
        );

        isJoin = true;
        gameModeId = roomConfig.GameModeId;
        SyncPlayer syncPlayer = GameObject.Find("Player(Clone)").GetComponent<SyncPlayer>();
        syncPlayer.isLocalPlayer = true;
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
            TMP_Text text = player.transform.Find("Canvas/NameTag").GetComponent<TMP_Text>();
            text.text = user.Name;
            SyncPlayer syncPlayer = player.GetComponent<SyncPlayer>();
            syncPlayer.SetConnectionId(user.ConnectionId);
            PlayerData data = new PlayerData()
            {
                playerObj = player,
                joinedUser = user,
            };
            InRoomPlayerData.I.AddPlayer(user.ConnectionId, data);

            PlayerSetMaterial setMaterial = playerSetMaterials.First(_=>_.playerConId == Guid.Empty);
            setMaterial.playerConId = user.ConnectionId;

            player.GetComponentsInChildren<MeshRenderer>()
            .Where(_ => _.gameObject.name == "Head" ||
            _.gameObject.name == "LeftHand" ||
            _.gameObject.name == "RightHand")
            .ToList()
            .ForEach(_ => _.material = setMaterial.material);
        }
        else
        {
            GameObject player = GameObject.Find("Player(Clone)");
            player.transform.position = new Vector3((user.JoinOrder * 2f) - 4.85f, 0, 0);
            TMP_Text text = player.transform.Find("Canvas/NameTag").GetComponent<TMP_Text>();
            text.text = user.Name;
            PlayerData data = new PlayerData()
            {
                playerObj = player,
                joinedUser = user,
            };

            data.playerObj.GetComponent<SyncPlayer>().SetConnectionId(user.ConnectionId);

            InRoomPlayerData.I.AddPlayer(user.ConnectionId, data);
            InRoomPlayerData.I.SetMySelf(data);
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
        if (connectionId == myConnectionId) return;
            TextLogs($"ConnectionId：{connectionId} が退室");
        InRoomPlayerData.I.RemovePlayer(connectionId);

        playerSetMaterials.First(_ => _.playerConId == connectionId).playerConId = Guid.Empty;
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

        gameManager.miniRankingList[user.ConnectionId]= rank;

        
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
        Debug.Log("らんきんぐしゅとく");
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        int index = 0;
        foreach (JoinedUser user in ranking)
        {
            Debug.Log(winCount[index]);
            gameManager.RankingList[user.JoinOrder] = index+1;
            gameManager.playerWinlist[user.JoinOrder] = winCount[index];
            gameManager.SetCrown(user.ConnectionId,user.JoinOrder);
            gameManager.SetRankText(user.ConnectionId, user.JoinOrder);
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
    /// ミニゲーム大会の司会進行通知送信
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

    public void OnAudioAsync(int id)
    {
        myAudioSource.PlayOneShot(audioClips[id]);
    }
}
