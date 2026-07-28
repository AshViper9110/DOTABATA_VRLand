using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using Steamworks;
using System;
using UnityEngine;

public class ConnectionTest : MonoBehaviour {
    private void Awake() {
        RoomModel.I.OnJoinedUser += OnJoinedUser;
        RoomModel.I.OnLeavedUser += OnLeavedUser;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnJoinedUser -= OnJoinedUser;
            RoomModel.I.OnLeavedUser -= OnLeavedUser;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private async void Start() {
        await UserModel.I.CreateUserModel();
        await RoomModel.I.ConnectAsync();
        ulong steamId = SteamUser.GetSteamID().m_SteamID;//steam��ID��擾
        await RoomModel.I.JoinRoomAsync(steamId, new RoomConfig() { GameModeId = 0, Name = "TestRoom"});
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの入室通知
    /// </summary>
    private void OnJoinedUser(JoinedUser user) {
        Debug.Log($"{user.Name}が入室");
    }

    /// <summary>
    /// [サーバー通知]
    /// ロビーの退室通知
    /// </summary>
    private void OnLeavedUser(Guid connectionId, int joinOrder) {
        Debug.Log($"ConnectionId：{connectionId} が退室");
    }
}
