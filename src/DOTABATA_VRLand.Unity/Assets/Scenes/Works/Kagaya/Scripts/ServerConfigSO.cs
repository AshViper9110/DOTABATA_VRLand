using UnityEngine;

[CreateAssetMenu(menuName = "Config/ServerConfig")]
public class ServerConfigSO : ScriptableObject {
    public ServerConfig DEBUG = new ServerConfig();
    public ServerConfig PRODUCTION = new ServerConfig();
}

[System.Serializable]
public class ServerConfig {
    public string url;
}