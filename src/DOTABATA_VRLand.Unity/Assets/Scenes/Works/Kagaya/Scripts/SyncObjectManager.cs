using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SyncObjectManager : MonoBehaviour {
    // 同期するオブジェクト
    [SerializeField] private SyncObjectDataSO syncObjectData;

    // 同期するオブジェクトリスト
    private Dictionary<Guid, SyncObject> syncObjectDataList = new Dictionary<Guid, SyncObject>();
    public Dictionary<Guid, SyncObject> SyncObjectDataList {
        get { return syncObjectDataList; }
        set {  syncObjectDataList = value; }
    }

    private void Awake() {
        RoomModel.I.OnCreatedObject += OnCreatedObject;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnCreatedObject -= OnCreatedObject;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    /// <summary>
    /// [サーバー通知]
    /// オブジェクト作成通知
    /// </summary>
    public void OnCreatedObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, string objecName) {
        GameObject createSyncObject = syncObjectData.syncObjectDataList.First(_=> objecName.StartsWith(_.syncObject.name)).syncObject;
        GameObject createdObj = Instantiate(
            createSyncObject,
            createdTransform.localPosition,
            createdTransform.localRotation
            );

        // フィールド設定
        SyncObject syncObject = createdObj.GetComponent<SyncObject>();
        syncObject.ObjectId = objectId;
        syncObject.CreaterId = createrConnectionId;
        syncObject.ApplyGuidToInspector();
    }
}
