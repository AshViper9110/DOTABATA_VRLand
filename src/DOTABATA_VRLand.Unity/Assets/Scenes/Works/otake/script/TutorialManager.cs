using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] List<Sprite> TutorialImages;
    [SerializeField] Image tutorialImage;
    int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tutorialImage.sprite = TutorialImages[0];
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextTutorial()
    {
        // 次に進めるかチェック
        if (index + 1 >= TutorialImages.Count)
        {
            return;
        }

        index++;
        tutorialImage.sprite = (Sprite) TutorialImages[index];

    }

    public void BackTutorial()
    {
        if (index > 0)
        {
            index--;
        }

        tutorialImage.sprite = (Sprite)TutorialImages[index];
    }
}
