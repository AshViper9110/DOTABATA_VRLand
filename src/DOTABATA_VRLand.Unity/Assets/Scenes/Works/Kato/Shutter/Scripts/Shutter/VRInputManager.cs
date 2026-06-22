using UnityEngine;
using Valve.VR;

public class VRInputManager : MonoBehaviour
{
    // Aボタンに割り当てるSteamVRアクション
    public SteamVR_Action_Boolean grabAction;

    // 使用するコントローラー（右手・左手）
    public SteamVR_Input_Sources handType;

    // 操作するコントローラー
    public Transform controller;

    // Aボタンを押した位置
    Vector3 startPos;

    // ドラッグ中かどうか
    bool isDragging = false;

    // ゲーム全体の管理
    public ShutterGameManager shuttergameManager;

    void Update()
    {
        // 入力できない状態なら処理しない
        if (!shuttergameManager.canInput)
            return;

        // ============================
        // Aボタンを押した瞬間
        // ============================
        if (grabAction.GetStateDown(handType))
        {
            Debug.Log("A押した！");

            // コントローラーの開始位置を保存
            startPos = controller.position;
            isDragging = true;
        }

        // ============================
        // Aボタンを離した瞬間
        // ============================
        if (grabAction.GetStateUp(handType) && isDragging)
        {
            // 現在位置を取得
            Vector3 endPos = controller.position;

            // 移動方向を正規化して取得
            Vector3 dir = (endPos - startPos).normalized;

            // ベクトルから上下左右を判定
            Shutter.Direction inputDir = GetDirection(dir);

            // 現在操作対象になっているシャッターを取得
            Shutter current = shuttergameManager.GetCurrentShutter();

            // シャッターが存在するなら入力判定
            if (current != null)
            {
                current.TryOpen(inputDir);
            }

            // ドラッグ終了
            isDragging = false;
        }
    }

    /// <summary>
    /// コントローラーの移動方向を
    /// シャッターの方向（上下左右）に変換する
    /// </summary>
    Shutter.Direction GetDirection(Vector3 dir)
    {
        // 上方向
        if (Vector3.Dot(dir, Vector3.up) > 0.7f)
            return Shutter.Direction.Up;

        // 下方向
        if (Vector3.Dot(dir, Vector3.down) > 0.7f)
            return Shutter.Direction.Down;

        // 右方向
        if (Vector3.Dot(dir, Vector3.right) > 0.7f)
            return Shutter.Direction.Right;

        // それ以外は左方向
        return Shutter.Direction.Left;
    }
}