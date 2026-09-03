using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InRoomPlayerData : Singleton<InRoomPlayerData> {
    // 自分
    public PlayerData MySelf { get; private set; }
    // プレイヤーリスト
    public Dictionary<Guid, PlayerData> PlayerList { get; private set; } = new Dictionary<Guid, PlayerData>();

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init() {
        MySelf = null;
        PlayerList.Clear();
    }

    /// <summary>
    /// 自分の情報を追加
    /// </summary>
    public void SetMySelf(PlayerData self) {
        MySelf = self;
    }

    /// <summary>
    /// プレイヤーリストに追加
    /// </summary>
    public void AddPlayer(Guid connectionId,  PlayerData playerData) {
        PlayerList[connectionId] = playerData;
    }

    /// <summary>
    /// プレイヤーリストから削除
    /// </summary>
    public void RemovePlayer(Guid connectionId) {
        if (PlayerList[connectionId].joinedUser == null) return;

        int joinOrder = PlayerList[connectionId].joinedUser.JoinOrder;

        Destroy(PlayerList[connectionId].playerObj);
        PlayerList.Remove(connectionId);

        // JoinOeder繰り下げ
        if (MySelf != null &&
            MySelf.joinedUser != null &&
            MySelf.joinedUser.JoinOrder > joinOrder) {
            MySelf.joinedUser.JoinOrder--;
        }

        foreach (PlayerData player in PlayerList.Values) {
            if (player == null || player.joinedUser == null)
                continue;

            if (player.joinedUser.JoinOrder > joinOrder) {
                player.joinedUser.JoinOrder--;
            }
        }
    }

    /// <summary>
    /// デバッグ用
    /// </summary>
    public void ShowPlayerList() {
        string text = "";
        text += $"PlayerCount : {PlayerList.Count}\n\n";
        foreach (var player in PlayerList) {
            text +=
                $"ID：{player.Key}, Name：{player.Value.joinedUser.Name}, JoinOrder：{player.Value.joinedUser.JoinOrder}\n";
        }

        Debug.Log(text);
    }
}
