using UnityEngine;

[CreateAssetMenu(fileName = "MinigameInfo", menuName = "Minigame/Info")]
public class MinigameInfo : ScriptableObject
{
    public string gameName;

    [TextArea(6, 10)]
    public string description;
}