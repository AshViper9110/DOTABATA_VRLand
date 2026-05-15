using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class RoomModel : Singleton<RoomModel>, IRoomHubReceiver {
    [SerializeField] private ServerConfigSO serverConfig;

    private GrpcChannelx channelx;
    private IRoomHub roomHub;

    /// <summary>
    /// 　接続ID
    /// </summary>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// MagicOnionに接続しているか
    /// </summary>
    private bool isConnected = false;
    public bool IsConnected { get { return isConnected; } }

    /// <summary>
    /// ロームに入っているか
    /// </summary>
    private bool isJoinRoom = false;
    public bool IsJoinRoom { get  { return isJoinRoom; } }

    /*
     * サーバー通知
     */

    /// <summary>
    /// ユーザー接続通知
    /// </summary>
    public Action<JoinedUser> OnJoinedUser { get; set; }
    /// <summary>
    /// ユーザー退出通知
    /// </summary>
    public Action<Guid, int> OnLeavedUser { get; set; }


    /// <summary>
    /// ユーザーのTransfrom通知
    /// </summary>
    public Action<Guid, PlayerTransformDTO> OnUpdatedUserTransfrom { get; set; }

    /// <summary>
    /// ミニゲーム選択通知
    /// </summary>
    public Action<int> OnSelectedMiniGame { get; set; }

    /// <summary>
    /// オブジェクト作成通知
    /// </summary>
    public Action<Guid, Guid, SimpleTransform, int> OnCreatedObject { get; set; }

    /// <summary>
    /// オブジェクトのTransform通知
    /// </summary>
    public Action<Guid, SimpleTransform> OnUpdatedObjectTransform { get; set; }

    /// <summary>
    /// オブジェクトの削除通知
    /// </summary>
    public Action<Guid> OnDestroyedObject { get; set; }

    /*
     * 処理
     */

    /// <summary>
    /// 　MagicOnion接続処理
    /// </summary>
    public async UniTask ConnectAsync() {
        channelx = GrpcChannelx.ForAddress(
#if DEBUG
            serverConfig.DEBUG.url
#else
            serverConfig.PRODUCTION.url
#endif
            );
        roomHub = await StreamingHubClient.
             ConnectAsync<IRoomHub, IRoomHubReceiver>(channelx, this);
        this.ConnectionId = await roomHub.GetConnectionId();
        isConnected = true;
    }

    /// <summary>
    /// MagicOnion切断処理
    /// </summary>
    public async UniTask DisconnectAsync() {
        isConnected = false;
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channelx != null) await channelx.ShutdownAsync();
        roomHub = null;
        channelx = null;
    }
    /// <summary>
    /// 破棄処理
    /// </summary>
    protected override void OnDestroy() {
        base.OnDestroy();
        DisconnectAsync().Forget();
    }

    /// <summary>
    /// ゲーム終了時
    /// </summary>
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        DisconnectAsync().Forget();
    }

    /// <summary>
    /// ルームを全取得
    /// </summary>
    public async UniTask<List<RoomInfo>> GetAllRoomAsync() {
        return await roomHub.GetAllRoomAsync();
    }


    /// <summary>
    /// ルームに入室
    /// </summary>
    public async UniTask JoinRoomAsync(string userName, RoomConfig roomConfig) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        try {
            JoinedUser[] joinedUsers = await roomHub.JoinRoomAsync(userName, roomConfig);
            isJoinRoom = true;
            InRoomPlayerData.I.SetMySelf(joinedUsers.First(_=>_.ConnectionId == ConnectionId));
            if (joinedUsers != null) {
                foreach (var user in joinedUsers) {
                    // 自分自身はスキップ
                    if (user.ConnectionId != ConnectionId) {
                        OnJoinedUser(user);
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }

    }


    /// <summary>
    /// [サーバー通知]
    /// ロビーの入室通知
    /// </summary>
    public void OnJoinRoom(JoinedUser user) {
        if (OnJoinedUser != null) {
            OnJoinedUser(user);
        }
    }

    /// <summary>
    /// ルームから退室
    /// </summary>
    public async UniTask LeaveRoomAsync() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        isJoinRoom = false;

        await roomHub.LeaveRoomAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの退室通知
    /// </summary>
    public void OnLeaveRoom(Guid connectionId, int joinOrder) {
        if (OnLeavedUser != null) {
            OnLeavedUser(connectionId, joinOrder);
        }
    }


    /// <summary>
    /// ユーザーのTransform同期
    /// </summary>
    public async UniTask UpdateUserTransformAsync(PlayerTransformDTO playerTransform) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.UpdateUserTransformAsync(playerTransform);
    }

    /// <summary>
    /// [サーバー通知]
    /// ユーザーのTransfrom通知
    /// </summary>
    public void OnUpdateUserTransform(Guid connectionId, PlayerTransformDTO playerTransform) {
        if (OnUpdatedUserTransfrom != null) {
            OnUpdatedUserTransfrom(connectionId, playerTransform);
        }
    }

    /// <summary>
    /// ミニゲームの選択
    /// </summary>
    public async UniTask SelectMiniGameAsync(int miniGameId){
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.SelectMiniGameAsync(miniGameId);
    }

    /// <summary>
    /// [サーバー通知]
    /// ミニゲーム選択通知
    /// </summary>
    public void OnSelectMiniGame(int miniGameId) {
        if (OnSelectedMiniGame != null) {
            OnSelectedMiniGame(miniGameId);
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// ゲームスタート通知
    /// </summary>
    public void OnGameStart() {

    }

    /*
     * オブジェクト
     */

    /// <summary>
    /// オブジェクト生成
    /// </summary>
    public async UniTask<Guid> CreateObjectAsync(SimpleTransform createdTransform, int objectListId) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        return await roomHub.CreateObjectAsync(createdTransform, objectListId);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクト作成通知
    /// </summary>
    public void OnCreateObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, int objectListId) {
        if (OnCreatedObject != null) {
            OnCreatedObject(objectId, createrConnectionId, createdTransform, objectListId);
        }
    }

    /// <summary>
    /// オブジェクトリストに追加
    /// </summary>
    public async UniTask AddObjectListAsync(Guid objectId, int objectListId, SimpleTransform simpleTransform) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.AddObjectListAsync(objectId, objectListId, simpleTransform);
    }

    /// <summary>
    /// オブジェクトのTransform同期
    /// </summary>
    public async UniTask UpdateObjectTransformAsync(Guid objectId, SimpleTransform sTransform) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.UpdateObjectTransformAsync(objectId, sTransform);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトのTransform通知
    /// </summary>
    public void OnUpdateObjectTransform(Guid objectId, SimpleTransform sTransform) {
        if (OnUpdatedObjectTransform != null) {
            OnUpdatedObjectTransform(objectId, sTransform);
        }
    }

    /// <summary>
    /// オブジェクトの削除
    /// </summary>
    public async UniTask DestroyObjectAsync(Guid objectId) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.DestroyObjectAsync(objectId);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトの削除通知
    /// </summary>
    public void OnDestroyObject(Guid objectId) {
        if (OnDestroyedObject != null) {
            OnDestroyedObject(objectId);
        }
    }
}
