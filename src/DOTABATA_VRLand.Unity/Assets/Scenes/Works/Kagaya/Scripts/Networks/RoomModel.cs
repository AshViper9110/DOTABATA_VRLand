using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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

    /// <summary>
    /// オブジェクトの所有権削除通知
    /// </summary>
    public Action<Guid> OnDeleatedOwnership { get; set; }


    /// <summary>
    /// ミニゲームの順位取得通知
    /// </summary>
    public Action<JoinedUser,int> OnGetMiniGameRanking { get; set; }

    /// <summary>
    /// 順位取得通知
    /// </summary>
    public Action<List<JoinedUser>, List<int>> OnGetRanking { get; set; }

    public Action OnHostProgressed { get; set; }

    /// <summary>
    /// 個人準備完了状態切り替え通知
    /// </summary>
    public Action<JoinedUser[], bool[]> OnUpdatedReadyStateAction { get; set; }

    /// <summary>
    /// 全員準備完了状態通知
    /// </summary>
    public Action<bool> OnUpdatedAllReadyStateAction { get; set; }

    /// <summary>
    /// カウントダウン通知
    /// </summary>
    public Action<int> OnCountdownAction { get; set; }

    /// <summary>
    /// ミニゲーム順位通知
    /// </summary>
    public Action<List<JoinedUser>> OnRegisterScoreAction { get; set; }

    /// <summary>
    /// シーン移行完了通知
    /// </summary>
    public Action<Guid> OnCompletedSceneTransition { get; set; }

    /// <summary>
    /// 全員のシーン移行完了通知
    /// </summary>
    public Action OnAllCompletedSceneTransition { get; set; }

    public Action<Guid, float> onUpdateNit { get; set; }
    public Action OnGameStartAction { get; set; }

    /// <summary>
    /// アルカナスケッチの死亡通知
    /// </summary>
    public Action<Guid> OnDead {  get; set; }
    public Action OnRoomStarted {  get; set; }

    /// <summary>
    /// アルカナスケッチのゲーム終了通知
    /// </summary>
    public Action<Guid> OnArcanaGameSeted { get; set; }

    /// <summary>
    /// 絵描き板の表示非表示同期通知
    /// </summary>
    public Action<Guid, bool> OnSwitchedDrawBoadActive { get; set; }

    /// <summary>
    /// 魔法オブジェクトのフィールド同期
    /// </summary>
    public Action<Guid, Guid, string> OnSyncdMagicBall { get; set; }

    /// <summary>
    /// [サーバー通知]
    /// プレイヤーのステータス同期通知
    /// </summary>
    public Action<Guid, int> OnSyncdPlayerStatus { get; set; }

    public Action<int> OnBallingNexted { get; set; }

    public Action<Guid> OnHitingDodgeBall { get; set; }
    public Action<Guid> OnHitingBomber { get; set; }
    public Action<Guid> OnOpenedShutter { get; set; }

    public Action<string> OnSelectedFreeMinigame { get; set; }

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
    public async UniTask JoinRoomAsync(ulong steamID, RoomConfig roomConfig) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        try {
            JoinedUser[] joinedUsers = await roomHub.JoinRoomAsync(steamID, roomConfig);
            isJoinRoom = true;
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
        NetworkManager.I.isJoin = false;

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
        if (playerTransform != null) await roomHub.UpdateUserTransformAsync(playerTransform);
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
    /// 個人準備完了状態切り替え
    /// </summary>
    public async Task OnGameStartAsync()
    {
        await roomHub.GameStartAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// ゲームスタート通知
    /// </summary>
    public void OnGameStart() {

        OnGameStartAction?.Invoke();

    }

    /// <summary>
    /// 個人準備完了状態切り替え
    /// </summary>
    public async Task SendReadyState(bool isReady)
    {
        await roomHub.UpdateReadyStateAsync(isReady);
    }

    /// <summary>
    /// [サーバー通知]
    /// 個人準備完了状態切り替え
    /// </summary>
    public void OnUpdateReadyState(JoinedUser[] users, bool[] isReadyList)
    {
        OnUpdatedReadyStateAction?.Invoke(
            users,
            isReadyList);
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

        OnUpdatedAllReadyStateAction?.Invoke(
            isAllReady);
    }

    /// <summary>
    /// カウントダウン開始
    /// </summary>
    public async Task StartCountdown()
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

        OnCountdownAction?.Invoke(count);

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

        OnRegisterScoreAction ?.Invoke(rankOrder);
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
    public void OnGetAllRoundRanking(List<JoinedUser> ranking,List<int> winCount)
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            Debug.Log($"{i + 1}位: ID:{ranking[i].JoinOrder}  {ranking[i].Name} 勝利数: {winCount[i]}");
        }
        // 順位表示UIの更新など
        OnGetRanking(ranking,winCount);
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
    public void OnGetLastMiniGameRanking(JoinedUser user,int lastRank)
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
        Debug.Log($"プレイヤー:{user.Name} 最終順位: {lastRank}位");

        OnGetMiniGameRanking(user,lastRank);



    }

    /// <summary>
    /// 勝利カウントUP
    /// </summary>
    public async void RequestWinCountUp(Guid connectionId)
    {
        await roomHub.WinCountUpAsync(connectionId);
    }

    /// <summary>
    /// [サーバー通知]
    /// 勝利カウントUP
    /// </summary>
    public void OnWinCountUp(JoinedUser user, int winCount)
    {
        Debug.Log($"{user.Name} の勝利数: {winCount}");
        // UIの更新など
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        manager.AddCrown(user.ConnectionId, user.JoinOrder);
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

    /// <summary>
    /// ミニゲーム大会の司会進行
    /// </summary>
    public async void HostProgress()
    {
       await roomHub.HostProgress();
    }


    /// <summary>
    /// [サーバー通知]
    /// ミニゲーム大会の司会進行
    /// </summary>
    public void OnHostProgress()
    {
        OnHostProgressed?.Invoke();
    }

    /// <summary>
    /// 所有権を取得する
    /// </summary>
    public async UniTask<bool> GetOwnershipAsync(Guid objectId, bool forcibly = false) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        return await roomHub.GetOwnershipAsync(objectId, forcibly);
    }

    /// <summary>
    /// 所有権を放棄する
    /// </summary>
    public async UniTask OwnershipAbandonmentAsync(Guid objectId) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.OwnershipAbandonmentAsync(objectId);
    }

    /// <summary>
    /// [サーバー通知]
    /// 所有者削除通知
    /// </summary>
    public void OnDeleateOwnership(Guid objectId) {
        if(OnDeleatedOwnership != null) {
            OnDeleatedOwnership(objectId);
        }
    }

    /// <summary>
    /// ニット生成とポイント更新
    ///</summary>
    public void UpdateNit(Guid id,float point)
    {
        roomHub.UpdateNit(id, point);
    }

    /// <summary>
    /// [サーバー通知]
    /// ニット生成とポイント更新
    ///</summary>
    public void OnUpdateNit(Guid id, float point)
    {
       
        onUpdateNit(id, point);
    }

    /// <summary>
    /// シーン移行が完了したことを他プレイヤーに伝える
    /// </summary>
    public async UniTask CompleteSceneTransition() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.CompleteSceneTransition();
    }

    /// <summary>
    /// [サーバー通知]
    /// シーン移行完了通知
    /// </summary>
    public void OnCompleteSceneTransition(Guid connectionId) {
        if (OnCompletedSceneTransition != null) {
            OnCompletedSceneTransition(connectionId);
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// 全員のシーン移行完了通知
    /// </summary>
    public void OnAllCompleteSceneTransition() {
        if (OnAllCompletedSceneTransition  != null) {
            OnAllCompletedSceneTransition();
        }
    }

    /// <summary>
    /// アルカナスケッチの初期化
    /// </summary>
    public async UniTask ArcanaInitGameAsync() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.ArcanaInitGameAsync();
    }

    /// <summary>
    /// 死亡同期
    /// </summary>
    public async UniTask DeathAsync() {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.DeathAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// 死亡通知
    /// </summary>
    public void OnDeath(Guid connectionId) {
        if (OnDead != null) {
            OnDead(connectionId);
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// アルカナスケッチのゲーム終了通知
    /// </summary>
    public void OnArcanaGameSet(Guid winnerConId) {
        if (OnArcanaGameSeted != null) {
            OnArcanaGameSeted(winnerConId);
        }
    }

    /// <summary>
    /// 魔法オブジェクトのフィールド同期
    /// </summary>
    public async UniTask SyncMagicBallAsync(Guid objectId, string gestureClassName) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.SyncMagicBallAsync(objectId, gestureClassName);
    }

    /// <summary>
    /// [サーバー通知]
    /// 魔法オブジェクトのフィールド同期
    /// </summary>
    public void OnSyncMagicBall(Guid objectId, Guid createrConId, string gestureClassName) {
        if (OnSyncdMagicBall != null) {
            OnSyncdMagicBall(objectId, createrConId, gestureClassName);
        }
    }

    /// <summary>
    /// 絵描き板の表示非表示同期
    /// </summary>
    public async UniTask SwitchDrawBoadActiveAsync(bool active) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.SwitchDrawBoadActiveAsync(active);
    }

    /// <summary>
    /// [サーバー通知]
    /// 絵描き板の表示非表示同期通知
    /// </summary>
    public void OnSwitchDrawBoadActive(Guid playerConId, bool active) {
        if (OnSwitchedDrawBoadActive != null) {
            OnSwitchedDrawBoadActive(playerConId, active);
        }
    }

    /// <summary>
    /// プレイヤーのステータス同期
    /// </summary>
    public async UniTask SyncPlayerStatusAsync(int hp) {
        if (roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }

        await roomHub.SyncPlayerStatusAsync(hp);
    }

    /// <summary>
    /// [サーバー通知]
    /// プレイヤーのステータス同期通知
    /// </summary>
    public void OnSyncPlayerStatus(Guid playerConId, int hp) {
        if (OnSyncdPlayerStatus  != null) {
            OnSyncdPlayerStatus(playerConId, hp);
        }
    }

    public async UniTask RoomStart()
    {
        if(roomHub == null) {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.RoomStart();
    }

    public void OnRoomStart()
    {
        if (OnRoomStarted != null)
        {
            OnRoomStarted();
        }
    }

    public async UniTask BallingNext()
    {
        if (roomHub == null)
        {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.BallingNext();
    }

    public void OnBallingNext(int order)
    {
        if (OnBallingNexted != null)
        {
            OnBallingNexted(order);
        }
    }

    public async UniTask HitDodgeBall()
    {
        if (roomHub == null)
        {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.HitDodgeBall(NetworkManager.I.myConnectionId);
    }

    public void OnHitDodgeBall(Guid connectionId)
    {
        if(OnHitingDodgeBall != null)
        {
            OnHitingDodgeBall(connectionId);
        }
    }

    public async UniTask HitBomber()
    {
        if (roomHub == null)
        {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.HitBomber(NetworkManager.I.myConnectionId);
    }

    public void OnHitBomber(Guid connectionId)
    {
        if (OnHitingBomber != null)
        {
            OnHitingBomber(connectionId);
        }
    }

    public async UniTask OpenShutter()
    {
        if (roomHub == null)
        {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.OpenShutter(NetworkManager.I.myConnectionId);

    }
    public void OnOpenShutter(Guid connectionId)
    {
        if (OnOpenedShutter != null)
        {
            OnOpenedShutter(connectionId);
        }
    }
    
    public async UniTask SelectFreeMinigame(string name)
    {
        if (roomHub == null)
        {
            throw new Exception("RoomHubがnullです。");
        }
        await roomHub.SelectFreeMinigame(name);
    }

    public void OnSelectFreeMinigame(string name)
    {
        if(OnSelectedFreeMinigame != null)
        {
            OnSelectedFreeMinigame(name);
        }
    }
}
