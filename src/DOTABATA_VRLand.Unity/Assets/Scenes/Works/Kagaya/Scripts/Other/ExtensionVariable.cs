using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 拡張変数クラス
/// </summary>
[System.Serializable]
public class ExtensionVariable<T> {
    public T value;
    private T beforeValue;

    public ExtensionVariable(T value = default) {
        this.value = value;
        this.beforeValue = value;
    }

    /// <summary>
    /// this ==> T
    /// </summary>
    public static implicit operator T(ExtensionVariable<T> extensionVariable) {
        return extensionVariable.value;
    }

    /// <summary>
    /// T ==> this
    /// </summary>
    public static implicit operator ExtensionVariable<T>(T v) {
        return new ExtensionVariable<T> {
            value = v,
        };
    }

    /// <summary>
    /// 値の変更があったら
    /// </summary>
    public void IsChanged(Action afterFunc) {
        if (!EqualityComparer<T>.Default.Equals(value, beforeValue)) {
            afterFunc?.Invoke();
            beforeValue = value;
        }
    }
}
