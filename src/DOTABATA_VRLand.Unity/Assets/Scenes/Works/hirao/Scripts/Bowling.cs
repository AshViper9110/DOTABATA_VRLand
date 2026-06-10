using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Text defeatedPinText;
    [SerializeField] private Text defeatedPinTextLog;
    private int defeatedPinCount;

    private List<PinStatus> pinList = new List<PinStatus>();
    private GameObject currentBall;

    private void Start()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
        }
        currentBall = Instantiate(ball, spawnPosBall);
        SetPin();
        defeatedPinCount = 0;
    }

    private void FixedUpdate()
    {
        if (pinList.Count > 0)
        {
            foreach (PinStatus pinStatus in pinList)
            {
                if (!pinStatus.isDefeated && Vector3.Angle(pinStatus.gameObject.transform.up, Vector3.up) > 30)
                {
                    defeatedPinCount++;
                    pinStatus.isDefeated = true;
                }
            }
            defeatedPinText.text = $"{defeatedPinCount}本";
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
        SetPin();
        defeatedPinCount = 0;
    }

    public void SetPin()
    {
        float rowSpacing = 0.2f; // 前後間隔
        float colSpacing = 0.2f;   // 左右間隔

        int rows = 14; // 1 + 2 + 3 + 4 = 10本

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
}
