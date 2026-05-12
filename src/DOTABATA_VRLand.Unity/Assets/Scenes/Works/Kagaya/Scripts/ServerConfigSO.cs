using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/ServerConfig")]
public class ServerConfigSO : ScriptableObject {
    [SerializeField] private List<ServerConfig> debugs = new();
    [SerializeField] private List<ServerConfig> productions = new();

    public ServerConfig DEBUG => debugs.FirstOrDefault(_ => _.use);
    public ServerConfig PRODUCTION => productions.FirstOrDefault(_ => _.use);

    private void OnValidate() {
        Validate(debugs);
        Validate(productions);
    }

    /// <summary>
    /// チェックを一つだけに
    /// </summary>
    private void Validate(List<ServerConfig> condigs) {
        int index;

        if (debugs.Count(_ => _.use) <= 1) {
            index = debugs.FindIndex(_ => _.use);
            if (index == -1) {
                return;
            }

            foreach (var item in debugs) {
                item.saveUse = false;
            }

            debugs[index].saveUse = true;

            return;
        }

        index = debugs.FindIndex(_ => _.saveUse);
        if (index < 0 || index > debugs.Count() - 1) {
            foreach (var item in debugs) {
                item.use = false;
            }

            index = debugs.FindIndex(_ => _.use);
            if (index == -1) {
                return;
            }

            foreach (var item in debugs) {
                item.saveUse = false;
            }

            debugs[index].saveUse = true;

            return;
        }

        debugs[index].use = false;

        index = debugs.FindIndex(_ => _.use);
        if (index == -1) {
            return;
        }

        foreach (var item in debugs) {
            item.saveUse = false;
        }

        debugs[index].saveUse = true;
    }
}

[System.Serializable]
public class ServerConfig {
    public bool use;
    [HideInInspector] public bool saveUse = false;
    public string url;
}