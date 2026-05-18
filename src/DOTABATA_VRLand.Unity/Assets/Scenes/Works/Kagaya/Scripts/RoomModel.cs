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
using UnityEditor.MemoryProfiler;
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

    /// <summary>
    /// 個人準備完了状態切り替え
    /// </summary>
    public async void SendReadyState(bool isReady)
    {
        await roomHub.UpdateReadyStateAsync(isReady);
    }

    /// <summary>
    /// [サーバー通知]
    /// 個人準備完了状態切り替え
    /// </summary>
    public void OnUpdateReadyState(JoinedUser updatedUser, bool isReady)
    {
        Debug.Log($"{updatedUser.Name}の準備状態: {isReady}");
    }

    /// <summary>
    /// [サーバー通知]
    /// 全員準備完了状態切り替え
    /// </summary>
    public void OnUpdateAllReadyState(bool isAllReady)
    {
        if (isAllReady)
        {
            Debug.Log("全員準備完了 → ゲーム開始");
        }
        else
        {
            Debug.Log("準備中のプレイヤーがいます");
        }
    }

    /// <summary>
    /// カウントダウン開始
    /// </summary>
    public async void StartCountdown()
    {
        await roomHub.StartCountdownAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// カウントダウン受け取り
    /// </summary>
    public void OnCountdown(int count)
    {
        Debug.Log($"カウント: {count}");
        // カウントダウンUIの更新
        // count == 0 でゲーム開始演出など
        if (count == 0)
        {
            Debug.Log("ゲームスタート");
        }
    }

    /// <summary>
    /// ミニゲーム結果送信
    /// </summary>
    /// <remarks>
    /// 制限時間の場合、Unity側でfloatをintに変換してから実行
    /// int result = (int)(remainingTime * 1000) でミリ秒に変換
    /// </remarks>
    public async void SendScore(int result)
    {
        await roomHub.RegisterScoreAsync(result);
    }

    /// <summary>
    /// [サーバー通知]
    /// ミニゲーム結果順位
    /// </summary>
    public void OnRegisterScore(List<JoinedUser> rankOrder)
    {
        // 順位表示UIの更新など
        for (int i = 0; i < rankOrder.Count; i++)
        {
            Debug.Log($"{i + 1}位: {rankOrder[i].Name}");
        }
    }

    /// <summary>
    /// ゲーム大会順位取得
    /// </summary>
    public async void RequestAllRoundRanking()
    {
        await roomHub.GetAllRoundRankingAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// ゲーム大会順位取得
    /// </summary>
    public void OnGetAllRoundRanking(List<JoinedUser> ranking)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            Debug.Log($"{i + 1}位: {ranking[i].Name}");
        }
        // 順位表示UIの更新など
    }

    /// <summary>
    /// プレイヤーの最終プレイ順位の取得
    /// </summary>
    public async void RequestLastRanking(Guid connectionId)
    {
        await roomHub.GetLastRankingAsync(connectionId);
    }

    /// <summary>
    /// [サーバー通知]
    /// プレイヤーの最終プレイ順位の取得
    /// </summary>
    public void OnGetLastMiniGameRanking(int lastRank)
    {
        if (lastRank == -99)
        {
            Debug.Log("対象プレイヤーが存在しません");
            return;
        }
        if (lastRank == -1)
        {
            Debug.Log("ランキングデータが存在しません");
            return;
        }
        Debug.Log($"最終順位: {lastRank}位");
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
//    public async void SendReadyState(bool isReady)
//    {
//        await _hubClient.UpdateReadyStateAsync(isReady);
//    }

   
//public void OnUpdateReadyState(JoinedUser updatedUser, bool isReady)
//    {
//        Debug.Log($"{updatedUser.Name}の準備状態: {isReady}");
//    }

//    public void OnUpdateAllReadyState(bool isAllReady)
//    {
//        if (isAllReady)
//        {
//            Debug.Log("全員準備完了 → ゲーム開始");
//        }
//        else
//        {
//            Debug.Log("準備中のプレイヤーがいます");
//        }
//    }



//public async void GameStart()
//    {
//        await _hubClient.GameStartAsync();
//    }

  



//public async void StartCountdown()
//    {
//        await _hubClient.StartCountdownAsync();
//    }


//public void OnCountdown(int count)
//    {
//        Debug.Log($"カウント: {count}");
//        // カウントダウンUIの更新
//        // count == 0 でゲーム開始演出など
//        if (count == 0)
//        {
//            Debug.Log("ゲームスタート");
//        }
//    }



//public async void SendScore(int result)
//    {
//        await _hubClient.RegisterScoreAsync(result);
//    }


//public void OnRegisterScore(List<JoinedUser> rankOrder)
//    {
//        // 順位表示UIの更新など
//        for (int i = 0; i < rankOrder.Count; i++)
//        {
//            Debug.Log($"{i + 1}位: {rankOrder[i].Name}");
//        }
//    }
    
    

//public async void RequestAllRoundRanking()
//    {
//        await _hubClient.GetAllRoundRankingAsync();
//    }


// public void OnGetAllRoundRanking(List<JoinedUser> ranking)
//    {
//        for (int i = 0; i < ranking.Count; i++)
//        {
//            Debug.Log($"{i + 1}位: {ranking[i].Name}");
//        }
//        // 順位表示UIの更新など
//    }
    
    

//public async void RequestLastRanking()
//    {
//        await _hubClient.GetLastRankingAsync(connectionId);
//    }


//public void OnGetLastMiniGameRanking(int lastRank)
//    {
//        if (lastRank == -99)
//        {
//            Debug.Log("対象プレイヤーが存在しません");
//            return;
//        }
//        if (lastRank == -1)
//        {
//            Debug.Log("ランキングデータが存在しません");
//            return;
//        }
//        Debug.Log($"最終順位: {lastRank}位");
//    }
}
