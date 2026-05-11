using System;
using System.Collections.Generic;
using UnityEngine;

public class InRoomPlayerData : Singleton<InRoomPlayerData> {
    // プレイヤーリスト
    private Dictionary<Guid, PlayerData> playerList;
    public Dictionary<Guid, PlayerData> PlayerList { get { return playerList; } }

    protected override void Awake() {
        base.Awake();
        playerList = new Dictionary<Guid, PlayerData>();
    }

    /// <summary>
    /// プレイヤーリストの初期化
    /// </summary>
    public void InitPlayerList() {
        playerList.Clear();
    }

    /// <summary>
    /// プレイヤーリストに追加
    /// </summary>
    public void AddPlayer(Guid connectionId,  PlayerData playerData) {
        playerList[connectionId] = playerData;
    }

    /// <summary>
    /// プレイヤーリストから削除
    /// </summary>
    public void RemovePlayer(Guid connectionId) {
        Destroy(playerList[connectionId].playerObj);
        playerList.Remove(connectionId);
    }
}
