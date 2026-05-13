using DG.Tweening.Core.Easing;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using UnityEngine;

public class SyncObject : MonoBehaviour {
    [SerializeField, Header("作成通知を送信")] private bool SendCreate = false;


    [SerializeField, Header("Transform情報を送信")] private bool SendTransform = false;

    // 送る間隔
    [SerializeField] private float SendSpan = 0.2f;
    private float sendTimer = 0f;

    [SerializeField, Header("削除通知を送信")] private bool SendDestroy = false;

    // オブジェクトId
    private Guid objectId = Guid.Empty;
    public Guid ObjectId {
        get { return objectId; }
        set { objectId = value; }
    }

    // 作成者のコネクションId
    private Guid createrId = Guid.Empty;
    public Guid CreaterId {
        get { return createrId; }
        set { createrId = value; }
    }

    private void Awake() {
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
        if (RoomModel.I == null ||
            !RoomModel.I.IsJoinRoom ||
            !SendCreate ||
            objectId != Guid.Empty) {
            return;
        }

        createrId = RoomModel.I.ConnectionId;
        objectId = await RoomModel.I.CreateObjectAsync(this.transform.ToSimpleTransform(), this.gameObject.name);
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
