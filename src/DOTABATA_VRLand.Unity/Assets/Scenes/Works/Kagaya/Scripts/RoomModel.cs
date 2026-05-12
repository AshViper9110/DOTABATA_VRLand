using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoomModel : Singleton<RoomModel>, IRoomHubReceiver {
    [SerializeField] private ServerConfigSO serverConfig;

    private GrpcChannelx channelx;
    private IRoomHub roomHub;

    /// <summary>
    /// 　接続ID
    /// </summary>
    public Guid ConnectionId { get; private set; }

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
    }

    /// <summary>
    /// MagicOnion切断処理
    /// </summary>
    public async UniTask DisconnectAsync() {
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
    /// ゲームモードを指定してルーム名を全取得
    /// </summary>
    public async UniTask<List<string>> GetAllRoomNamesAsync(int gameModeId) 
    {
        return await roomHub.GetAllRoomNamesAsync(gameModeId);
    }



    /// <summary>
    /// ルームに入室
    /// </summary>
    public async UniTask JoinRoomAsync(RoomConfig roomConfig) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        try {
                JoinedUser[] joinedUsers = await roomHub.JoinRoomAsync("TestUser", roomConfig);
                if (joinedUsers != null)
                {
                    foreach (var user in joinedUsers)
                    {
                        // 自分自身はスキップ
                        if (user.ConnectionId != ConnectionId)
                        {
                            OnJoinedUser(user);
                        }
                    }
                }
            }
            catch(Exception e) {
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
}
