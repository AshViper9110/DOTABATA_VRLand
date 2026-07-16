using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(SyncObject))]
public class ButtonAttribute : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        SyncObject syncObject = (SyncObject)target;
        if (GUILayout.Button("Generate ObjectId")) {
            syncObject.GenerateObjectId();
        }
        else if(GUILayout.Button("Reset ObjectId")) {
            syncObject.ResetObjectId();
        }
    }
}

[CustomEditor(typeof(BlockBreakBlockObjectsManager))]
public class BlockObjectManagerAttribute : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        BlockBreakBlockObjectsManager bbBlockObjectManager = (BlockBreakBlockObjectsManager)target;
        if (GUILayout.Button("SaveJson")) {
            bbBlockObjectManager.CreateJson();
        }
        else if (GUILayout.Button("LoadAllJson")) {
            bbBlockObjectManager.LoadAllJsonFromProjectsFile();
        }
    }
}
#endif