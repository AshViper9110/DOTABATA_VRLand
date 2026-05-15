using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Models.Entities;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoCanvas : MonoBehaviour {
    /*
     * 作成用
     */

    private string playerName;
    private int gameModeId = 0;

    // ルーム名
    [SerializeField] private TextMeshProUGUI myRoomNameText;
    // パスワードを使うか
    [SerializeField] private Toggle usePasswordToggle;
    // パスワード入力欄
    [SerializeField] private TMP_InputField passwordInputField;
    // ゲームモード
    [SerializeField] private Toggle freePlayModeToggle;
    [SerializeField] private Toggle tournamentModeToggle;

    /*
     * 参加用
     */

    // ルームリストに使う要素
    [SerializeField] private GameObject roomInfoElement;
    // RoomInfoを生成する親オブジェクト
    [SerializeField] private Transform roomInfoParent;

    private void Start() {
        if (SteamManager.Initialized) {
            playerName = SteamFriends.GetPersonaName();
            Debug.Log(playerName);
        }
        else {
            playerName = "Guest";
            Debug.LogError("Steam is not initialized.");
        }

        SetMyRoomName(playerName);
        usePasswordToggle.onValueChanged.AddListener(UsePasswordToggleOnValueChanged);
        freePlayModeToggle.onValueChanged.AddListener(FreePlayModeToggleOnValueChanged);
        tournamentModeToggle.onValueChanged.AddListener(TournamentModeToggleOnValueChanged);
        RefreshRoomInfoList();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            RefreshRoomInfoList();
        }
    }

    /// <summary>
    /// ルーム名を設定
    /// </summary>
    public void SetMyRoomName(string name) {
        myRoomNameText.text = $"{name}のRoom";
    }

    /// <summary>
    /// パスワードを使うかどうかのトグル
    /// </summary>
    public void UsePasswordToggleOnValueChanged(bool callBack) {
        if (callBack) {
            passwordInputField.GetComponent<Image>().color = Color.white;
            passwordInputField.readOnly = false;
        }
        else {
            passwordInputField.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
            passwordInputField.text = string.Empty;
            passwordInputField.readOnly = true;
        }
    }
    /// <summary>
    /// フリープレイモードにするトグル
    /// </summary>
    public void FreePlayModeToggleOnValueChanged(bool callBack) {
        if (callBack) {
            tournamentModeToggle.isOn = false;
            gameModeId = 0;
        }
        else {
            tournamentModeToggle.isOn = true;
            gameModeId = 1;
        }
    }

    /// <summary>
    /// 大会モードにするトグル
    /// </summary>
    public void TournamentModeToggleOnValueChanged(bool callBack) {
        if (callBack) {
            freePlayModeToggle.isOn = false;
            gameModeId = 1;
        }
        else {
            freePlayModeToggle.isOn = true;
            gameModeId = 0;
        }
    }

    /// <summary>
    /// ルームを作成して参加
    /// </summary>
    public async void CreateAndJoinRoom() {
        string passwordString = "";
        if (usePasswordToggle.isOn) {
            passwordString = passwordInputField.text;
        }

        RoomConfig roomConfig = new RoomConfig() {
            Name = myRoomNameText.text,
            Password = passwordString,
            GameModeId = gameModeId,
        };

        await RoomModel.I.JoinRoomAsync(playerName, roomConfig);
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
        List<RoomInfo> roomInfoList = await RoomModel.I.GetAllRoomAsync();
        foreach (RoomInfo roomInfo in roomInfoList) {
            GameObject createdUI = Instantiate(roomInfoElement, parent: roomInfoParent);
            TextMeshProUGUI[] roomInfoTexts = createdUI.GetComponentsInChildren<TextMeshProUGUI>();
            string roomNameString = roomInfo.Name;
            if (roomInfo.UsePassword) {
                roomNameString += " 🔒";
            }
            roomInfoTexts.First(_ => _.gameObject.name == "RoomNameText").text = roomNameString;
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
