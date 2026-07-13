using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour {
    private ArcanaGameManager arcanaGameManager;
    private SyncPlayer syncPlayer;

    private Transform hpImages;

    // シールドのエフェクト
    private GameObject shieldEffect;

    [SerializeField] private int maxHp = 3;
    [SerializeField] private int hp;

    // シールド中か
    private bool isShield = false;

    private void Awake() {
        RoomModel.I.OnSyncdPlayerStatus += OnSyncdPlayerStatus;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnSyncdPlayerStatus -= OnSyncdPlayerStatus;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void Start() {
        syncPlayer = this.GetComponent<SyncPlayer>();

        hpImages = this.GetComponentsInChildren<Transform>().First(_=>_.gameObject.name == "Heats");
    
        hp = maxHp;
    }

    /// <summary>
    /// フィールド設定
    /// </summary>
    public void SetField(ArcanaGameManager gameManager, GameObject shieldEffect) {
        this.arcanaGameManager = gameManager;
        this.shieldEffect = shieldEffect;
    }

    /// <summary>
    /// ダメージ受ける処理
    /// </summary>
    public async void OnDamage() {
        if (!syncPlayer.IsOwner()) return;

        // シールド中だったら
        if (isShield) {
            DisenableShield();
            return;
        }

        hp--;
        UpdateHpSlider();

        await RoomModel.I.SyncPlayerStatusAsync(hp);

        if (hp < 0) {
            this.gameObject.SetActive(false);
            arcanaGameManager.DeathAsync();
        }
    }

    /// <summary>
    /// HP画像更新
    /// </summary>
    private void UpdateHpSlider() {
        foreach (Transform child in hpImages) {
            child.Find("Heat").gameObject.SetActive(true);
        }

        if (hp == 2) {
            hpImages.Find("Heat_3/Heat").gameObject.SetActive(false);
        }
        else if (hp == 1) {
            hpImages.Find("Heat_3/Heat").gameObject.SetActive(false);
            hpImages.Find("Heat_2/Heat").gameObject.SetActive(false);

        }
        else if (hp == 0) {
            hpImages.Find("Heat_3/Heat").gameObject.SetActive(false);
            hpImages.Find("Heat_2/Heat").gameObject.SetActive(false);
            hpImages.Find("Heat_1/Heat").gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// シールドを有効か
    /// </summary>
    public async void EnableShield() {
        if (isShield) return;

        isShield = true;
        Instantiate(shieldEffect, this.transform.position, Quaternion.identity, this.transform);

        await UniTask.WaitForSeconds(4);

        DisenableShield();
    }

    /// <summary>
    /// シールド無効化
    /// </summary>
    private void DisenableShield() {
        isShield = false;
    }

    /// <summary>
    /// [サーバー通知]
    /// プレイヤーのステータス同期通知
    /// </summary>
    public void OnSyncdPlayerStatus(Guid playerConId, int hp) {
        if (syncPlayer.ConnectionId != playerConId) return;
        this.hp = hp;

        UpdateHpSlider();
    }
}
