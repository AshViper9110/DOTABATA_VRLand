using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;
using static GestureRecognizer;

public class ArcanaGameManager : MonoBehaviour {
    [SerializeField] private GestureRecognizer gestureRecognizer;

    private PlayerData mySelf;

    // スポーツ位置
    [SerializeField] private List<Transform> spawnPoints;

    // 魔法のPrefab
    [SerializeField] private GameObject magicBallPrefab;
    // 魔法のVFX
    [SerializeField] private List<GameObject> magicVFXList;

    // 自分のコントロ
    private ArcanaPlayerController myController;

    private void Awake() {
        gestureRecognizer.CompleteRecognize += CreateMagic;
    }

    private async void Start() {
        RoomModel.I.OnDead += OnDead;
        RoomModel.I.OnArcanaGameSeted += OnArcanaGameSeted;

        // 自分のインスタンスを保持
        mySelf = InRoomPlayerData.I.MySelf;
        if (mySelf.joinedUser.JoinOrder == 1) {
            // サーバーのArcanaContextを初期化
            await RoomModel.I.ArcanaInitGameAsync();
        }

        // プレイヤーにScriptを付与
        mySelf.playerObj.AddComponent<PlayerStatus>().SetGameManager(this);
        myController = mySelf.playerObj.AddComponent<ArcanaPlayerController>();

        // スポーン位置に移動
        mySelf.playerObj.transform.position = spawnPoints[mySelf.joinedUser.JoinOrder - 1].position;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnDead -= OnDead;
            RoomModel.I.OnArcanaGameSeted -= OnArcanaGameSeted;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void Update() {
        
    }

    /// <summary>
    /// 魔法生成
    /// </summary>
    private void CreateMagic(GestureClass gesture, float score) {
        // 魔法生成
        Transform createdTransform = Instantiate(magicBallPrefab, new Vector3(0, 10, 0), Quaternion.identity).transform;
        int rnd = UnityEngine.Random.Range(0, magicVFXList.Count);
        // VFX生成
        Instantiate(magicVFXList[rnd], createdTransform);

        myController.SetMagicObj(createdTransform.gameObject);
    }

    /// <summary>
    /// 志望動機
    /// </summary>
    public async void DeathAsync() {
        await RoomModel.I.DeathAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// 死亡通知
    /// </summary>
    public void OnDead(Guid connectionId) {
        InRoomPlayerData.I.PlayerList[connectionId].playerObj.SetActive(false);
    }

    /// <summary>
    /// [サーバー通知]
    /// アルカナスケッチのゲーム終了
    /// </summary>
    public void OnArcanaGameSeted(Guid winnerConId) {
        Debug.Log($"勝者は{InRoomPlayerData.I.PlayerList[winnerConId].joinedUser.Name}");

        Destroy(InRoomPlayerData.I.MySelf.playerObj.GetComponent<PlayerStatus>());
        Destroy(myController);
    }
}
