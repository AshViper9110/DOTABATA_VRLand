using UnityEngine;

public class Cuttable : MonoBehaviour
{
    [Tooltip("次に切断可能になる時刻")]
    public float nextCutTime = 0f;

    [Tooltip("切断後のクールダウン時間")]
    public float coolTime = 0.1f;
}