using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SyncObejctData")]
public class SyncObjectDataSO : ScriptableObject {
    public List<ObjectData> syncObjectDataList = new List<ObjectData>();

    private void OnValidate() {
        for (int i = 0; i < syncObjectDataList.Count; i++) {
            syncObjectDataList[i].objectListId = i + 1;
            syncObjectDataList[i].syncObject.GetComponent<SyncObject>().SetSyncObjectListId(i + 1);
        }
    }
}

[System.Serializable]
public class ObjectData {
    [ReadOnly] public int objectListId;
    public GameObject syncObject;
}
