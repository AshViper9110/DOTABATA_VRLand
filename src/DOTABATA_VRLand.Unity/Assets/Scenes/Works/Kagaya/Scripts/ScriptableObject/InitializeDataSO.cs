using UnityEngine;

[CreateAssetMenu(menuName = "InitializeData")]
public class InitializeDataSO : ScriptableObject {
    public SerializableDictionary<string, GameObject> datas = new SerializableDictionary<string, GameObject>();
}
