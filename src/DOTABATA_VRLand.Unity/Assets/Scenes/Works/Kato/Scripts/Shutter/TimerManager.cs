using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public float timeLimit = 30f;

    public TextMeshProUGUI timerText;

    bool isFinished = false;

    void Update()
    {
        if (isFinished)
            return;

        timeLimit -= Time.deltaTime;

        if (timeLimit <= 0)
        {
            timeLimit = 0;
            isFinished = true;

            Debug.Log("TIME UP!");
        }

        timerText.text = Mathf.Ceil(timeLimit).ToString();
    }
}