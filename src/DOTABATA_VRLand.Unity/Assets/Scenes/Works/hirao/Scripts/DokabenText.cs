using System.Collections;
using UnityEngine;

public class DokabenText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform target;

    [Header("Animation")]
    [SerializeField] private float duration = 0.5f;

    private Coroutine animationCoroutine;

    private void Start()
    {
        Play();
    }

    /// <summary>
    /// UIを90度倒した状態から起き上がらせる
    /// </summary>
    public void Play()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(RotateUp());
    }

    private IEnumerator RotateUp()
    {
        // 開始角度：90度倒れている
        target.localRotation = Quaternion.Euler(90f, 0f, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // SmoothStepで自然に起き上がる
            t = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(90f, 0f, t);

            target.localRotation = Quaternion.Euler(angle, 0f, 0f);

            yield return null;
        }

        // 最終位置を確実に0度にする
        target.localRotation = Quaternion.Euler(0f, 0f, 0f);

        animationCoroutine = null;
    }
}