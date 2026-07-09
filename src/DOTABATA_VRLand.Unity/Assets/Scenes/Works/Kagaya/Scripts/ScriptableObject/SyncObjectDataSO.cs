using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SyncObejctData")]
public class SyncObjectDataSO : ScriptableObject {
    public enum Minigames {
        None = 0,
        ArcanaSketch,
        NitNit,
        Bowling,
        Kinko,
        BombDodge,
        PanicSoda
    }

    public Minigames minigame = Minigames.None;
    public List<ObjectData> syncObjectDataList = new List<ObjectData>();

    private void OnValidate() {
        for (int i = 0; i < syncObjectDataList.Count; i++) {
            syncObjectDataList[i].objectListId = i + 1;
            if (!syncObjectDataList[i].syncObject) continue;
            SyncObject syncObject = syncObjectDataList[i].syncObject.GetComponent<SyncObject>();
            if (syncObject) {
                syncObject.SetMinigameAndListId(minigame, i + 1);
            }
        }

        int listCount = syncObjectDataList.Count;
        if (listCount < 2) return;
        
        if (syncObjectDataList[listCount - 1].syncObject == syncObjectDataList[listCount - 2].syncObject) {
            syncObjectDataList[listCount - 1].syncObject = null;
        }
    }
}

[System.Serializable]
public class ObjectData {
    [ReadOnly] public int objectListId;
    public GameObject syncObject;
}
