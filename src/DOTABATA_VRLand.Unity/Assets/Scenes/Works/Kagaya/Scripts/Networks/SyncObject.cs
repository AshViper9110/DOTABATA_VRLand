using Cysharp.Threading.Tasks;
using DG.Tweening.Core.Easing;
using DOTABATA_VRLand.Shared.Models.Entities;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static SyncObjectDataSO;

public class SyncObject : MonoBehaviour {
    [ReadOnly] public Minigames minigame = Minigames.None;
    [ReadOnly] public int objectListId = 0;

    [SerializeField, Header("作成通知を送信")] private bool SendCreate;


    [SerializeField, Header("Transform情報を送信")] private bool SendTransform;

    // 送る間隔
    [SerializeField] private float SendSpan = 0.1f;
    private float sendTimer = 0f;

    [SerializeField, Header("削除通知を送信")] private bool SendDestroy;

    // オブジェクトId
    private Guid objectId;
    public Guid ObjectId {
        get { return objectId; }
        set { objectId = value; }
    }

    // ヒエラルキーに表示する用
    [ReadOnly] public string stringObjectId;

    /// <summary>
    /// 所有者かどうか
    /// </summary>
    public bool IsOwner { get; private set; } = false;

    [ReadOnly] public bool Initialized = false;

    // 作成者のコネクションId
    private Guid createrId = Guid.Empty;
    public Guid CreaterId {
        get { return createrId; }
        set { createrId = value; }
    }

    /// <summary>
    /// オブジェクトIdを生成
    /// </summary>
    public void GenerateObjectId() {
        objectId = Guid.NewGuid();
        ApplyGuidToInspector();
    }

    /// <summary>
    /// オブジェクトIdをリセット
    /// </summary>
    public void ResetObjectId() {
        objectId= Guid.Empty;
        ApplyGuidToInspector();
    }

    /// <summary>
    /// オブジェクトIdをインスペクターに反映
    /// </summary>
    public void ApplyGuidToInspector() {
        if (objectId == Guid.Empty) {
            stringObjectId = string.Empty;
        }
        else {
            stringObjectId = objectId.ToString();
        }
    }

    /// <summary>
    /// ミニゲームとオブジェクトリストIdセット
    /// </summary>
    public void SetMinigameAndListId(Minigames minigame,int indexNum) {
        this.minigame = minigame;
        objectListId = indexNum;
    }

    private void Awake() {
        if (objectListId == 0) {
            Debug.LogError($"{this.gameObject.name}：objectListIdが指定されていません");
        }

        RoomModel.I.OnUpdatedObjectTransform += OnUpdatedObjectTransform;
        RoomModel.I.OnDestroyedObject += OnDestroyedObject;
        RoomModel.I.OnDeleatedOwnership += OnDeleatedOwnership;
    }

    private void Start() {
        SendCreateObjectAsync();
    }

    private void Update() {
        SendUpdateObjectTransformAsync();
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnUpdatedObjectTransform -= OnUpdatedObjectTransform;
            RoomModel.I.OnDestroyedObject -= OnDestroyedObject;
            RoomModel.I.OnDeleatedOwnership -= OnDeleatedOwnership;
        }
    }

    private void OnDestroy() {
        OnDisable();

        SendDestroyAsync();
    }

    /// <summary>
    /// オブジェクトリスト作成同期
    /// </summary>
    private async void SendCreateObjectAsync() {
        await UniTask.WaitUntil(()=> RoomModel.I != null && RoomModel.I.IsJoinRoom);

        if (!SendCreate &&
            stringObjectId != string.Empty) {
            Debug.Log(stringObjectId);
            objectId = Guid.Parse(stringObjectId);

            if (InRoomPlayerData.I.MySelf.joinedUser.JoinOrder == 1) {
                CreaterId = RoomModel.I.ConnectionId;
                IsOwner = true;
                await RoomModel.I.AddObjectListAsync(objectId, minigame, objectListId, this.transform.ToSimpleTransform());
            }
        }
        else if (SendCreate &&
            objectId == Guid.Empty) {
            createrId = RoomModel.I.ConnectionId;
            IsOwner = true;
            objectId = await RoomModel.I.CreateObjectAsync(this.transform.ToSimpleTransform(), minigame, objectListId);
            ApplyGuidToInspector();
        }

        Initialized = true;

    }

    /// <summary>
    /// Transform同期
    /// </summary>
    private async void SendUpdateObjectTransformAsync() {
        if (RoomModel.I == null ||
            !RoomModel.I.IsJoinRoom ||
            !SendTransform ||
            !IsOwner) {
            return;
        }

        sendTimer += Time.deltaTime;

        if (sendTimer >= SendSpan) {
            sendTimer = 0;
            await RoomModel.I.UpdateObjectTransformAsync(objectId, this.transform.ToSimpleTransform());
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトのTransform通知
    /// </summary>
    public void OnUpdatedObjectTransform(Guid objectId, SimpleTransform sTransform) {
        if (this.objectId != objectId) {
            return;
        }

        this.transform.ApplyTransform(sTransform, SendSpan);
    }

    /// <summary>
    /// オブジェクト削除同期
    /// </summary>
    private async void SendDestroyAsync() {
        if (RoomModel.I == null ||
            !RoomModel.I.IsJoinRoom ||
            !SendDestroy) {
            return;
        }

        await RoomModel.I.DestroyObjectAsync(objectId, false);
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクトの破棄通知
    /// </summary>
    private void OnDestroyedObject(Guid objectId) {
        if (this.objectId != objectId) {
            return;
        }

        SendDestroy = false;
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 所有権を取得
    /// </summary>
    public async UniTask<bool> GetOwnership(bool forcibly = false) {
        IsOwner = await RoomModel.I.GetOwnershipAsync(objectId, forcibly);

        return IsOwner;
    }

    /// <summary>
    /// 所有権を放棄する
    /// </summary>
    public async void OwnershipAbandonment() {
        await RoomModel.I.OwnershipAbandonmentAsync(objectId);
    }

    /// <summary>
    /// [サーバー通知]
    /// 所有者削除通知
    /// </summary>
    public void OnDeleatedOwnership(Guid objectId) {
        if (this.objectId != objectId) {
            return;
        }
        IsOwner = false;
    }

    public void invalidCreate()
    {
        SendCreate = false;
        SendDestroy = false;
        SendTransform = false;
    }
}
