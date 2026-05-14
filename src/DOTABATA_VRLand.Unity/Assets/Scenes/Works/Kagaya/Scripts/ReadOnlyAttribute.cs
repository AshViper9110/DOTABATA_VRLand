using UnityEditor;
using UnityEngine;

// インスペクターで使用する属性
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR

// 実際の表示を変更するドロワー
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.enabled = false;

        EditorGUI.PropertyField(position, property, label, true);

        GUI.enabled = true;
    }
}
#endif