using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;

public class RoomInfoCanvas : MonoBehaviour {
    /*
     * 作成用
     */

    private string playerName;
    private int gameModeId = 0;
    private ulong steamId;

    [SerializeField] private GameObject standyPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomStartButton;

    // ルーム作成UI
    [SerializeField] private GameObject createRoomUI;

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

    // ルーム参加UI
    [SerializeField] private GameObject joinRoomUI;

    // ルームリストに使う要素
    [SerializeField] private GameObject roomInfoElement;
    // RoomInfoを生成する親オブジェクト
    [SerializeField] private Transform roomInfoParent;

    // ルーム名
    [SerializeField] private TextMeshProUGUI roomNameText;
    // プレイヤー人数
    [SerializeField] private TextMeshProUGUI playerAmountText;
    // パスワード入力欄
    [SerializeField] private TMP_InputField joinPasswordInputField;
    // ルーム参加ボタン
    [SerializeField] private Button joinRoomBtn;

    //SteamVRのボタン
    public SteamVR_Action_Boolean triggerAction;


    /*
     * 共通
     */

    // パスワード入力用キーボードUI
    [SerializeField] private GameObject keyBoardUI;
    // パスワード入力先
    private TMP_InputField targetInputFirld;

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnJoinedUser += OnJoinedUser;
        RoomModel.I.OnLeavedUser += OnLeavedUser;
        RoomModel.I.OnRoomStarted += OnRoomStarted;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnJoinedUser -= OnJoinedUser;
        RoomModel.I.OnLeavedUser -= OnLeavedUser;
        RoomModel.I.OnRoomStarted -= OnRoomStarted;
    }

    private void Start() {
        if (SteamManager.Initialized) {
            playerName = SteamFriends.GetPersonaName();
            steamId = SteamUser.GetSteamID().m_SteamID;//steamIdを取得
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

        ChangeInputFieldFocuse();
    }

    /// <summary>
    /// どのInputFieldを選択しているか
    /// </summary>
    private void ChangeInputFieldFocuse() {
        if (passwordInputField.isFocused) {
            if (!keyBoardUI.activeSelf) {
                keyBoardUI.SetActive(true);
            }
            targetInputFirld = passwordInputField;
        }
        else if (joinPasswordInputField.isFocused) {
            if (!keyBoardUI.activeSelf) {
                keyBoardUI.SetActive(true);
            }
            targetInputFirld = joinPasswordInputField;
        }
    }

    public void CloseKeyBoard()
    {
        keyBoardUI.SetActive(false);
        targetInputFirld = null;
    }

    /// <summary>
    /// ワールド空間にあるテンキーが押されたら
    /// </summary>
    public void InputWorldNumKeyBtn(int num) {
        if (targetInputFirld != null) {
            if (num == -1) {
                if (targetInputFirld.text.Length > 0) {
                    targetInputFirld.text = targetInputFirld.text.Substring(0, targetInputFirld.text.Length - 1);
                }
            }
            else if (num == -2) {
                targetInputFirld.text = "";
            }
            else {
                if (targetInputFirld.text.Length < 4) {
                    targetInputFirld.text += num.ToString();
                }
            }
        }
    }

    /// <summary>
    /// ルーム作成画面にする
    /// </summary>
    public void ChangeCreateRoomUIBtn() {
        createRoomUI.SetActive(true);
        joinRoomUI.SetActive(false);
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

        RoomConfig roomConfig = new RoomConfig()
        {
            Name = myRoomNameText.text,
            Password = passwordString,
            GameModeId = gameModeId,
        };        
        await NetworkManager.I.JointoRoom(steamId, roomConfig);
        standyPanel.SetActive(true);
        lobbyPanel.SetActive(false);
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
                roomNameString += " <sprite name=lock>";
            }
            roomInfoTexts.First(_ => _.gameObject.name == "RoomNameText").text = roomNameString;
            roomInfoTexts.First(_=>_.gameObject.name == "PlayerAmountText").text = roomInfo.PlayerAmount + "/4";

            Button joinBtn = createdUI.GetComponentInChildren<Button>();
            joinBtn.onClick.AddListener(() => {
                ChangeJoinRoomUIBtn(roomInfo);
            });
        }
    }

    /// <summary>
    /// ルーム参加画面にする
    /// </summary>
    private void ChangeJoinRoomUIBtn(RoomInfo roomInfo) {
        createRoomUI.SetActive(false);
        joinRoomUI.SetActive(true);

        string roomNameString = roomInfo.Name;
        if (roomInfo.UsePassword) {
            roomNameString += " <sprite name=lock>";

            joinPasswordInputField.GetComponent<Image>().color = Color.white;
            joinPasswordInputField.readOnly = false;
        }
        else {
            joinPasswordInputField.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
            joinPasswordInputField.text = string.Empty;
            joinPasswordInputField.readOnly = true;
        }
        roomNameText.text = roomNameString;
        playerAmountText.text = roomInfo.PlayerAmount + "/4";

        joinRoomBtn.onClick.AddListener(async () => {
            string passwordString = "";
            if (roomInfo.UsePassword) {
                passwordString = joinPasswordInputField.text;
            }

            RoomConfig roomConfig = new RoomConfig() {
                Name = roomInfo.Name,
                Password = passwordString,
            };
            await NetworkManager.I.JointoRoom(steamId, roomConfig);

            standyPanel.SetActive(true);
            lobbyPanel.SetActive(false);
        });
    }

    public async void LeaveOnRoom()
    {
        NetworkManager.I.isJoin = false;
        await RoomModel.I.LeaveRoomAsync();
        standyPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public async void RoomStart()
    {
        await RoomModel.I.RoomStart();
    }

    //Server通知
    private void OnJoinedUser(JoinedUser joinedUser)
    {
        if(joinedUser.ConnectionId == RoomModel.I.ConnectionId)
        {
            if (joinedUser.JoinOrder == 1) roomStartButton.SetActive(true);
        }
    }

    private void OnLeavedUser(Guid connectionId, int joinOrder)
    {

    }

    private void OnRoomStarted()
    {
        SteamVR_Fade.View(Color.white, 0.5f);
        SceneManager.LoadScene("GameScene");
    }
}
