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

    private float elapsedTime;
    private Interactable bottle;    // 掴むオブジェクト
    private bool isClear;

    public float ShakePower { get; private set; }

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction += StartCountdown;
    }

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction -= StartCountdown;
    }

    void Start()
    {
        AudioManager.StopBgm();
        elapsedTime = 0f;
        ShakePower = 0f;
        var myId = NetworkManager.I.myConnectionId;
        int index = InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;
        var player = InRoomPlayerData.I.PlayerList[myId].playerObj.transform;

        bottle = Instantiate(bottlePrefab, bottlePos[index]).GetComponent<Interactable>();
        player.position = playerPos[index].position;
        player.rotation = playerPos[index].rotation;
    }

    void FixedUpdate()
    {
        //if (!controller.isGameStarted) return;

        if (bottle.attachedToHand != null)
        {
            Hand hand = bottle.attachedToHand;

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

            AudioManager.PlaySE(AudioManager.SE.Bank_Open);
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
            AudioManager.ChangeBGM(AudioManager.BGM.Bank);
        }
    }
}