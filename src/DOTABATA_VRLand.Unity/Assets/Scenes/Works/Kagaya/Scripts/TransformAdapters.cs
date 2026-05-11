using DG.Tweening;
using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;

public static class TransformAdapters
{
    /// <summary>
    /// Transform -> DTO
    /// </summary>
    public static SimpleTransform ToSimpleTransform(this Transform t) =>
    new SimpleTransform
    {
        localPosition = t.position,    // ✅ world座標
        localRotation = t.rotation,    // ✅ world回転
        localScale = t.localScale,     // scaleはそのままでOK
    };

    /// <summary>
    /// DTO -> Transform
    /// </summary>
    public static void ApplyTransform(this Transform t, in SimpleTransform st, float duration)
    {
        t.DOKill();  // 前フレームのTweenをキャンセル

        t.DOMove(st.localPosition, duration)
            .SetEase(Ease.Linear);

        t.DORotateQuaternion(st.localRotation, duration)
            .SetEase(Ease.Linear);

        t.DOScale(st.localScale, duration)
            .SetEase(Ease.Linear);
    }
}