using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using Valve.VR;
using TMPro;

public class MinigameFlowController : MonoBehaviour
{
    [Header("UI")]
    public GameObject introUI;
    public GameObject gameUI;
    public GameObject resultUI;

    [Header("Intro")]
    public GameObject descriptionPanel;
    public GameObject readyPanel;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI readyText;

    [Header("Ready")]
    public Button readyButton;
    public TextMeshProUGUI waitingText;
    public Transform UserReadyObject;//プレイヤー情報整列用オブジェクト
    public GameObject playerNamePrefab;  //プレイヤーテキストプレハブ
    public List<GameObject> UserReadyText;   //プレイヤー準備情報テキスト 
    public Button StartButton;

    [Header("Countdown")]
    public TextMeshProUGUI countdownText;

    [Header("Data")]
    public MinigameInfo info;

    public bool isGameStarted = false;

    public List<string> names;
    bool willReady = false;

    // =====================================================
    // Start
    // =====================================================

    async void Start()
    {
        SteamVR_Fade.View(new Color(0,0,0,0),1.0f);
        //RoomModelイベント購読
        
        //InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.transform.position = Vector3.zero;//プレイヤー座標初期化(0,0,0)
        //foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values)//他プレイヤー非表示
        //{
        //    if (player.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) continue;
        //    player.playerObj.SetActive(false);
        //}
           
        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        resultUI.SetActive(false);
        gameUI.SetActive(false);
        introUI.SetActive(false);
        StartButton.gameObject.SetActive(false);
        introUI.SetActive(true);

        titleText.text = info.gameName;
        descriptionText.text = info.description;

        readyText.text = "0/"+ InRoomPlayerData.I.PlayerList.Count + "プレイヤー準備完了";
    }

    private void OnEnable()
    {
        if (RoomModel.I == null) return;

        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction += OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction += OnUpdatePlayerReady;
        RoomModel.I.OnGameStartAction += StartGameFlow;
    }

    // =====================================================
    // Destroy
    // =====================================================

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;

        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnRegisterScoreAction -= OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction -= OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction -= OnUpdatePlayerReady;
        RoomModel.I.OnGameStartAction -= StartGameFlow;
    }

    // =====================================================
    // Ready切り替え
    // =====================================================

    public async void OnReadyButton()
    {
         willReady = !willReady;//状態切り替え

        // サーバー送信
        await RoomModel.I.SendReadyState(willReady);

        // UI更新
        if (willReady)
        {
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "取り消し";
            waitingText.gameObject.SetActive(true);
        }
        else
        {
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = "準備OK！";
            waitingText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // プレイヤーReady更新
    // =====================================================

    void OnUpdatePlayerReady(JoinedUser[] users, bool[] isReadyList)
    {
        int readyCount = 0;
        //状態のログ表示
        for (int i = 0; i < users.Length; i++)
        {
            Debug.Log(isReadyList[i] ? $"{users[i].Name} : 準備OK" : $"{users[i].Name} : 待機中");
            if (isReadyList[i]) readyCount++;
        }

        readyText.text = readyCount +"/" + users.Length + "プレイヤー準備完了";
    }

    // =====================================================
    // 全員Ready通知
    // =====================================================

    void OnAllReadyState(bool isAllReady)
    {        
        if(isAllReady)Debug.Log("全員Ready");
        else Debug.Log("誰かの準備ができていません");
        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder != 1) return;
        //各ボタン切り替え
        readyButton.gameObject.SetActive(!isAllReady);
        StartButton.gameObject.SetActive(isAllReady);
    }

    // =====================================================
    // ゲーム開始送信
    // =====================================================

    public void GameStrat()
    {
        RoomModel.I.OnGameStartAsync();
    }

    // =====================================================
    // ゲーム開始取得
    // =====================================================

    void StartGameFlow()
    {
        StartButton.gameObject.SetActive(false);
        descriptionPanel.SetActive(false);
        readyPanel.SetActive(false);
        RoomModel.I.StartCountdown();
    }

    // =====================================================
    // カウントダウン受信
    // =====================================================
    public void StartCountdown(int remain)
    {
        countdownText.gameObject.SetActive(true);
        if (remain > 0)
        {
            countdownText.text = remain.ToString();
            AudioManager.PlaySE(AudioManager.SE.MiniGame_CountDown);
        }
        else
        {
            AudioManager.PlaySE(AudioManager.SE.MiniGame_Start);
            countdownText.text = "START!";
            StartCoroutine(BeginGameAfterStart());
        }
    }

    // =====================================================
    // ミニゲーム開始
    // =====================================================

    IEnumerator BeginGameAfterStart()
    {
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);//カウントダウン削除
        introUI.SetActive(false);
        gameUI.SetActive(true);
        
        foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values)//プレイヤー表示
        {
            if (player.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) continue;
            player.playerObj.SetActive(true);
        }
        
        isGameStarted = true;//ゲーム開始判定

    }

    // =====================================================
    // Score送信
    // =====================================================

    public void OnSendScore(int score)
    {
        // サーバー送信
        RoomModel.I.SendScore(score);
    }

   
    // =====================================================
    // ランキング受信
    // =====================================================

    void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        AudioManager.PlaySE(AudioManager.SE.MiniGame_Finish);
        names.Clear();//前回のユーザー情報クリア
        foreach (var user in rankOrder)names.Add(user.Name);
        Debug.Log("OnReceiveRanking受信");
        foreach (var user in names)Debug.Log($"ランキング受信:{user}");
        isGameStarted = false;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "FINISH!";
        }
        StartCoroutine(EndGame());
    }

    // =====================================================
    // 終了
    // =====================================================

    IEnumerator EndGame()
    {
        yield return new WaitForSecondsRealtime(2f);

        SteamVR_Fade.View(new Color(1, 1, 1, 1), 2);
        SceneManager.LoadScene("GameScene");//シーン移行
    }
}