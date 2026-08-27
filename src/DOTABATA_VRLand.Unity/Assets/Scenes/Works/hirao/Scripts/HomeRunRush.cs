using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class HomeRunRush : MonoBehaviour
{
    [SerializeField] private MinigameFlowController controller;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private Transform batPos;
    [SerializeField] private float shotPower = 10f;
    [SerializeField] private float shotHight = 5f;

    private float shotInterval = 2f;
    private float nextShotTime = 0f;

    [SerializeField] private List<Transform> playerPos = new List<Transform>();
    [SerializeField] private GameObject panel;
    private float panelOffset = 2;

    private GameObject bat;

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnBallingNexted += OnBallingNexted;
        RoomModel.I.OnBallingPinAsynced += OnBallingPinAsync;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnBallingNexted -= OnBallingNexted;
        RoomModel.I.OnBallingPinAsynced -= OnBallingPinAsync;
    }

    private void OnBallingNexted(int order, JoinedUser joinedUser, int pinCount)
    {
        UpdatePlayerPosition(order);
    }

    private void OnBallingPinAsync(int count, JoinedUser joineduser)
    {
        //
    }

    private void Start()
    {
        AudioManager.StopBgm();
        UpdatePlayerPosition(1);
    }

    private void UpdatePlayerPosition(int currentOrder)
    {
        var myId = NetworkManager.I.myConnectionId;
        if (!InRoomPlayerData.I.PlayerList.TryGetValue(myId, out var playerData))
            return;

        int myOrder = playerData.joinedUser.JoinOrder;

        int index;

        if (myOrder == currentOrder)
        {
            index = 0;
            bat = Instantiate(batPrefab, batPos);
        }
        else if (myOrder < currentOrder)
        {
            index = myOrder;
        }
        else
        {
            index = myOrder - 1;
        }

        if (index < 0 || index >= playerPos.Count)
            return;

        playerData.playerObj.transform.position = playerPos[index].position;
        Vector3 panelPos = new Vector3(playerPos[index].position.x, panel.transform.position.y, playerPos[index].position.z + panelOffset);
        panel.transform.position = panelPos;
    }

    private void Update()
    {
        if (Time.time >= nextShotTime)
        {
            ShotBall();

            nextShotTime = Time.time + shotInterval;
        }
    }

    private void ShotBall()
    {
        GameObject ball = Instantiate(
            ballPrefab,
            transform.position,
            transform.rotation
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity =
            transform.forward * shotPower +
            Vector3.up * shotHight;
    }

    private void OnTriggerExit(Collider other)
    {
        // ボール以外は無視
        if (!other.CompareTag("projectile"))
            return;

        Debug.Log("ホームラン！");

        // ホームラン処理
        HomeRun();
    }

    private void HomeRun()
    {
        // ここにホームラン時の処理を書く
        // controller側に処理を作ったらここから呼び出す
    }

    public void StartCountdown(int remain)
    {
        if (remain <= 0)
        {
            AudioManager.ChangeBGM(AudioManager.BGM.Bowling);
        }
    }
}