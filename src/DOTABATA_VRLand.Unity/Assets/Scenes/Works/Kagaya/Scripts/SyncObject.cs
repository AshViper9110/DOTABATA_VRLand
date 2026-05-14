using Cysharp.Threading.Tasks;
using DG.Tweening.Core.Easing;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using System.Linq;
using UnityEngine;

public class SyncObject : MonoBehaviour {
    [ReadOnly] public int objectListId = 0;

    [SerializeField, Header("作成通知を送信")] private bool SendCreate;


    [SerializeField, Header("Transform情報を送信")] private bool SendTransform;

    // 送る間隔
    [SerializeField] private float SendSpan = 0.2f;
    private float sendTimer = 0f;

    [SerializeField, Header("削除通知を送信")] private bool SendDestroy;

    // オブジェクトId
    private Guid objectId;
    public Guid ObjectId {
        get { return objectId; }
        set { objectId = value; }
    }

    [ReadOnly] public string stringObjectId;

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
    /// オブジェクトリストIdセット
    /// </summary>
    public void SetSyncObjectListId(int indexNum) {
        objectListId = indexNum;
    }

    private void Awake() {
        if (objectListId == 0) {
            Debug.LogError($"{this.gameObject.name}：objectListIdが指定されていません");
        }

        RoomModel.I.OnUpdatedObjectTransform += OnUpdatedObjectTransform;
        RoomModel.I.OnDestroyedObject += OnDestroyedObject;
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

            if (InRoomPlayerData.I.MySelf.JoinOrder == 1) {
                CreaterId = RoomModel.I.ConnectionId;
                await RoomModel.I.AddObjectListAsync(objectId, objectListId, this.transform.ToSimpleTransform());
            }
        }
        else if (SendCreate &&
            objectId == Guid.Empty) {
            createrId = RoomModel.I.ConnectionId;
            objectId = await RoomModel.I.CreateObjectAsync(this.transform.ToSimpleTransform(), objectListId);
            ApplyGuidToInspector();
        }
    }

    /// <summary>
    /// Transform同期
    /// </summary>
    private async void SendUpdateObjectTransformAsync() {
        if (RoomModel.I == null ||
            !RoomModel.I.IsJoinRoom ||
            !SendTransform ||
            RoomModel.I.ConnectionId != createrId) {
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

        await RoomModel.I.DestroyObjectAsync(objectId);
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
}
