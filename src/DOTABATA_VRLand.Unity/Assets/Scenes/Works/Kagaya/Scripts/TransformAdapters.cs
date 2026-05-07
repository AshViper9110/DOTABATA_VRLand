using DG.Tweening;
using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;

public static class TransformAdapters {
    /// <summary>
    /// Transform -> DTO
    /// </summary>
    public static SimpleTransform ToSimpleTransform(this Transform t) =>
        new SimpleTransform {
            localPosition = t.localPosition,
            localRotation = t.localRotation,
            localScale = t.localScale,
        };

    /// <summary>
    /// DTO ->Transform
    /// </summary>
    public static void ApplyTransform(this Transform t, in SimpleTransform st, float duration) {
        duration *= 2;
        t.DOMove(st.localPosition, duration).SetEase(Ease.InOutQuad);
        t.DOLocalRotateQuaternion(st.localRotation, duration).SetEase(Ease.InOutQuad);
        t.DOScale(st.localScale, duration).SetEase(Ease.InOutQuad);
    }
}
