using TMPro;
using UnityEngine;

public class ObjectSyncTest : MonoBehaviour {
    [SerializeField] private GameObject obj;
    [SerializeField] private TextMeshProUGUI objectNameText;

    private SyncObject targetObj;

    private async void Update() {
        if (!RoomModel.I.IsJoinRoom) {
            return;
        }

        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10.0f)) {
                Debug.Log(hit.transform.gameObject.name);
                targetObj = hit.transform.GetComponent<SyncObject>();
                if (targetObj) {
                    objectNameText.text = targetObj.name;
                }
                
            }
            else {
                objectNameText.text = "";
            }
        }
    }

    /// <summary>
    /// オブジェクト作成
    /// </summary>
    public void CreateObj() {
        Instantiate(obj);
    }

    /// <summary>
    /// 所有権破棄
    /// </summary>
    public void OwnershipAbandonment() {
        if (!targetObj) {
            return;
        }
        targetObj.OwnershipAbandonment();
        Debug.Log($"所有権破棄");
    }

    /// <summary>
    /// 所有権取得
    /// </summary>
    public async void GetOwnership() {
        if (!targetObj) {
            return;
        }
        bool result = await targetObj.GetOwnership();
        Debug.Log($"所有権取得結果：{result}");
    }

    /// <summary>
    /// 強制所有権取得
    /// </summary>
    public async void ForciblyGetOwnership() {
        if (!targetObj) {
            return;
        }
        bool result = await targetObj.GetOwnership(true);
        Debug.Log($"強制所有権取得結果：{result}");
    }
}
