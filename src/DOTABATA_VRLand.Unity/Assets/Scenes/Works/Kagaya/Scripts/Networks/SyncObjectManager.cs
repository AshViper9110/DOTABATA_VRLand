using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SyncObjectDataSO;

public class SyncObjectManager : MonoBehaviour {
    // 同期するオブジェクト
    [SerializeField] private List<SyncObjectDataSO> syncObjectData;

    // 同期するオブジェクトリスト
    private Dictionary<Guid, SyncObject> syncObjectDataList = new Dictionary<Guid, SyncObject>();
    public Dictionary<Guid, SyncObject> SyncObjectDataList {
        get { return syncObjectDataList; }
        set {  syncObjectDataList = value; }
    }

    private void Awake() {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnCreatedObject += OnCreatedObject;
        }
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
    public void OnCreatedObject(Guid objectId, Guid createrConnectionId, SimpleTransform createdTransform, Minigames minigame, int objectListId) {
        List<ObjectData> syncObjectDataList = syncObjectData.First(_ => _.minigame == minigame).syncObjectDataList;
        GameObject createSyncObject = syncObjectDataList.First(_=> _.objectListId == objectListId).syncObject;
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

        syncObject.Initialized = true;
    }
}
