using UnityEngine;

/// <summary>
/// サイダーを振った量を計測するクラス
/// 制限時間中の移動量をスコアとして蓄積し、
/// 終了後に開封処理を行う
/// </summary>

public class ShakeController : MonoBehaviour
{
    // 前フレームの位置
    private Vector3 lastPosition;

    // 振った量
    private float pressure;

    // 開封済みか
    private bool opened = false;

    // 開封時に再生する泡パーティクル
    public ParticleSystem foamParticle;

    // ゲームタイマー参照
    public SiderGameTimer sidergameTimer;

    // ドラッグ状態取得用
    private BottleDrag bottleDrag;

    void Start()
    {
        lastPosition = transform.position;

        bottleDrag = GetComponent<BottleDrag>();
    }

    void Update()
    {
        // ゲーム終了後の開封
        if (sidergameTimer.gameEnded &&
            !opened &&
            Input.GetKeyDown(KeyCode.Space))
        {
            OpenBottle();

            opened = true;
        }

        // ゲーム中だけ振動計測
        if (!sidergameTimer.gameEnded)
        {
            DetectShake();
        }
    }

    void DetectShake()
    {
        // 掴んでいない時は計測しない
        if (!bottleDrag.IsDragging)
        {
            lastPosition = transform.position;
            return;
        }

        // 前フレームからの移動量
        Vector3 movement =
            transform.position - lastPosition;

        float speed =
            movement.magnitude / Time.deltaTime;

        // 一定以上の速度だけ加算
        if (speed > 5f)
        {
            pressure += speed * 0.01f;

            Debug.Log("Pressure : " + pressure);
        }

        // 減衰
        pressure -= Time.deltaTime * 5f;

        // マイナス防止
        pressure = Mathf.Max(pressure, 0f);

        // 次回計測用に現在位置保存
        lastPosition = transform.position;
    }

    void OpenBottle()
    {
        foamParticle.Play();

        // 少数を整数に
        int finalScore =
            Mathf.RoundToInt(pressure);

        Debug.Log("最終スコア : " + finalScore);
    }
}