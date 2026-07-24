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
    SerializedProperty previewJsonName;

    public void OnEnable() {
        previewJsonName = serializedObject.FindProperty("previewJsonName");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "previewJsonName");

        BlockBreakBlockObjectsManager bbBlockObjectManager = (BlockBreakBlockObjectsManager)target;
        if (GUILayout.Button("SaveJson")) {
            bbBlockObjectManager.CreateJson();
        }
        if (GUILayout.Button("LoadAllJson")) {
            bbBlockObjectManager.LoadAllJsonFromProjectsFile();
        }

        EditorGUILayout.PropertyField(previewJsonName);

        if (GUILayout.Button("PreviewObject")) {
            bbBlockObjectManager.PreviewJsonObject();
        }

        if (GUILayout.Button("DestroyObjects")) {
            bbBlockObjectManager.DestroyObjects();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif