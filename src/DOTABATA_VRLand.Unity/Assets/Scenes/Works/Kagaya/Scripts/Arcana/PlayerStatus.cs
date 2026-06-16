using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour {
    private ArcanaGameManager arcanaGameManager;
    private SyncPlayer syncPlayer;

    private Slider hpSlider;

    [SerializeField] private float maxHp = 100;
    [SerializeField] private float hp;

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
    public void OnDamage(float damage) {
        if (!syncPlayer.IsOwner()) return;

        hp -= damage;
        hpSlider.value = hp / maxHp;
        if (hp < 0) {
            this.gameObject.SetActive(false);
            arcanaGameManager.DeathAsync();
        }
    }
}
