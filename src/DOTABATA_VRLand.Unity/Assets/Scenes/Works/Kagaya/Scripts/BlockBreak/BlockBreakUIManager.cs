using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlockBreakUIManager : MonoBehaviour {
    // スコアボード
    [SerializeField] private List<TextMeshProUGUI> playerNameText;
    [SerializeField] private List<TextMeshProUGUI> playerScoreText;

    // ラウンド背景
    [SerializeField] private List<Image> roundBackImageList;

    // 汎用テキスト
    [SerializeField] private TextMeshProUGUI generalPurposeText;

    private bool isCountTime = false;
    private float timer = 0;

    /// <summary>
    /// モニターにプレイヤー名を設定
    /// </summary>
    public void SetPlayerNameText(string[] names) {
        for (int i =0; i < names.Length; i++) {
            playerNameText[i].text = names[i];
            playerScoreText[i].text = "0";
        }
    }

    /// <summary>
    /// モニターにスコアを設定
    /// </summary>
    public void SetPlayerScoreText(int joinOrder, int score) {
        playerScoreText[joinOrder - 1].text = score.ToString();
    }

    /// <summary>
    /// 現在のランドを表示するテキストを更新
    /// </summary>
    public void UpdateRoundText(int round) {
        for (int i = 0; i < roundBackImageList.Count; i++) {
            if (i == round - 1) {
                ColorUtility.TryParseHtmlString("#FFB200", out Color color);
                roundBackImageList[i].color = color; ;
            }
            else {
                ColorUtility.TryParseHtmlString("#808080", out Color color);
                roundBackImageList[i].color = color; ;
            }
        }
    }

    /// <summary>
    /// ターゲットプレイヤーの名前を設定
    /// </summary>
    public void SetCurrentTurnPlayerName(string name) {
        generalPurposeText.text = $"Turn : {name}";
    }

    /// <summary>
    /// ゲームセットテキストを表示
    /// </summary>
    public void GameSetText() {
        generalPurposeText.text = "GameSet";
    }

    /// <summary>
    /// タイマースタート
    /// </summary>
    public void StartTimer(float time) {
        timer = time;
        isCountTime = true;
        generalPurposeText.text = $"Timer : {timer.ToString("F1")}";
    }

    /// <summary>
    /// タイマーストップ
    /// </summary>
    public void StopTimer() {
        isCountTime = false;

        generalPurposeText.text = "";
    }

    private void Update() {
        if (isCountTime) {
            timer -= Time.deltaTime;
            generalPurposeText.text = $"Timer : {timer.ToString("F1")}";
        }
    }
}
