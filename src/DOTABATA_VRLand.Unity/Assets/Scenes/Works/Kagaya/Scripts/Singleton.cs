using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static bool isQuitting = false;

    /// <summary>
    /// シーンロード時に破棄するか
    /// </summary>
    protected virtual bool IsDontDestroyOnLoad => true;

    /// <summary>
    /// 排他制御用
    /// </summary>
    private static readonly object _lock = new object();

    private static T i;

    /// <summary>
    /// インスタンス
    /// </summary>
    public static T I {
        get {
            // アプリ終中は新規生成しない
            if (isQuitting) {
                return null;
            }

            if (i != null) {
                return i;
            }

            lock (_lock) {
                if(i != null) {
                    return i;
                }

                // シーン上から探す
                i = FindAnyObjectByType<T>();
                if (i != null) {
                    return i;
                }

                // 無ければ生成
                GameObject obj = new GameObject(typeof(T).Name);
                return i = obj.AddComponent<T>();
            }
        }
    }

    /// <summary>
    /// 既存インスタンスの整合性チェックと重複排除。
    /// </summary>
    protected virtual void Awake() {
        if (i == null) {
            i = this as T;

            if (IsDontDestroyOnLoad) {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        else if (i != this) {
            // 2つ目以降を破棄して一意性を担保
            Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} detected. Destroying the new one on {name}.");

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 終了時にアクセスを止める
    /// </summary>
    protected virtual void OnApplicationQuit() {
        isQuitting = true;
    }

    /// <summary>
    /// 破棄時にインスタンスをクリア
    /// </summary>
    protected virtual void OnDestroy() {
        if (i == this) {
            i = null;
        }
    }
}
