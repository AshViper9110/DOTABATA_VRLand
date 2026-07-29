using System.Collections.Generic;
using UnityEngine;
using static SkinManager;

[CreateAssetMenu(menuName = "PlayerSkin/SkinData")]
public class SkinDataSO : ScriptableObject {
    public SkinCategory skinCategory;
    public List<Skin> skinList = new List<Skin>();
}

[System.Serializable]
public class Skin {
    public string name;
    public Sprite spriteImage;
    public GameObject skinObject;
    public Color color;
}
