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
    // シールド状況スライダー
    private Slider shieldInfoSlider;
    // シールドスライダーのFill
    private Image shieldSliderFill;

    [SerializeField] private int maxHp = 3;
    [SerializeField] private int hp;

    // 最大シールド量
    [SerializeField] private float maxShieldAmount = 100;
    // シールド量
    [SerializeField] private float shieldAmount = 100;
    // 使用シールド量
    [SerializeField] private float useShieldAmount = 30;
    // シールド回復量
    [SerializeField] private float healShieldAmount = 6;

    // シールド復活時間
    [SerializeField] private float shieldResurrection = 30f;

    // シールド中か
    [SerializeField] private bool isShield = false;

    // シールドを使い果たしたか
    private bool isUseShieldAllUp = false;

    // シールドの使用フレーム数
    [SerializeField] private int useShieldFlameTimer = 0;
    // ジャストシールド受付フレーム
    [SerializeField] private int justShieldFlame = 18;
    // フレーム待機中か
    private bool isDisenableShieldWaitFlame = false;
    

    private void Awake() {
        RoomModel.I.OnSyncdPlayerStatus += OnSyncdPlayerStatus;
        RoomModel.I.OnShieldActivedState += OnShieldActivedState;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnSyncdPlayerStatus -= OnSyncdPlayerStatus;
            RoomModel.I.OnShieldActivedState -= OnShieldActivedState;
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

    private void Update() {
        if (!isShield) {
            useShieldFlameTimer = 0;
            shieldAmount += Time.deltaTime * healShieldAmount;
            if (shieldAmount > maxShieldAmount) {
                shieldAmount = maxShieldAmount;
            }
            else if (isUseShieldAllUp && shieldAmount >= shieldResurrection) {
                isUseShieldAllUp = false;
            }
            UpdateShieldInfoSlider(shieldAmount, maxShieldAmount);
        }
        else {
            useShieldFlameTimer++;
        }

        if (isUseShieldAllUp) {
            SetShieldSliderColor(Color.red);
        }
        else {
            SetShieldSliderColor(Color.cyan);
        }
    }

    /// <summary>
    /// フィールド設定
    /// </summary>
    public void SetField(ArcanaGameManager gameManager, GameObject shieldEffect, Slider sieldInfoSlider) {
        this.arcanaGameManager = gameManager;
        this.shieldEffect = shieldEffect;

        this.shieldInfoSlider = sieldInfoSlider;
        this.shieldSliderFill = this.shieldInfoSlider.GetComponentsInChildren<Image>().First(_ => _.gameObject.name == "Fill");
        this.shieldSliderFill.color = Color.cyan;
    }

    /// <summary>
    /// ダメージ受ける処理
    /// </summary>
    public async UniTask<bool> OnDamage(GameObject magicBall, GameObject hit, GameObject shieldHit, GameObject justHit) {
        if (!syncPlayer.IsOwner()) return true;

        // シールド中かつ 5flame以下
        // ジャストシールド
        if (isShield && useShieldFlameTimer >= justShieldFlame) {
            Instantiate(justHit, magicBall.transform.position, Quaternion.identity);

            bool result = await magicBall.GetComponent<SyncObject>().GetOwnership(true);
            if (result) {
                MagicController mc = magicBall.GetComponent<MagicController>();
                mc.JustShield(syncPlayer.ConnectionId);
            }

            return true;
        }
        // シールド中だったら
        else if (isShield) {
            shieldAmount -= useShieldAmount;
            if (shieldAmount < 0) {
                shieldAmount = 0;
            }
            Instantiate(shieldHit, magicBall.transform.position, Quaternion.identity);
            return false;
        }

        hp--;
        UpdateHpSlider();

        Instantiate(hit, magicBall.transform.position, Quaternion.identity);

        RoomModel.I.SyncPlayerStatusAsync(hp).Forget();

        if (hp < 0) {
            this.gameObject.SetActive(false);
            arcanaGameManager.DeathAsync();
        }

        return false;
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
    public void EnableShield() {
        if (isDisenableShieldWaitFlame) return;

        if (!isShield && isUseShieldAllUp) {
            return;
        }
        else if (isShield && shieldAmount <= 0) {
            isUseShieldAllUp = true;
            DisenableShield();
            return;
        }

        shieldAmount -= Time.deltaTime * useShieldAmount;
        UpdateShieldInfoSlider(shieldAmount, maxShieldAmount);

        shieldEffect.SetActive(true);

        if (!isShield) {
            RoomModel.I.ShieldActiveStateAsync(true).Forget();
            Debug.Log("Active");
        }
        isShield = true;
    }

    /// <summary>
    /// シールド無効化
    /// </summary>
    public async void DisenableShield() {
        if (!isShield ||
            isDisenableShieldWaitFlame) return;

        isDisenableShieldWaitFlame = true;

        await UniTask.WaitUntil(()=> useShieldFlameTimer >= 5);

        isDisenableShieldWaitFlame = false;

        RoomModel.I.ShieldActiveStateAsync(false).Forget();
        Debug.Log("NotActive");
        shieldEffect.SetActive(false);
        isShield = false;
    }

    /// <summary>
    /// シールド情報スライダーを更新
    /// </summary>
    private void UpdateShieldInfoSlider(float timer, float time) {
        if (!shieldInfoSlider) return;

        shieldInfoSlider.value = timer / time;
    }

    /// <summary>
    /// シールドスライダーカラー変更
    /// </summary>
    private void SetShieldSliderColor(Color color) {
        if (!shieldSliderFill) return;
        shieldSliderFill.color = color;
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

    /// <summary>
    /// [サーバー通知]
    /// シールドのアクティブ状態通知
    /// </summary>
    public void OnShieldActivedState(Guid playerConId, bool activeState) {
        if (syncPlayer.ConnectionId != playerConId) return;

        shieldEffect.SetActive(activeState);
    }
}
