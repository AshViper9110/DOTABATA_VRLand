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
    private void Validate(List<ServerConfig> list) {
        int index;

        if (list.Count(_ => _.use) <= 1) {
            index = list.FindIndex(_ => _.use);
            if (index == -1) {
                return;
            }

            foreach (var item in list) {
                item.saveUse = false;
            }

            list[index].saveUse = true;

            return;
        }

        index = list.FindIndex(_ => _.saveUse);
        if (index < 0 || index > list.Count() - 1) {
            foreach (var item in list) {
                item.use = false;
            }

            index = list.FindIndex(_ => _.use);
            if (index == -1) {
                return;
            }

            foreach (var item in list) {
                item.saveUse = false;
            }

            list[index].saveUse = true;

            return;
        }

        list[index].use = false;

        index = list.FindIndex(_ => _.use);
        if (index == -1) {
            return;
        }

        foreach (var item in list) {
            item.saveUse = false;
        }

        list[index].saveUse = true;
    }
}

[System.Serializable]
public class ServerConfig {
    public bool use;
    [HideInInspector] public bool saveUse = false;
    public string url;
}