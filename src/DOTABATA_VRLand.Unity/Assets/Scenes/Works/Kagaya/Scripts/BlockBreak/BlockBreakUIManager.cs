using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlockBreakUIManager : MonoBehaviour {
    // モニター
    [SerializeField] private List<TextMeshProUGUI> playerNameText;
    [SerializeField] private List<TextMeshProUGUI> playerScoreText;

    /// <summary>
    /// モニターにプレイヤー名を設定
    /// </summary>
    public void SetPlayerNameToMonitor(string[] names) {
        for (int i =0; i < playerNameText.Count; i++) {
            playerNameText[i].text = names[i];
        }
    }

    /// <summary>
    /// モニターにスコアを設定
    /// </summary>
    public void SetPlayerScoreToMoniter(int joinOrder, int score) {
        playerScoreText[joinOrder - 1].text = score.ToString();
    }
}
