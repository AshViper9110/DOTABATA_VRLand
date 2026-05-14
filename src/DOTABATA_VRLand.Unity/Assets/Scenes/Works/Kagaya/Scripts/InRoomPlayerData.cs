using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InRoomPlayerData : Singleton<InRoomPlayerData> {
    // 自分
    private JoinedUser mySelf;
    public JoinedUser MySelf { get { return mySelf; } }
    // プレイヤーリスト
    private Dictionary<Guid, PlayerData> playerList;
    public Dictionary<Guid, PlayerData> PlayerList { get { return playerList; } }

    protected override void Awake() {
        base.Awake();
        playerList = new Dictionary<Guid, PlayerData>();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init() {
        mySelf = null;
        playerList.Clear();
    }

    /// <summary>
    /// 自分の情報を追加
    /// </summary>
    public void SetMySelf(JoinedUser joinedUser) {
        mySelf = joinedUser;
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
