using DG.Tweening.Core.Easing;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public float timeLimit = 30f;

    public TextMeshProUGUI timerText;

    bool isFinished = false;

    public ShutterGameManager shuttergameManager;

    public GameObject gameOverText;

    void Update()
    {
        if (isFinished)
            return;

        if (shuttergameManager.isGameOver)
            return;

        timeLimit -= Time.deltaTime;

        if (timeLimit <= 0)
        {
            timeLimit = 0;
            isFinished = true;

            shuttergameManager.isGameOver = true;

            gameOverText.SetActive(true);

            Debug.Log("TIME UP!");
        }

        timerText.text = Mathf.Ceil(timeLimit).ToString();
    }
}