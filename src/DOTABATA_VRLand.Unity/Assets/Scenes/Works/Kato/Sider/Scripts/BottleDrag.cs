using UnityEngine;

/// <summary>
/// ボトルのドラッグ操作を担当するクラス
/// マウスで掴み、移動させる
/// ShakeControllerから現在ドラッグ中か参照される
/// </summary>

public class BottleDrag : MonoBehaviour
{
    // ボトルをドラッグ中か
    private bool dragging;

    // カメラからボトルまでの距離
    private float zDistance;

    public SiderGameTimer siderGameTimer;


    // 現在ドラッグ中か取得
    public bool IsDragging
    {
        get { return dragging; }
    }

    void Update()
    {
        // 左クリックでボトルを掴む
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray =
                Camera.main.ScreenPointToRay(Input.mousePosition);

            // マウスカーソル位置にRayを飛ばし、
            // ボトルがクリックされたか判定
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    dragging = true;

                    // ボトルとカメラの距離を保持
                    // ドラッグ中に奥行きが変わらないようにする
                    zDistance =
                        Camera.main.WorldToScreenPoint(transform.position).z;
                }
            }
        }

        // 左クリックを離したらドラッグ終了
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        // ドラッグ中はマウス位置へ移動
        if (dragging)
        {
            Vector3 mousePos = Input.mousePosition;

            // 保持した奥行きを設定
            mousePos.z = zDistance;

            // 画面座標 → ワールド座標へ変換
            Vector3 worldPos =
                Camera.main.ScreenToWorldPoint(mousePos);

            transform.position = worldPos;
        }


        // 時間切れで操作不能
        if (siderGameTimer.gameEnded)
        {
            dragging = false;
            return;
        }
    }

}