using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/ServerConfig")]
public class ServerConfigSO : ScriptableObject {
    [SerializeField] private List<ServerConfig> debug = new List<ServerConfig>();
    public ServerConfig DEBUG {
        get { return debug.FirstOrDefault(_=> _.use); }
    }

    [SerializeField] private List<ServerConfig> production = new List<ServerConfig>();
    public ServerConfig PRODUCTION {
        get { return production.FirstOrDefault(_ => _.use); }
    }

    private List<ServerConfig> saveDebug;
    private List<ServerConfig> saveProduction;

    private void OnValidate() {
        ValidateDebug();
        ValidateProduction();
    }

    private void ValidateDebug() {
        if (debug.Count(_ => _.use) <= 1 ||
            debug.Count() != saveDebug.Count()) {
            if (debug.Count() > 1) {
                debug.Last().use = false;
            }
            saveDebug = debug.Select(_ => _.DeepCopy()).ToList();
            return;
        }

        int index = saveDebug.FindIndex(_ => _.use);
        if (index < 0 || index > debug.Count() - 1) {
            debug.All(_ => _.use = false);
            saveDebug = debug.Select(_ => _.DeepCopy()).ToList();
            return;
        }

        debug[index].use = false;
        saveDebug = debug.Select(_ => _.DeepCopy()).ToList();
    }

    private void ValidateProduction() {
        if (production.Count(_ => _.use) <= 1 ||
            production.Count() != saveProduction.Count()) {
            if (production.Count() > 1) {
                production.Last().use = false;
            }
            saveProduction = production.Select(_ => _.DeepCopy()).ToList();
            return;
        }
            int index = saveProduction.FindIndex(_ => _.use);
        if (index < 0 || index > production.Count() - 1) {
            production.All(_ => _.use = false);
            saveProduction = production.Select(_ => _.DeepCopy()).ToList();
            return;
        }

        production[index].use = false;
        saveProduction = production.Select(_ => _.DeepCopy()).ToList();
    }
}

[System.Serializable]
public class ServerConfig {
    public bool use;
    public string url;

    public ServerConfig DeepCopy() {
        return new ServerConfig {
            use = this.use,
            url = this.url
        };
    }
}