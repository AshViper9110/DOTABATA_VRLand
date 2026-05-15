using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoCanvas : MonoBehaviour {
    // ルームリストに使う要素
    [SerializeField] private GameObject roomInfoElement;
    // RoomInfoを生成する親オブジェクト
    [SerializeField] private Transform roomInfoParent;

    private void Start() {
        RefreshRoomInfoList();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            RefreshRoomInfoList();
        }
    }

    /// <summary>
    /// ルーム一覧更新
    /// </summary>
    public async void RefreshRoomInfoList() {
        await UniTask.WaitUntil(() => RoomModel.I != null && RoomModel.I.IsConnected);

        // 要素を全削除
        foreach (Transform child in roomInfoParent) {
            Destroy(child.gameObject);
        }

        // 要素を再生成
        List<RoomInfo> roomInfoList = await RoomModel.I.GetAllRoomNamesAsync();
        foreach (RoomInfo roomInfo in roomInfoList) {
            GameObject createdUI = Instantiate(roomInfoElement, parent: roomInfoParent);
            TextMeshProUGUI[] roomInfoTexts = createdUI.GetComponentsInChildren<TextMeshProUGUI>();
            roomInfoTexts.First(_=>_.gameObject.name == "RoomNameText").text = roomInfo.Name;
            roomInfoTexts.First(_=>_.gameObject.name == "PlayerAmountText").text = roomInfo.PlayerAmount + "/4";

            Button joinBtn = createdUI.GetComponentInChildren<Button>();
            joinBtn.onClick.AddListener(async () => {
                RoomConfig roomConfig = new RoomConfig() {
                    Name = roomInfo.Name,
                };
                await RoomModel.I.JoinRoomAsync("TestUser", roomConfig);
            });
        }
    }
}
