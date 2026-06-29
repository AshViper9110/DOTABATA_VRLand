using System;
using UnityEngine;

public class SyncDrawBoad : MonoBehaviour {
    private SyncPlayer syncPlayer;

    // 絵描き板
    private GameObject drawBoadObj;

    private void Start() {
        syncPlayer = GetComponent<SyncPlayer>();
    }

    /// <summary>
    /// フィールド設定
    /// </summary>
    public void SetField(GameObject drawBoad) {
        drawBoadObj = drawBoad;
    }

    private void Awake() {
        RoomModel.I.OnSwitchedDrawBoadActive += OnSwitchDrawBoadActive;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnSwitchedDrawBoadActive -= OnSwitchDrawBoadActive;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    /// <summary>
    /// [サーバー通知]
    /// 絵描き板の表示非表示同期通知
    /// </summary>
    public void OnSwitchDrawBoadActive(Guid playerConId, bool active) {
        if (syncPlayer.ConnectionId != playerConId) return;
        drawBoadObj.SetActive(active);
    }
}
