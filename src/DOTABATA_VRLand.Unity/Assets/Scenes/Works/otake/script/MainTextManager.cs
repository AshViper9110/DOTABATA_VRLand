using UnityEngine;
using UnityEngine.UI;

public class MainTextManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Text text;
    string tempText;
    void Start()
    {
        text = GetComponent<Text>();
        tempText = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (tempText != text.text)
        {
            AudioManager.PlaySE(AudioManager.SE.Main_text_voice);
            tempText = text.text;
        }
    }
}
