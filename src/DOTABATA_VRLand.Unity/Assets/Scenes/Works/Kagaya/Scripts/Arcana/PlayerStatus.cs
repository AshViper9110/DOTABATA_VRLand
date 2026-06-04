using UnityEngine;

public class PlayerStatus : MonoBehaviour {
    private ArcanaGameManager arcanaGameManager;

    [SerializeField] private float hp = 100;

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
        hp -= damage;
        if (hp < 0) {
            arcanaGameManager.DeathAsync();
        }
    }
}
