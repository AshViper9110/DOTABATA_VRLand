using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DummyTextManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TextMeshProUGUI text;
    string tempText;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        tempText = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (tempText != text.text)
        {
            AudioManager.PlaySE(AudioManager.SE.Main_text_voice);
            HostManager.I.MoveBeard();
            tempText = text.text;
        }
    }
}
