using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Kinko : MonoBehaviour
{
    [Serializable]
    class DialData
    {
        public GameObject GameObject;
        public GameObject chackMark;
        public float rot = 0;
        public bool isOpen = false;

        [NonSerialized]
        public Interactable interactable;
    }

    [SerializeField] private List<DialData> dialList = new();

    [SerializeField] private float openLockTime;

    [SerializeField] private MinigameFlowController controller;
    [SerializeField] private List<Transform> playerPos = new List<Transform>();
    [SerializeField] private Transform UIPanel;
    [SerializeField] private Transform kinkoPos;

    public SteamVR_Input_Sources handType;
    public float power;
    private bool isClear;
    private float currentLockTime;

    public SteamVR_Action_Vibration hapticAction =
        SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Hapic");

    [SerializeField] float maxTimer;
    [SerializeField] TextMeshProUGUI timerText;
    float timer;

    void Start()
    {
        AudioManager.StopBgm(); ;
        timer = maxTimer;
        timerText.text = "";
        //ダイアルの初期設定
        foreach (var dial in dialList)
        {
            dial.rot = UnityEngine.Random.Range(-180, 180);
            dial.isOpen = false;
            dial.interactable = dial.GameObject.GetComponent<Interactable>();
            dial.chackMark.gameObject.SetActive(false);
        }

        //シーン移行後の位置配置
        var myId = NetworkManager.I.myConnectionId;
        int index = InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;
        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.position = playerPos[index].position;
        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.rotation = playerPos[index].rotation;
        UIPanel.eulerAngles = new Vector3(0, index * -90, 0);
        kinkoPos.eulerAngles = new Vector3(0, index * -90, 0);
    }

    private void OnEnable()
    {
        if (RoomModel.I == null) return;

        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
    }

    // =====================================================
    // Destroy
    // =====================================================

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;

        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
    }

        // Update is called once per frame
    void FixedUpdate()
    {
        if (controller.isGameStarted)
        {

            foreach (var dial in dialList)
            {
                if (dial.isOpen) continue;
                if (dial.interactable.hoveringHand == null) continue;
                //TODO 鍵開けの処理の実装
                float currentRot = dial.GameObject.transform.localEulerAngles.y;
                if (Mathf.Abs(Mathf.DeltaAngle(currentRot, dial.rot)) <= 3f)
                {
                    hapticAction.Execute(0, Time.deltaTime, 100, power, handType);
                    currentLockTime += 0.1f;
                    if (currentLockTime >= openLockTime)
                    {
                        dial.isOpen = true;
                        dial.chackMark.gameObject.SetActive(true);
                        AudioManager.PlaySE(AudioManager.SE.Dial_Open);
                    }
                }
                else
                {
                    if (currentLockTime > 0) currentLockTime = 0;
                }

                timer -= Time.deltaTime;
                timerText.text = ((int)timer).ToString();

                if (timer < 0)
                {
                    controller.isGameStarted = false;
                    Debug.Log("GameEnd");
                    timerText.text = "Finish";
                    isClear = true;
                    controller.OnSendScore((int)timer);
                    return;
                }
            }
            //ゲームクリア判定
            if (dialList.Count > 0 && dialList.All(x => x.isOpen) && !isClear)
            {
                controller.isGameStarted = false;
                Debug.Log("GameClear");
                timerText.text = "Open!!";
                isClear = true;
                AudioManager.PlaySE(AudioManager.SE.Bank_Open);
                controller.OnSendScore((int)timer);
                return;
            }
        }
    }

    public void StartCountdown(int remain)
    {
        if (remain <= 0)
        {
            AudioManager.ChangeBGM(AudioManager.BGM.Bank);
        }
    }

    void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        if (timerText == null) return;
        timerText.gameObject.SetActive(false);
    }
}
