using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PanicSoda : MonoBehaviour
{
    [SerializeField] private MinigameFlowController controller;
    [SerializeField] private List<Transform> playerPos = new();
    [SerializeField] private List<Transform> bottlePos;
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private float maxTime = 30f;
    [SerializeField] private float targetShake = 10f;
    [SerializeField] private GameObject panel;

    private float elapsedTime;
    private GameObject bottle;    // 掴むオブジェクト
    private bool isClear;

    public float ShakePower { get; private set; }

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
    }

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnRegisterScoreAction -= OnReceiveRanking;
    }

    void Start()
    {
        AudioManager.StopBgm();
        elapsedTime = 0f;
        ShakePower = 0f;
        var myId = NetworkManager.I.myConnectionId;
        int index = InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;
        var player = InRoomPlayerData.I.PlayerList[myId].playerObj.transform;

        //bottle = Instantiate(bottlePrefab, bottlePos[index]);
        player.position = playerPos[index].position;
        player.rotation = playerPos[index].rotation;
        panel.transform.rotation = playerPos[index].rotation;
    }

    void FixedUpdate()
    {
        if (!controller.isGameStarted) return;

        if (bottle.GetComponent<Interactable>().attachedToHand != null)
        {
            Hand hand = bottle.GetComponent<Interactable>().attachedToHand;

            Vector3 velocity = hand.GetTrackedObjectVelocity();

            float speedY = Mathf.Abs(velocity.y);

            ShakePower += speedY * Time.fixedDeltaTime;

            Debug.Log($"ShakePower : {ShakePower:F2}");
        }

        // クリア判定
        elapsedTime += Time.fixedDeltaTime;

        if (ShakePower >= targetShake && !isClear)
        {
            isClear = true;
            controller.isGameStarted = false;

            // 残り時間をスコアにする

            SendGameClear(-(int)(elapsedTime * 1000));

            Debug.Log($"Clear Time : {elapsedTime:F2}s");

            AudioManager.PlaySE(
                   AudioManager.SE.PanicOpen
               );


            if (bottle.GetComponent<Interactable>().attachedToHand != null)
            {
                bottle.GetComponent<Interactable>().attachedToHand.DetachObject(bottle);
            }
        }
    }

    void SendGameClear(int score)
    {
        controller.OnSendScore(score);
    }

    public void StartCountdown(int remain)
    {
        if (remain <= 0)
        {
            AudioManager.ChangeBGM(AudioManager.BGM.Panic);
            var myId = NetworkManager.I.myConnectionId;
            int index = InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;

            bottle = Instantiate(bottlePrefab, bottlePos[index]);
        }
    }

    void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        Destroy(bottle);
    }
}