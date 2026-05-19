using System.Collections.Generic;
using UnityEngine;

public class MinigameNetworkManager : MonoBehaviour
{
    public static MinigameNetworkManager Instance;

    [Header("参照")]
    public MinigameFlowController flowController;

    // =====================================================
    // 初期化
    // =====================================================

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // Ready送信
    // =====================================================

    public async void SendReadyState(bool isReady)
    {
        Debug.Log($"Ready送信 : {isReady}");

        // TODO:
        // await _hubClient.UpdateReadyStateAsync(isReady);
    }

    // =====================================================
    // ゲーム開始送信
    // =====================================================

    public async void GameStart()
    {
        Debug.Log("ゲーム開始送信");

        // TODO:
        // await _hubClient.GameStartAsync();
    }

    // =====================================================
    // カウントダウン開始送信
    // =====================================================

    public async void StartCountdown()
    {
        Debug.Log("カウントダウン開始");

        // TODO:
        // await _hubClient.StartCountdownAsync();
    }

    // =====================================================
    // スコア送信
    // =====================================================

    public async void SendScore(int result)
    {
        Debug.Log($"スコア送信 : {result}");

        // TODO:
        // await _hubClient.RegisterScoreAsync(result);
    }

    // =====================================================
    // Ready状態更新受信
    // =====================================================

    public void OnUpdateReadyState(string playerName, bool isReady)
    {
        Debug.Log($"{playerName} のReady状態 : {isReady}");

        flowController.UpdatePlayerReady(playerName, isReady);
    }

    // =====================================================
    // 全員Ready受信
    // =====================================================

    public void OnUpdateAllReadyState(bool isAllReady)
    {
        if (isAllReady)
        {
            Debug.Log("全員準備完了");

            flowController.AllPlayerReady();
        }
        else
        {
            Debug.Log("準備中のプレイヤーがいます");
        }
    }

    // =====================================================
    // カウントダウン受信
    // =====================================================

    public void OnCountdown(int count)
    {
        Debug.Log($"カウント : {count}");

        flowController.StartCountdown(count);
    }

    // =====================================================
    // ランキング受信
    // =====================================================

    public void OnRegisterScore(List<string> rankOrder)
    {
        Debug.Log("ランキング受信");

        flowController.ShowRanking(rankOrder);
    }

    // =====================================================
    // 最終順位受信
    // =====================================================

    public void OnGetLastMiniGameRanking(int lastRank)
    {
        if (lastRank == -99)
        {
            Debug.Log("対象プレイヤーが存在しません");
            return;
        }

        if (lastRank == -1)
        {
            Debug.Log("ランキングデータが存在しません");
            return;
        }

        Debug.Log($"最終順位 : {lastRank}位");
    }

    // =====================================================
    // 総合順位受信
    // =====================================================

    public void OnGetAllRoundRanking(List<string> ranking)
    {
        Debug.Log("総合順位受信");

        for (int i = 0; i < ranking.Count; i++)
        {
            Debug.Log($"{i + 1}位 : {ranking[i]}");
        }
    }
}