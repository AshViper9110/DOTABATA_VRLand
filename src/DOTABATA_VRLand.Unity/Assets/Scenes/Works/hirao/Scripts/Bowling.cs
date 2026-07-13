using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Bowling : MonoBehaviour
{
    [Serializable]
    class PinStatus
    {
        public int id;
        public GameObject gameObject;
        public bool isDefeated;
    }

    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject pin;
    [SerializeField] private Transform spawnPosBall;
    [SerializeField] private Transform spawnPosPin;
    [SerializeField] private TextMeshProUGUI defeatedPinText;
    [SerializeField] private TextMeshProUGUI defeatedPinTextLog;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pinDown;
    [SerializeField] private GameObject panel;
    private int defeatedPinCount = 0;
    private float panelOffset = 5;

    private int currentNextGameTime = -1;

    private List<PinStatus> pinList = new List<PinStatus>();
    private GameObject currentBall;
    [SerializeField] private List<Transform> playerPos = new List<Transform>();

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnBallingNexted += OnBallingNexted;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnBallingNexted -= OnBallingNexted;
    }

    private void OnBallingNexted(int order, JoinedUser joinedUser, int pinCount)
    {
        UpdatePlayerPosition(order);
        defeatedPinTextLog.text += $"{joinedUser.Name} : {pinCount}本";
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
            SpawnBall();
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
        Vector3 panelPos = new Vector3(playerPos[index].position.x, playerPos[index].position.y, playerPos[index].position.z + panelOffset);
        panel.transform.position = panelPos;
    }

    private async void FixedUpdate()
    {
        if (pinList.Count > 0)
        {
            foreach (PinStatus pinStatus in pinList)
            {
                if (!pinStatus.isDefeated && Vector3.Angle(pinStatus.gameObject.transform.up, Vector3.up) > 30)
                {
                    defeatedPinCount++;
                    audioSource.PlayOneShot(pinDown);
                    pinStatus.isDefeated = true;
                    currentNextGameTime = 120;
                }
            }
            defeatedPinText.text = $"{InRoomPlayerData.I.PlayerList[RoomModel.I.ConnectionId].joinedUser.Name} : {defeatedPinCount}本";
        }

        if (currentNextGameTime == 0)
        {
            currentNextGameTime = -1;
            DeletePins();
            var myId = NetworkManager.I.myConnectionId;
            if (!InRoomPlayerData.I.PlayerList.TryGetValue(myId, out var playerData)) return;
            JoinedUser joinedUser = playerData.joinedUser;
            RoomModel.I.SendScore(defeatedPinCount);
            await RoomModel.I.BallingNext(defeatedPinCount, joinedUser);
        }
        else if (currentNextGameTime > 0)
        {
            currentNextGameTime -= 1;
        }
    }

    private void DeletePins()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        if (pinList.Count > 0)
        {
            foreach (PinStatus pin in pinList)
            {
                Destroy(pin.gameObject);
            }
            pinList.Clear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("projectile") && currentBall != null)
        {
            currentNextGameTime = 100;
        }
    }

    public void SpawnBall()
    {
        defeatedPinTextLog.text = $"{defeatedPinCount}本\n" + defeatedPinTextLog.text;
        if (currentBall != null)
        {
            Destroy(currentBall);
        }
        currentBall = Instantiate(ball, spawnPosBall);
        currentBall.GetComponent<Rigidbody>().useGravity = true;
        SetPin();
        defeatedPinCount = 0;
    }

    public void SetPin()
    {
        float rowSpacing = 0.2f; // 前後間隔
        float colSpacing = 0.2f;   // 左右間隔

        int rows = 14;

        int pinCount = 0;

        if (pinList.Count > 0)
        {
            foreach (PinStatus pin in pinList)
            {
                Destroy(pin.gameObject);
            }
            pinList.Clear();
        }

        for (int row = 0; row < rows; row++)
        {
            int pinsInRow = row + 1;

            for (int col = 0; col < pinsInRow; col++)
            {
                float x = (col - (pinsInRow - 1) * 0.5f) * colSpacing;
                float z = row * rowSpacing;

                Vector3 pos = spawnPosPin.position + new Vector3(x, 0, z);

                GameObject pinObject = Instantiate(pin, pos, spawnPosPin.rotation);
                pinObject.GetComponent<Rigidbody>().useGravity = true;

                PinStatus pinStatus = new PinStatus()
                {
                    id = pinCount,
                    gameObject = pinObject,
                    isDefeated = false,
                };
                pinList.Add(pinStatus);
                pinCount++;
            }
        }
    }

    public void StartCountdown(int remain)
    {
        if (remain <= 0)
        {
            AudioManager.ChangeBGM(AudioManager.BGM.Bowling);
        }
    }
}
