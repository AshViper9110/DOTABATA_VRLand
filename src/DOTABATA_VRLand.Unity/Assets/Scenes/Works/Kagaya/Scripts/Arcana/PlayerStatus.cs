using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour {
    private ArcanaGameManager arcanaGameManager;
    private SyncPlayer syncPlayer;

    private Slider hpSlider;

    [SerializeField] private float maxHp = 100;
    [SerializeField] private float hp;

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

        hpSlider = this.GetComponentInChildren<Slider>();
    
        hp = maxHp;
    }

    /// <summary>
    /// ゲームマネージャーをセット
    /// </summary>
    public void SetGameManager(ArcanaGameManager gameManager) {
        this.arcanaGameManager = gameManager;
    }

    /// <summary>
    /// ダメージ受ける処理
    /// </summary>
    public async void OnDamage(float damage) {
        if (!syncPlayer.IsOwner()) return;

        hp -= damage;

        UpdateHpSlider();

        await RoomModel.I.SyncPlayerStatusAsync(hp);

        if (hp < 0) {
            this.gameObject.SetActive(false);
            arcanaGameManager.DeathAsync();
        }
    }

    /// <summary>
    /// HPバー更新
    /// </summary>
    private void UpdateHpSlider() {
        hpSlider.value = hp / maxHp;
    }

    /// <summary>
    /// [サーバー通知]
    /// プレイヤーのステータス同期通知
    /// </summary>
    public void OnSyncdPlayerStatus(Guid playerConId, float hp) {
        if (syncPlayer.ConnectionId != playerConId) return;
        this.hp = hp;

        UpdateHpSlider();
    }
}
