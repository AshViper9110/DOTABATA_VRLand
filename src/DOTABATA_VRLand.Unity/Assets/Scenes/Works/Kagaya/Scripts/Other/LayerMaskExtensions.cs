using UnityEngine;

public static class LayerMaskExtensions {

    private static bool IsValidLayer(int layerId) {
        return layerId >= 0 && layerId <= 31;
    }

    private static bool TryGetLayer(string layerName, out int layerId) {
        layerId = LayerMask.NameToLayer(layerName);

        if (!IsValidLayer(layerId)) {
            Debug.LogWarning($"Layer '{layerName}' does not exist.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// LayerMaskに指定したレイヤーが含まれているか
    /// </summary>
    public static bool Contains(this LayerMask self, int layerId) {
        if (!IsValidLayer(layerId)) {
            Debug.LogWarning($"Invalid layer id: {layerId}");
            return false;
        }

        return (self.value & (1 << layerId)) != 0;
    }

    /// <summary>
    /// LayerMaskに指定したレイヤーが含まれているか
    /// </summary>
    public static bool Contains(this LayerMask self, string layerName) {
        return TryGetLayer(layerName, out int layerId) &&
               self.Contains(layerId);
    }

    /// <summary>
    /// LayerMaskにレイヤーを追加
    /// </summary>
    public static LayerMask Add(this LayerMask self, int layerId) {
        if (!IsValidLayer(layerId)) {
            Debug.LogWarning($"Invalid layer id: {layerId}");
            return self;
        }

        return self.value | (1 << layerId);
    }

    /// <summary>
    /// LayerMaskにレイヤーを追加
    /// </summary>
    public static LayerMask Add(this LayerMask self, string layerName) {
        return TryGetLayer(layerName, out int layerId)
            ? self.Add(layerId)
            : self;
    }

    /// <summary>
    /// LayerMaskにレイヤーを追加/削除切替
    /// </summary>
    public static LayerMask Toggle(this LayerMask self, int layerId) {
        if (!IsValidLayer(layerId)) {
            Debug.LogWarning($"Invalid layer id: {layerId}");
            return self;
        }

        return self.value ^ (1 << layerId);
    }

    /// <summary>
    /// LayerMaskにレイヤーを追加/削除切替
    /// </summary>
    public static LayerMask Toggle(this LayerMask self, string layerName) {
        return TryGetLayer(layerName, out int layerId)
            ? self.Toggle(layerId)
            : self;
    }

    /// <summary>
    /// LayerMaskからレイヤーを削除
    /// </summary>
    public static LayerMask Remove(this LayerMask self, int layerId) {
        if (!IsValidLayer(layerId)) {
            Debug.LogWarning($"Invalid layer id: {layerId}");
            return self;
        }

        return self.value & ~(1 << layerId);
    }

    /// <summary>
    /// LayerMaskからレイヤーを削除
    /// </summary>
    public static LayerMask Remove(this LayerMask self, string layerName) {
        return TryGetLayer(layerName, out int layerId)
            ? self.Remove(layerId)
            : self;
    }
}